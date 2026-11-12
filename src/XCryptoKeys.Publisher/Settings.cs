using System.Text.Json;
using System.Text.Json.Nodes;

namespace XCryptoKeys.Publisher;

public sealed class Settings
{
    /// <summary>Chemin du clone local du dépôt.</summary>
    public string RepoPath { get; set; } = "";
    /// <summary>Dossier des comptes, relatif à RepoPath : un sous-dossier par compte X, avec ses campagnes dans threads/.</summary>
    public string AccountsFolder { get; set; } = "accounts";
    /// <summary>Version de texte utilisée par défaut : simple, tech, variant…</summary>
    public string DefaultVersion { get; set; } = "simple";
    public bool AttachImages { get; set; } = true;
    /// <summary>Pose le texte alternatif des images (un appel API de plus par image).</summary>
    public bool AltText { get; set; } = true;
    public int MaxWeightedLength { get; set; } = 280;
    public int DelayBetweenPostsSeconds { get; set; } = 30;
    public LogSettings Log { get; set; } = new();
    public GitSettings Git { get; set; } = new();
    public ServiceSettings Service { get; set; } = new();
    /// <summary>Réglages communs de l'API X (ApiBaseUrl). Les identifiants sont par compte, dans Accounts.</summary>
    public XSettings X { get; set; } = new();
    /// <summary>Réglages par compte, indexés par le nom du dossier dans accounts/ (ex. « XCryptoKeys »).</summary>
    public Dictionary<string, AccountSettings> Accounts { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    public string AccountsPath => Path.Combine(RepoPath, AccountsFolder);

    public sealed class AccountSettings
    {
        /// <summary>false : le compte est ignoré (rien n'est publié, pas d'erreur).</summary>
        public bool Enabled { get; set; } = true;
        /// <summary>Username et identifiants OAuth du compte. ApiBaseUrl est pris dans la section X commune.</summary>
        public XSettings X { get; set; } = new();
    }

    /// <summary>Fichier du compte, commité (Enabled, X.Username ; jamais de clés).</summary>
    public const string AccountFile = "account.json";
    /// <summary>Fichier des clés X du compte, sur le serveur seulement (ignoré par git).</summary>
    public const string AccountLocalFile = "account.local.json";

    /// <summary>Settings fusionnés bruts (fichiers + variables d'environnement), gardés pour fusionner les fichiers des comptes en dessous.</summary>
    [System.Text.Json.Serialization.JsonIgnore]
    public JsonObject? Raw { get; set; }

    private static readonly JsonDocumentOptions DocumentOptions = new() { CommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true };
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString,
    };

    /// <summary>
    /// Charge les réglages du compte depuis son dossier : account.json, puis account.local.json (clés X),
    /// puis la section Accounts.&lt;compte&gt; des settings et des variables d'environnement par-dessus.
    /// Un fichier illisible ou invalide lève une exception (le compte est alors ignoré par l'appelant).
    /// </summary>
    public void LoadAccountFiles(Account account)
    {
        JsonObject merged = new();
        foreach (string name in new[] { AccountFile, AccountLocalFile })
        {
            string file = Path.Combine(account.Directory, name);
            if (!File.Exists(file)) continue;
            JsonNode? node;
            try { node = JsonNode.Parse(File.ReadAllText(file), documentOptions: DocumentOptions); }
            catch (JsonException e) { throw new InvalidDataException($"{name} invalide : {e.Message}", e); }
            if (node is not JsonObject obj) throw new InvalidDataException($"{name} doit contenir un objet JSON.");
            Merge(merged, obj);
        }

        JsonObject? section = null;
        if (Raw is not null)
        {
            var accounts = Raw.FirstOrDefault(p => string.Equals(p.Key, nameof(Accounts), StringComparison.OrdinalIgnoreCase)).Value as JsonObject;
            section = accounts?.FirstOrDefault(p => string.Equals(p.Key, account.Name, StringComparison.OrdinalIgnoreCase)).Value as JsonObject;
        }
        else if (Accounts.FirstOrDefault(kv => string.Equals(kv.Key, account.Name, StringComparison.OrdinalIgnoreCase)).Value is { } typed)
        {
            // Settings construits en code (sans Load) : seules les valeurs renseignées priment sur les fichiers.
            section = new JsonObject();
            if (!typed.Enabled) section["Enabled"] = false;
            var x = new JsonObject();
            foreach (var (k, v) in new[] { ("Username", typed.X.Username), ("ApiKey", typed.X.ApiKey), ("ApiKeySecret", typed.X.ApiKeySecret), ("AccessToken", typed.X.AccessToken), ("AccessTokenSecret", typed.X.AccessTokenSecret) })
                if (!string.IsNullOrWhiteSpace(v)) x[k] = v;
            section["X"] = x;
        }
        if (section is not null) Merge(merged, section);

        var settings = merged.Deserialize<AccountSettings>(SerializerOptions) ?? new AccountSettings();
        foreach (string key in Accounts.Keys.Where(k => string.Equals(k, account.Name, StringComparison.OrdinalIgnoreCase)).ToList())
            Accounts.Remove(key);
        Accounts[account.Name] = settings;
    }

