using System.Globalization;
using XCryptoKeys.Publisher;

// xck-publisher : publie sur X les posts dus des campagnes actives de chaque compte de accounts/.
// À lancer toutes les 15 minutes (cron ou Planificateur de tâches). Voir src/README.md.

string? settingsPath = null;
string? only = null;
string? onlyAccount = null;
bool dryRun = false;
DateTimeOffset now = DateTimeOffset.UtcNow;

for (int i = 0; i < args.Length; i++)
{
    switch (args[i])
    {
        case "--dry-run": dryRun = true; break;
        case "--settings" when i + 1 < args.Length: settingsPath = args[++i]; break;
        case "--campaign" when i + 1 < args.Length: only = args[++i]; break;
        case "--account" when i + 1 < args.Length: onlyAccount = args[++i]; break;
        case "--now" when i + 1 < args.Length:
            now = DateTimeOffset.Parse(args[++i], CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
            break;
        case "-h" or "--help":
            Console.WriteLine("""
                xck-publisher [--dry-run] [--account <compte>] [--campaign <nom>] [--settings <fichier>] [--now <date ISO>]

                  --dry-run    affiche ce qui serait publié, sans appel à X ni modification du dépôt
                  --account    ne traite qu'un compte (ex. XCryptoKeys, nom du dossier dans accounts/)
                  --campaign   ne traite qu'une campagne (ex. bitcoin-001, ou XCryptoKeys/bitcoin-001)
                  --settings   fichier de settings (défaut : appsettings.json à côté de l'exécutable,
                               puis appsettings.local.json par-dessus)
                  --now        simule l'heure courante (UTC), pour tester le calendrier
                """);
            return 0;
        default:
            Console.Error.WriteLine($"Argument inconnu : {args[i]} (voir --help)");
            return 2;
    }
}

// --campaign XCryptoKeys/bitcoin-001 : compte et campagne en une fois.
if (only is not null && only.Split('/', '\\') is [var accountPart, var campaignPart])
{
    onlyAccount ??= accountPart;
    only = campaignPart;
}

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

using var log = new Log(settings.Log.Directory, settings.Log.RetentionDays);
var problems = settings.Validate().ToList();
if (problems.Count > 0)
{
    foreach (var p in problems) log.Error($"Settings : {p}");
    return 2;
}

// Un seul passage à la fois (si un passage dure plus que l'intervalle du planificateur).
string lockPath = Path.Combine(Path.GetTempPath(), "xck-publisher.lock");
FileStream lockFile;
try
{
    lockFile = new FileStream(lockPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
}
catch (IOException)
{
    log.Warn("Un autre passage est en cours ; abandon.");
    return 0;
}

using (lockFile)
{
    log.Info($"Début du passage{(dryRun ? " (dry-run)" : "")}, heure de référence {now:yyyy-MM-ddTHH:mm:ssZ}, version par défaut « {settings.DefaultVersion} ».");

    Git? git = null;
    if (settings.Git.Enabled && !dryRun)
    {
        git = new Git(settings.RepoPath, settings.Git, log);
        if (!git.Pull(settings.AccountsFolder))
        {
            // Sans dépôt à jour on risque de republier un post : on ne publie rien.
            log.Error("git pull impossible ; aucun post publié pendant ce passage.");
            return 1;
        }
    }

    // Après le pull, pour voir les comptes et campagnes poussés depuis le dernier passage.
    if (!Directory.Exists(settings.AccountsPath))
    {
        log.Error($"Settings : dossier des comptes introuvable : {settings.AccountsPath}");
        return 2;
    }
    var accounts = Account.Discover(settings.AccountsPath)
        .Where(a => onlyAccount is null || string.Equals(a.Name, onlyAccount, StringComparison.OrdinalIgnoreCase))
        .ToList();
    if (onlyAccount is not null && accounts.Count == 0)
    {
        log.Error($"Compte introuvable dans {settings.AccountsFolder}/ : {onlyAccount}");
        return 1;
    }
    if (only is not null && !accounts.Any(a => a.HasCampaign(only)))
    {
        log.Error($"Campagne introuvable : {only}");
        return 1;
    }

    var options = new RunOptions(dryRun, now, only);
    int published = 0;
    int calls = 0;
    using var cts = new CancellationTokenSource();
    Console.CancelKeyPress += (_, e) => { e.Cancel = true; cts.Cancel(); };
    try
    {
        foreach (var account in accounts)
        {
            try
            {
                settings.LoadAccountFiles(account);
            }
            catch (Exception e) when (e is InvalidDataException or IOException or UnauthorizedAccessException or System.Text.Json.JsonException)
            {
                // Un compte mal configuré ne bloque pas les autres.
                log.Error($"[{account.Name}] réglages du compte illisibles ({e.Message}) ; compte ignoré.");
                continue;
            }

            if (!settings.AccountSettingsFor(account.Name).Enabled)
            {
                log.Info($"[{account.Name}] compte désactivé (enabled = false dans {Settings.AccountFile} ou Accounts.{account.Name}.Enabled).");
                continue;
            }

            XClient? x = null;
            if (!dryRun)
            {
                var missing = settings.MissingCredentials(account.Name).ToList();
                if (missing.Count > 0)
                {
                    // Un compte pas encore configuré ne bloque pas les autres.
                    if (account.HasActiveCampaign())
                        log.Error($"[{account.Name}] identifiants X manquants ({string.Join(", ", missing)}) dans {settings.AccountsFolder}/{account.Name}/{Settings.AccountLocalFile} ; compte ignoré.");
                    else
                        log.Info($"[{account.Name}] pas d'identifiants X et aucune campagne active ; compte ignoré.");
                    continue;
                }
                x = new XClient(settings.XFor(account.Name), log);
            }

            using (x)
            {
                published += await new Publisher(settings, account, log, x, git, options).RunAsync(cts.Token);
                calls += x?.Calls ?? 0;
            }
        }
    }
    finally
    {
        if (git is not null && !git.Push())
            log.Error("git push impossible ; les commits seront poussés au prochain passage.");
    }

    log.Info($"Fin du passage : {published} post(s) {(dryRun ? "à publier" : "publié(s)")}, {calls} appel(s) à l'API X, {log.Errors} erreur(s), {log.Warnings} avertissement(s).");
    return log.Errors > 0 ? 1 : 0;
}
