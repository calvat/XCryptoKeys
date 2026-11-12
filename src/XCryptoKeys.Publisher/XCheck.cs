using System.Net;
using System.Text.Json.Nodes;

namespace XCryptoKeys.Publisher;

/// <summary>
/// Compte dont X a refusé les clés (401…) : mis en pause d'un passage à l'autre, pour ne pas dépenser un appel à l'API
/// toutes les 15 minutes, jusqu'à ce que account.local.json change (ou que le service redémarre, ou --check-x réussisse).
/// </summary>
public static class XAuthBlock
{
    private static string FilePath(string logDir, string account) => Path.Combine(logDir, $"x-auth-failed-{account}.txt");

    /// <summary>Empreinte de account.local.json : date de modification et taille.</summary>
    private static string Stamp(Account a)
    {
        var fi = new FileInfo(Path.Combine(a.Directory, Settings.AccountLocalFile));
        return fi.Exists ? $"{fi.LastWriteTimeUtc.Ticks}:{fi.Length}" : "absent";
    }

    /// <summary>Raison de la pause si le compte est en pause et que ses clés n'ont pas changé ; null sinon.</summary>
    public static string? Blocked(string? logDir, Account a)
    {
        if (logDir is null) return null;
        string f = FilePath(logDir, a.Name);
        if (!File.Exists(f)) return null;
        try
        {
            var lines = File.ReadAllLines(f);
            if (lines.Length > 0 && lines[0] == Stamp(a)) return lines.Length > 1 ? lines[1] : "clés refusées";
            File.Delete(f);  // account.local.json a changé : on réessaie
        }
        catch (IOException) { }
        return null;
    }

    public static void Block(string? logDir, Account a, string reason)
    {
        if (logDir is null) return;
        try
        {
            Directory.CreateDirectory(logDir);
            File.WriteAllLines(FilePath(logDir, a.Name), [Stamp(a), $"{DateTime.UtcNow:yyyy-MM-ddTHH:mm:ssZ} {reason}"]);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { }
    }

    public static void Clear(string? logDir, string account)
    {
        if (logDir is null) return;
        try { File.Delete(FilePath(logDir, account)); } catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { }
    }

    public static void ClearAll(string? logDir)
    {
        if (logDir is null || !Directory.Exists(logDir)) return;
        foreach (var f in Directory.GetFiles(logDir, "x-auth-failed-*.txt"))
        {
            try { File.Delete(f); } catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { }
        }
    }
}

/// <summary>
/// xck-publisher --check-x [--account Compte] : vérifie les clés X de chaque compte par un appel en lecture seule
/// (GET /2/users/me), sans rien publier. Les clés ne sont jamais affichées, seulement leur forme.
/// </summary>
public static class XCheck
{
    public static int Run(string? settingsPath, string? onlyAccount)
    {
        Settings settings;
        try
        {
            settings = Settings.Load(settingsPath);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Settings illisibles : {ex.Message}");
            return 2;
        }
        if (!Directory.Exists(settings.AccountsPath))
        {
            Console.Error.WriteLine($"Dossier des comptes introuvable : {settings.AccountsPath}");
            return 2;
        }
        var accounts = Account.Discover(settings.AccountsPath)
            .Where(a => onlyAccount is null || string.Equals(a.Name, onlyAccount, StringComparison.OrdinalIgnoreCase))
            .ToList();
        if (accounts.Count == 0)
        {
            Console.Error.WriteLine($"Aucun compte{(onlyAccount is null ? "" : $" « {onlyAccount} »")} dans {settings.AccountsPath}");
            return 1;
        }
        string? logDir = Log.ResolveDirectory(settings.Log.Directory);
        bool allOk = true;
        foreach (var a in accounts)
        {
            var (ok, lines) = CheckAccountAsync(settings, a).GetAwaiter().GetResult();
            Console.WriteLine($"== [{a.Name}] {(ok ? "OK" : "KO")}");
            foreach (string l in lines) Console.WriteLine($"  {l}");
            if (ok) XAuthBlock.Clear(logDir, a.Name);  // clés bonnes : le compte reprend au passage suivant
            allOk &= ok;
        }
        return allOk ? 0 : 1;
    }

    /// <summary>Forme d'une clé, sans la révéler : longueur, espaces autour, ressemblance avec un autre type de jeton.</summary>
    public static IEnumerable<string> DescribeKey(string name, string raw)
    {
        string v = raw ?? "";
        string t = v.Trim();
        var notes = new List<string>();
        if (t.Length == 0) notes.Add("VIDE");
        if (v.Length != t.Length) notes.Add("espaces ou retour à la ligne autour (retirés automatiquement)");
        if (Settings.IsUnset(t) && t.Length > 0) notes.Add("encore le texte d'exemple");
        if (t.StartsWith("AAAA") && t.Length > 80) notes.Add("ressemble à un Bearer Token (OAuth 2.0 « app-only ») : ce n'est pas la bonne clé");
        if (t.EndsWith("OjE6Y2k", StringComparison.Ordinal)) notes.Add("ressemble à un Client ID OAuth 2.0 : ce n'est pas la bonne clé");
        if (name == "AccessToken" && t.Length > 0 && !System.Text.RegularExpressions.Regex.IsMatch(t, @"^\d+-\w+$"))
            notes.Add("un Access Token commence normalement par l'identifiant numérique du compte suivi d'un tiret");
        yield return $"{name} : {t.Length} caractères{(notes.Count > 0 ? " — " + string.Join(" ; ", notes) : "")}";
    }