    /// <summary>Réglages du compte (nom insensible à la casse) ; des réglages vides s'il n'est pas configuré.</summary>
    public AccountSettings AccountSettingsFor(string account) =>
        Accounts.FirstOrDefault(kv => string.Equals(kv.Key, account, StringComparison.OrdinalIgnoreCase)).Value ?? new AccountSettings();

    /// <summary>
    /// Réglages X d'un compte : ses identifiants à lui (jamais ceux d'un autre compte), l'ApiBaseUrl commun,
    /// et le nom du dossier comme Username s'il n'est pas renseigné.
    /// </summary>
    public XSettings XFor(string account)
    {
        var a = AccountSettingsFor(account).X;
        return new XSettings
        {
            ApiBaseUrl = X.ApiBaseUrl,
            Username = string.IsNullOrWhiteSpace(a.Username) ? account : a.Username,
            ApiKey = a.ApiKey,
            ApiKeySecret = a.ApiKeySecret,
            AccessToken = a.AccessToken,
            AccessTokenSecret = a.AccessTokenSecret,
        };
    }

    /// <summary>Vrai si la valeur est vide ou encore un texte d'exemple comme « &lt;API Key&gt; » : elle n'est jamais envoyée à X.</summary>
    public static bool IsUnset(string? value) =>
        string.IsNullOrWhiteSpace(value) || (value.Trim().StartsWith('<') && value.Trim().EndsWith('>'));

    /// <summary>Identifiants X manquants pour un compte (vide si tout est là). Un texte d'exemple compte comme manquant.</summary>
    public IEnumerable<string> MissingCredentials(string account)
    {
        var x = XFor(account);
        if (IsUnset(x.ApiKey)) yield return "ApiKey";
        if (IsUnset(x.ApiKeySecret)) yield return "ApiKeySecret";
        if (IsUnset(x.AccessToken)) yield return "AccessToken";
        if (IsUnset(x.AccessTokenSecret)) yield return "AccessTokenSecret";
    }

    /// <summary>Vrai si aucune des 4 clés n'est renseignée : le compte attend ses clés (état normal, pas une erreur).</summary>
    public bool HasNoCredentials(string account) => MissingCredentials(account).Count() == 4;

    public sealed class LogSettings
    {
        /// <summary>Dossier des logs ; relatif au dossier de l'exécutable s'il n'est pas absolu.</summary>
        public string Directory { get; set; } = "logs";
        public int RetentionDays { get; set; } = 30;
        /// <summary>true : chaque passage est écrit dans le fichier, même sans rien à faire (pour vérifier la tâche planifiée).</summary>
        public bool Verbose { get; set; } = false;
    }

    public sealed class GitSettings
    {
        public bool Enabled { get; set; } = true;
        /// <summary>
        /// true : pull au début du passage, push à la fin. false : git local seulement (commits locaux, jamais de pull ni de
        /// push) ; la synchronisation avec GitHub se fait à la main (src\sync.cmd). Le service Windows est installé en false.
        /// </summary>
        public bool Sync { get; set; } = true;
        public string Remote { get; set; } = "origin";
        /// <summary>Branche à pousser ; vide = branche courante.</summary>
        public string Branch { get; set; } = "";
        public string UserName { get; set; } = "";
        public string UserEmail { get; set; } = "";
        public int PushRetries { get; set; } = 3;
        /// <summary>Commande ssh pour git (GIT_SSH_COMMAND), ex. la clé de déploiement du service ; vide = réglages git habituels.</summary>
        public string SshCommand { get; set; } = "";
        /// <summary>Chemin de git ; « git » = celui du PATH. Le service LocalService n'a pas le PATH de l'utilisateur : install.ps1 met le chemin complet.</summary>
        public string Executable { get; set; } = "git";
    }

    public sealed class ServiceSettings
    {
        /// <summary>Mode --service : un passage toutes les N minutes, aligné sur l'heure (:00, :15, :30, :45 pour 15).</summary>
        public int IntervalMinutes { get; set; } = 15;
    }

