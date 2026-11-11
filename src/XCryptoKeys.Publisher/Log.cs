namespace XCryptoKeys.Publisher;

/// <summary>Log minimal : console + fichier quotidien (publisher-AAAA-MM-JJ.log), avec purge des vieux fichiers.</summary>
public sealed class Log : IDisposable
{
    private readonly StreamWriter? _file;
    private readonly object _lock = new();
    public int Errors { get; private set; }
    public int Warnings { get; private set; }

    public Log(string? directory, int retentionDays)
    {
        if (string.IsNullOrWhiteSpace(directory)) return;
        string dir = Path.IsPathRooted(directory) ? directory : Path.Combine(AppContext.BaseDirectory, directory);
        Directory.CreateDirectory(dir);
        string path = Path.Combine(dir, $"publisher-{DateTime.UtcNow:yyyy-MM-dd}.log");
        _file = new StreamWriter(new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.ReadWrite)) { AutoFlush = true };
        if (retentionDays > 0)
        {
            foreach (var f in Directory.GetFiles(dir, "publisher-*.log"))
            {
                try
                {
                    if (File.GetLastWriteTimeUtc(f) < DateTime.UtcNow.AddDays(-retentionDays)) File.Delete(f);
                }
                catch (IOException) { }
            }
        }
    }

    public void Info(string message) => Write("INFO ", message);
    public void Warn(string message) { Warnings++; Write("WARN ", message); }
    public void Error(string message) { Errors++; Write("ERROR", message); }

    private void Write(string level, string message)
    {
        string line = $"{DateTime.UtcNow:yyyy-MM-ddTHH:mm:ssZ} {level} {message}";
        lock (_lock)
        {
            if (level == "ERROR") Console.Error.WriteLine(line); else Console.WriteLine(line);
            _file?.WriteLine(line);
        }
    }

    public void Dispose() => _file?.Dispose();
}
