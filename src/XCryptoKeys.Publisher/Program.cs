using System.Globalization;
using XCryptoKeys.Publisher;

// xck-publisher : publie sur X les posts dus des campagnes actives du dépôt XCryptoKeys.
// À lancer toutes les 15 minutes (cron ou Planificateur de tâches). Voir src/README.md.

string? settingsPath = null;
string? only = null;
bool dryRun = false;
DateTimeOffset now = DateTimeOffset.UtcNow;

for (int i = 0; i < args.Length; i++)
{
    switch (args[i])
    {
        case "--dry-run": dryRun = true; break;
        case "--settings" when i + 1 < args.Length: settingsPath = args[++i]; break;
        case "--campaign" when i + 1 < args.Length: only = args[++i]; break;
        case "--now" when i + 1 < args.Length:
            now = DateTimeOffset.Parse(args[++i], CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
            break;
        case "-h" or "--help":
            Console.WriteLine("""
                xck-publisher [--dry-run] [--campaign <nom>] [--settings <fichier>] [--now <date ISO>]

                  --dry-run    affiche ce qui serait publié, sans appel à X ni modification du dépôt
                  --campaign   ne traite qu'une campagne (ex. bitcoin-001)
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
var problems = settings.Validate(dryRun).ToList();
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
        if (!git.Pull(settings.ThreadsFolder))
        {
            // Sans dépôt à jour on risque de republier un post : on ne publie rien.
            log.Error("git pull impossible ; aucun post publié pendant ce passage.");
            return 1;
        }
    }

    using var x = dryRun ? null : new XClient(settings.X, log);
    var publisher = new Publisher(settings, log, x, git, new RunOptions(dryRun, now, only));

    int published;
    using var cts = new CancellationTokenSource();
    Console.CancelKeyPress += (_, e) => { e.Cancel = true; cts.Cancel(); };
    try
    {
        published = await publisher.RunAsync(cts.Token);
    }
    finally
    {
        if (git is not null && !git.Push())
            log.Error("git push impossible ; les commits seront poussés au prochain passage.");
    }

    log.Info($"Fin du passage : {published} post(s) {(dryRun ? "à publier" : "publié(s)")}, {x?.Calls ?? 0} appel(s) à l'API X, {log.Errors} erreur(s), {log.Warnings} avertissement(s).");
    return log.Errors > 0 ? 1 : 0;
}