    public sealed class XSettings
    {
        public string ApiBaseUrl { get; set; } = "https://api.x.com";
        public string Username { get; set; } = "";
        public string ApiKey { get; set; } = "";
        public string ApiKeySecret { get; set; } = "";
        public string AccessToken { get; set; } = "";
        public string AccessTokenSecret { get; set; } = "";
    }

    /// <summary>
    /// Charge appsettings.json, puis appsettings.local.json (secrets, ignoré par git) par-dessus,
    /// puis les variables d'environnement XCK__Section__Cle (ex. XCK__Accounts__XCryptoKeys__X__ApiKey).
    /// </summary>
    public static Settings Load(string? explicitPath)
    {
        JsonObject merged = new();
        IEnumerable<string> files;
        if (explicitPath is not null)
        {
            if (!File.Exists(explicitPath))
                throw new FileNotFoundException($"Fichier de settings introuvable : {explicitPath}");
            string dir = Path.GetDirectoryName(Path.GetFullPath(explicitPath))!;
            string local = Path.Combine(dir, Path.GetFileNameWithoutExtension(explicitPath) + ".local.json");
            files = [explicitPath, local];
        }
        else
        {
            string dir = AppContext.BaseDirectory;
            files = [Path.Combine(dir, "appsettings.json"), Path.Combine(dir, "appsettings.local.json")];
        }

        foreach (string file in files)
        {
            if (!File.Exists(file)) continue;
            var node = JsonNode.Parse(File.ReadAllText(file), documentOptions: DocumentOptions);
            if (node is JsonObject obj) Merge(merged, obj);
        }

        foreach (System.Collections.DictionaryEntry e in Environment.GetEnvironmentVariables())
        {
            string key = (string)e.Key;
            if (!key.StartsWith("XCK__", StringComparison.OrdinalIgnoreCase)) continue;
            string[] path = key["XCK__".Length..].Split("__");
            SetPath(merged, path, (string?)e.Value ?? "");
        }

        var settings = merged.Deserialize<Settings>(SerializerOptions) ?? new Settings();
        settings.Raw = merged;
        return settings;
    }

    /// <summary>Contrôles globaux. Les identifiants X sont vérifiés compte par compte au moment du passage.</summary>
    public IEnumerable<string> Validate()
    {
        if (string.IsNullOrWhiteSpace(RepoPath)) yield return "RepoPath est vide.";
        else if (!Directory.Exists(RepoPath)) yield return $"Dépôt introuvable : {RepoPath}";
        // AccountsPath est vérifié après le git pull : un clone resté sur l'ancienne organisation doit pouvoir se mettre à jour.
        if (string.IsNullOrWhiteSpace(DefaultVersion)) yield return "DefaultVersion est vide.";
        if (!string.IsNullOrWhiteSpace(X.ApiKey) || !string.IsNullOrWhiteSpace(X.AccessToken))
            yield return "Les identifiants X ne vont plus dans la section X mais dans accounts/<compte>/account.local.json (voir src/README.md).";
    }

    private static void Merge(JsonObject target, JsonObject source)
    {
        foreach (var (key, value) in source)
        {
            string? existingKey = target.Select(p => p.Key).FirstOrDefault(k => string.Equals(k, key, StringComparison.OrdinalIgnoreCase));
            if (value is JsonObject srcObj && existingKey is not null && target[existingKey] is JsonObject dstObj)
            {
                Merge(dstObj, srcObj);
                continue;
            }
            if (existingKey is not null) target.Remove(existingKey);
            target[key] = value?.DeepClone();
        }
    }

    private static void SetPath(JsonObject root, string[] path, string value)
    {
        JsonObject current = root;
        for (int i = 0; i < path.Length; i++)
        {
            string? existingKey = current.Select(p => p.Key).FirstOrDefault(k => string.Equals(k, path[i], StringComparison.OrdinalIgnoreCase));
            string key = existingKey ?? path[i];
            if (i == path.Length - 1)
            {
                // true/false deviennent des booléens ; les nombres restent des chaînes
                // (AllowReadingFromString les lit pour les champs int, et une clé numérique reste une chaîne).
                JsonNode node = bool.TryParse(value, out bool b) ? JsonValue.Create(b) : JsonValue.Create(value);
                current[key] = node;
                return;
            }
            if (current[key] is not JsonObject next)
            {
                next = new JsonObject();
                current[key] = next;
            }
            current = next;
        }
    }
}
