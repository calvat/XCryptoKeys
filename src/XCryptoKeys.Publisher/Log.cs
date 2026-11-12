namespace XCryptoKeys.Publisher;

/// <summary>
/// Log du passage. Tout s'affiche dans la console. Le fichier quotidien (publisher-AAAA-MM-JJ.log) ne reçoit le passage
/// que s'il s'est passé quelque chose : une action (post publié, campagne terminée…), un avertissement ou une erreur.
/// Les lignes sont gardées en mémoire jusqu'au premier de ces événements, puis tout le passage est écrit et la suite
/// va directement dans le fichier. Un passage sans rien à faire n'écrit rien (Verbose = true : tout est écrit).
/// last-run.txt, réécrit à chaque passage, dit quand a eu lieu le dernier passage et ce qu'il a donné.
/// </summary>
public sealed class Log : IDisposable
{
    public const string LastRunFile = "last-run.txt";

    private readonly string? _dir;
    private readonly int _retentionDays;
    private readonly List<string> _pending = [];
    private readonly object _lock = new();
    private bool _flushed;
    private bool _disposed;
    private DateTimeOffset? _nextDue;
    private string? _nextDueWhat;
    private readonly List<string> _waitingForKeys = [];

    public int Errors { get; private set; }
    public int Warnings { get; private set; }
    /// <summary>Nombre de posts publiés pendant le passage.</summary>
    public int Posted { get; private set; }
    /// <summary>Vrai si le passage a fait quelque chose (post publié, campagne terminée…).</summary>
    public bool HasActivity { get; private set; }
    /// <summary>Fichier du jour (null sans dossier de log).</summary>
    public string? FilePath => _dir is null ? null : Path.Combine(_dir, $"publisher-{DateTime.UtcNow:yyyy-MM-dd}.log");
    public string? LastRunPath => _dir is null ? null : Path.Combine(_dir, LastRunFile);

    /// <param name="directory">Dossier des logs ; null ou vide = console seulement (dry-run, tests).</param>
    /// <param name="verbose">true : chaque passage est écrit dans le fichier, même sans rien à faire.</param>
    /// <summary>Dossier des logs en chemin complet (relatif = à côté de l'exécutable) ; null si vide.</summary>
    public static string? ResolveDirectory(string? directory) =>
        string.IsNullOrWhiteSpace(directory) ? null
        : Path.IsPathRooted(directory) ? directory : Path.Combine(AppContext.BaseDirectory, directory);

    public Log(string? directory, int retentionDays, bool verbose = false)
    {
        if (string.IsNullOrWhiteSpace(directory)) return;
        _dir = ResolveDirectory(directory);
        _retentionDays = retentionDays;
        if (verbose) Flush();
    }

    public void Info(string message) => Write("INFO ", message, significant: false);
    /// <summary>Une action faite (post publié, campagne terminée…) : le passage est écrit dans le fichier.</summary>
    public void Action(string message) { HasActivity = true; Write("DONE ", message, significant: true); }
    /// <summary>Un post publié : compté pour le résumé et last-run.txt.</summary>
    public void PostPublished(string message) { Posted++; Action(message); }
    public void Warn(string message) { Warnings++; Write("WARN ", message, significant: true); }
    public void Error(string message) { Errors++; FirstError ??= message; Write("ERROR", message, significant: true); }
    /// <summary>Première erreur du passage, reprise dans last-run.txt.</summary>
    public string? FirstError { get; private set; }

    /// <summary>Retient le prochain post dû le plus proche, pour last-run.txt.</summary>
    public void NoteNextDue(DateTimeOffset due, string what)
    {
        lock (_lock)
        {
            if (_nextDue is null || due < _nextDue) { _nextDue = due; _nextDueWhat = what; }
        }
    }

    private readonly List<string> _authBlocked = [];
    /// <summary>Compte dont X refuse les clés, signalé dans last-run.txt.</summary>
    public void NoteAuthBlocked(string account)
    {
        lock (_lock) { if (!_authBlocked.Contains(account)) _authBlocked.Add(account); }
    }

    /// <summary>Compte en attente de ses clés X, signalé dans last-run.txt.</summary>
    public void NoteWaitingForKeys(string account)
    {
        lock (_lock) _waitingForKeys.Add(account);
    }

    private void Write(string level, string message, bool significant)
    {
        string line = $"{DateTime.UtcNow:yyyy-MM-ddTHH:mm:ssZ} {level} {message}";
        lock (_lock)
        {
            if (level == "ERROR") Console.Error.WriteLine(line); else Console.WriteLine(line);
            if (_dir is null || _disposed) return;
            _pending.Add(line);
            if (significant || _flushed) Flush();
        }
    }

    /// <summary>Écrit dans le fichier les lignes en attente ; ensuite, chaque ligne y est écrite tout de suite.</summary>
    private void Flush()
    {
        if (_dir is null) return;
        try
        {
            Directory.CreateDirectory(_dir);
            if (!_flushed) { _flushed = true; Purge(); }
            if (_pending.Count > 0) File.AppendAllLines(FilePath!, _pending);
            _pending.Clear();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            Console.Error.WriteLine($"Log : écriture impossible dans {FilePath} : {ex.Message}");
        }
    }

    /// <summary>Résultat du passage, pour last-run.txt.</summary>
    public string Result =>
        Errors > 0 ? $"{Errors} erreur(s){(Posted > 0 ? $", {Posted} post(s) publié(s)" : "")} : {FirstError}"
        : Posted > 0 ? $"{Posted} post(s) publié(s)"
        : HasActivity ? "état mis à jour, aucun post publié"
        : Warnings > 0 ? $"{Warnings} avertissement(s), rien publié"
        : "rien à faire";

    /// <summary>Fin du passage : termine le fichier s'il a été écrit, et réécrit last-run.txt.</summary>
    public void Dispose()
    {
        lock (_lock)
        {
            if (_disposed || _dir is null) { _disposed = true; return; }
            if (_flushed)
            {
                _pending.Add("");  // ligne vide entre deux passages
                Flush();
            }
            _disposed = true;
            try
            {
                Directory.CreateDirectory(_dir);
                string next = _nextDue is null ? "" : $" ; prochain post dû : {_nextDueWhat} le {_nextDue:yyyy-MM-dd HH:mm} UTC";
                string waiting = _waitingForKeys.Count == 0 ? "" : $" ; en attente des clés X : {string.Join(", ", _waitingForKeys)}";
                string refused = _authBlocked.Count == 0 ? "" : $" ; clés X refusées par X (compte en pause, voir xck-publisher --check-x) : {string.Join(", ", _authBlocked)}";
                File.WriteAllText(LastRunPath!, $"{DateTime.UtcNow:yyyy-MM-ddTHH:mm:ssZ} {Result}{next}{waiting}{refused}{Environment.NewLine}");
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                Console.Error.WriteLine($"Log : écriture impossible de {LastRunPath} : {ex.Message}");
            }
        }
    }

    private void Purge()
    {
        if (_retentionDays <= 0) return;
        foreach (var f in Directory.GetFiles(_dir!, "publisher-*.log"))
        {
            try
            {
                if (File.GetLastWriteTimeUtc(f) < DateTime.UtcNow.AddDays(-_retentionDays)) File.Delete(f);
            }
            catch (IOException) { }
        }
    }
}