    public static async Task<(bool Ok, List<string> Lines)> CheckAccountAsync(Settings settings, Account account, HttpMessageHandler? handler = null)
    {
        var lines = new List<string>();
        try
        {
            settings.LoadAccountFiles(account);
        }
        catch (Exception ex) when (ex is InvalidDataException or IOException or UnauthorizedAccessException)
        {
            lines.Add($"réglages du compte illisibles : {ex.Message}");
            return (false, lines);
        }
        var raw = settings.AccountSettingsFor(account.Name).X;
        if (settings.HasNoCredentials(account.Name))
        {
            lines.Add($"clés X non renseignées ({Settings.AccountLocalFile} absent, vide ou resté à l'exemple) : rien à vérifier.");
            return (false, lines);
        }
        lines.AddRange(DescribeKey("ApiKey", raw.ApiKey));
        lines.AddRange(DescribeKey("ApiKeySecret", raw.ApiKeySecret));
        lines.AddRange(DescribeKey("AccessToken", raw.AccessToken));
        lines.AddRange(DescribeKey("AccessTokenSecret", raw.AccessTokenSecret));
        var missing = settings.MissingCredentials(account.Name).ToList();
        if (missing.Count > 0)
        {
            lines.Add($"clés incomplètes : {string.Join(", ", missing)}");
            return (false, lines);
        }

        var x = settings.XFor(account.Name);
        using var log = new Log(null, 0);
        using var client = new XClient(x, log, handler);
        XClient.MeResponse me;
        try
        {
            me = await client.GetMeAsync(CancellationToken.None);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            lines.Add($"API X injoignable : {ex.Message}");
            return (false, lines);
        }

        // OAuth 1.0a refuse une horloge trop décalée.
        if (me.ServerDate is { } date)
        {
            double skew = (DateTimeOffset.UtcNow - date).TotalSeconds;
            lines.Add($"horloge : écart de {skew:+0;-0} s avec X{(Math.Abs(skew) > 60 ? " — TROP GRAND : régler l'heure de Windows (synchroniser l'horloge)" : "")}");
        }
        lines.Add($"GET /2/users/me : HTTP {(int)me.Status}");

        if (me.Status == HttpStatusCode.OK)
        {
            string? username = null;
            try { username = (string?)(JsonNode.Parse(me.Body)?["data"]?["username"]); } catch (System.Text.Json.JsonException) { }
            bool match = username is not null && string.Equals(username, x.Username, StringComparison.OrdinalIgnoreCase);
            lines.Add($"compte des clés : @{username ?? "?"}{(match ? "" : $" — DIFFÉRENT du compte configuré (@{x.Username}, x.username dans {Settings.AccountFile})")}");
            if (me.AccessLevel is not null)
            {
                bool write = me.AccessLevel.Contains("write", StringComparison.OrdinalIgnoreCase);
                lines.Add($"droits du jeton : {me.AccessLevel}{(write ? "" : " — LECTURE SEULE : passer l'app en « Read and write » puis RÉGÉNÉRER l'Access Token et son secret")}");
                if (!write) return (false, lines);
            }
            return (match, lines);
        }
        if (me.Status == HttpStatusCode.Unauthorized)
        {
            lines.Add("X refuse les clés (401). Causes habituelles :");
            lines.Add("- mauvaise paire : il faut l'API Key + API Key Secret (« Consumer Keys ») et l'Access Token + Access Token Secret, pas le Bearer Token ni le Client ID / Client Secret OAuth 2.0 ;");
            lines.Add("- clés d'une autre app, ou régénérées depuis (les anciennes ne marchent plus) : recopier les 4 valeurs actuelles ;");
            lines.Add("- API Key et API Key Secret inversés, ou Access Token et son secret inversés ;");
            lines.Add("- horloge du PC décalée (voir l'écart ci-dessus).");
        }
        else if (me.Status == HttpStatusCode.Forbidden)
        {
            lines.Add("X refuse l'accès (403) : droits de l'app (« Read and write », puis régénérer l'Access Token) ou offre de l'API (projet / app non rattachés à l'offre) ;");
        }
        string detail = me.Body.Length > 300 ? me.Body[..300] : me.Body;
        if (detail.Length > 0) lines.Add($"réponse de X : {detail.ReplaceLineEndings(" ")}");
        return (false, lines);
    }
}
