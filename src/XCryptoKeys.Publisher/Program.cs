using System.Globalization;
using XCryptoKeys.Publisher;

// xck-publisher : publie sur X les posts dus des campagnes actives de chaque compte de accounts/.
// À lancer toutes les 15 minutes (service Windows avec --service, cron ou Planificateur de tâches). Voir src/README.md.

string? settingsPath = null;
string? only = null;
string? onlyAccount = null;
bool dryRun = false;
bool service = false;
bool checkGit = false;
bool checkX = false;
DateTimeOffset now = DateTimeOffset.UtcNow;

for (int i = 0; i < args.Length; i++)
{
    switch (args[i])
    {
        case "--dry-run": dryRun = true; break;
        case "--service": service = true; break;
        case "--check-git": checkGit = true; break;
        case "--check-x": checkX = true; break;
        case "--settings" when i + 1 < args.Length: settingsPath = args[++i]; break;
        case "--campaign" when i + 1 < args.Length: only = args[++i]; break;
        case "--account" when i + 1 < args.Length: onlyAccount = args[++i]; break;
        case "--now" when i + 1 < args.Length:
            now = DateTimeOffset.Parse(args[++i], CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
            break;
        case "-h" or "--help":
            Console.WriteLine("""
                xck-publisher [--dry-run] [--account <compte>] [--campaign <nom>] [--settings <fichier>] [--now <date ISO>]
                xck-publisher --service [--settings <fichier>]
                xck-publisher --check-git [--settings <fichier>]
                xck-publisher --check-x [--account <compte>] [--settings <fichier>]

                  --dry-run    affiche ce qui serait publié, sans appel à X ni modification du dépôt
                  --account    ne traite qu'un compte (ex. XCryptoKeys, nom du dossier dans accounts/)
                  --campaign   ne traite qu'une campagne (ex. bitcoin-001, ou XCryptoKeys/bitcoin-001)
                  --settings   fichier de settings (défaut : appsettings.json à côté de l'exécutable,
                               puis appsettings.local.json par-dessus)
                  --now        simule l'heure courante (UTC), pour tester le calendrier
                  --service    tourne en continu : un passage toutes les Service.IntervalMinutes minutes
                               (service Windows « XBotsPublisher », ou dans la console : Ctrl+C pour arrêter)
                  --check-git  vérifie l'accès git (lecture et écriture, sans rien pousser) ;
                               résultat dans logs/git-check.txt
                  --check-x    vérifie les clés X de chaque compte (GET /2/users/me, rien n'est publié)
                """);
            return 0;
        default:
            Console.Error.WriteLine($"Argument inconnu : {args[i]} (voir --help)");
            return 2;
    }
}

if (service) return ServiceHost.Run(settingsPath);
if (checkGit) return GitCheck.Run(settingsPath);
if (checkX) return XCheck.Run(settingsPath, onlyAccount);

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

// Le dry-run n'écrit que dans la console : ni fichier de log, ni last-run.txt.
string? logDir = dryRun ? null : Log.ResolveDirectory(settings.Log.Directory);
using var log = new Log(dryRun ? null : settings.Log.Directory, settings.Log.RetentionDays, settings.Log.Verbose);
// Une erreur inattendue est écrite dans le fichier avec tout le passage avant l'arrêt du programme.
AppDomain.CurrentDomain.UnhandledException += (_, e) =>
{
    log.Error($"Erreur inattendue, passage interrompu : {e.ExceptionObject}");
    log.Dispose();
};
var problems = settings.Validate().ToList();
if (problems.Count > 0)
{
    foreach (var p in problems) log.Error($"Settings : {p}");
    return 2;
}

// Un seul passage à la fois sur ce dépôt, quel que soit le compte Windows qui le lance (service, tâche, à la main) :
// le verrou est dans le .git du dépôt.
string gitDir = Path.Combine(settings.RepoPath, ".git");
string lockPath = Directory.Exists(gitDir)
    ? Path.Combine(gitDir, "xck-publisher.lock")
    : Path.Combine(Path.GetTempPath(), "xck-publisher.lock");
FileStream lockFile;
try
{
    lockFile = new FileStream(lockPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
}
catch (IOException)
{
    // Un passage, ou une synchronisation faite avec sync.cmd, tient le verrou : on passe notre tour, sans bruit.
    log.Info("Un autre passage ou une synchronisation est en cours ; passage suivant dans quelques minutes.");
    return 0;
}

using (lockFile)
try
{
    log.Info($"Début du passage{(dryRun ? " (dry-run)" : "")}, heure de référence {now:yyyy-MM-ddTHH:mm:ssZ}, version par défaut « {settings.DefaultVersion} ».");

    Git? git = null;
    if (settings.Git.Enabled && !dryRun)
    {
        git = new Git(settings.RepoPath, settings.Git, log);
        if (!git.StartPass(settings.AccountsFolder))
        {
            // Sans dépôt à jour (ou sans état commité) on risque de republier un post : on ne publie rien.
            log.Error(settings.Git.Sync
                ? "git pull impossible ; aucun post publié pendant ce passage."
                : "git local inutilisable (commit de récupération impossible) ; aucun post publié pendant ce passage.");
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
    // Lancé par le service : il demande l'arrêt en créant ce fichier ; le passage s'arrête entre deux campagnes.
    string? stopFile = Environment.GetEnvironmentVariable(ServiceHost.StopFileVariable);
    using var stopWatch = string.IsNullOrEmpty(stopFile) ? null
        : new Timer(_ => { if (File.Exists(stopFile)) cts.Cancel(); }, null, 1000, 1000);
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
            string keysFile = $"{settings.AccountsFolder}/{account.Name}/{Settings.AccountLocalFile}";
            var missing = settings.MissingCredentials(account.Name).ToList();
            if (settings.HasNoCredentials(account.Name))
            {
                // Aucune clé (fichier absent, vide ou resté à l'exemple) : le compte attend ses clés, ce n'est pas une erreur.
                if (dryRun)
                    log.Info($"[{account.Name}] clés X non renseignées ({keysFile}) : en vrai, rien ne serait publié pour ce compte.");
                else
                {
                    log.Info($"[{account.Name}] clés X non renseignées ({keysFile}) ; compte en attente, rien n'est publié.");
                    log.NoteWaitingForKeys(account.Name);
                    continue;
                }
            }
            else if (missing.Count > 0)
            {
                // Clés en partie renseignées : sûrement une erreur de saisie. Le compte est ignoré, les autres continuent.
                log.Error($"[{account.Name}] clés X incomplètes ({string.Join(", ", missing)} manquante(s)) dans {keysFile} ; compte ignoré.");
                continue;
            }
            else if (!dryRun)
            {
                // Clés refusées par X à un passage précédent, et pas modifiées depuis : on ne refait pas l'appel.
                if (XAuthBlock.Blocked(logDir, account) is { } reason)
                {
                    log.Info($"[{account.Name}] clés X refusées au passage précédent ({reason}) ; compte en pause jusqu'à la modification de {keysFile} (ou un --check-x réussi, ou le redémarrage du service).");
                    log.NoteAuthBlocked(account.Name);
                    continue;
                }
                x = new XClient(settings.XFor(account.Name), log);
            }

            using (x)
            {
                var publisher = new Publisher(settings, account, log, x, git, options);
                try
                {
                    published += await publisher.RunAsync(cts.Token);
                }
                finally
                {
                    calls += x?.Calls ?? 0;
                    if (publisher.AuthFailure is { } fail)
                    {
                        XAuthBlock.Block(logDir, account, $"HTTP {(int)fail.Status}");
                        log.NoteAuthBlocked(account.Name);
                    }
                }
            }
        }
    }
    catch (OperationCanceledException) when (cts.IsCancellationRequested)
    {
        log.Info("Arrêt demandé : passage interrompu entre deux campagnes (rien n'est laissé à moitié écrit).");
    }
    finally
    {
        if (git is not null && !git.EndPass())
            log.Error("git push impossible ; les commits seront poussés au prochain passage.");
    }

    log.Info($"Fin du passage : {published} post(s) {(dryRun ? "à publier" : "publié(s)")}, {calls} appel(s) à l'API X, {log.Errors} erreur(s), {log.Warnings} avertissement(s).");
    return log.Errors > 0 ? 1 : 0;
}
catch (Exception ex) when (ex is not OperationCanceledException)
{
    // Jamais de plantage du programme (code 0xE0434352) : l'erreur et sa pile vont dans le log, code 1.
    log.Error($"Erreur inattendue, passage interrompu : {ex}");
    return 1;
}
