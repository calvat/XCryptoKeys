using System.Diagnostics;
using System.Runtime.Versioning;
using System.ServiceProcess;

namespace XCryptoKeys.Publisher;

/// <summary>
/// Mode --service : service Windows « XBotsPublisher » (ou, lancé dans une console, la même boucle jusqu'à Ctrl+C).
/// Chaque passage est un processus xck-publisher à part, lancé comme à la main : les réglages, les clés X et le
/// dépôt sont relus à chaque passage, et un passage qui plante n'arrête pas le service.
/// </summary>
public static class ServiceHost
{
    public const string ServiceName = "XBotsPublisher";
    /// <summary>Variable d'environnement donnée au passage : le fichier dont la création demande l'arrêt.</summary>
    public const string StopFileVariable = "XCK_STOP_FILE";

    public static int Run(string? settingsPath)
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
        var runner = new PassRunner(settingsPath, settings);

        if (OperatingSystem.IsWindows() && !Environment.UserInteractive)
        {
            ServiceBase.Run(new WindowsService(runner));
            return 0;
        }

        // Dans une console : même boucle, Ctrl+C pour arrêter proprement.
        using var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (_, e) => { e.Cancel = true; cts.Cancel(); };
        runner.Report($"Service démarré dans la console, un passage toutes les {runner.Loop.Interval.TotalMinutes} min ; Ctrl+C pour arrêter.");
        runner.CheckGitAsync(cts.Token).GetAwaiter().GetResult();
        runner.Loop.RunAsync(cts.Token).GetAwaiter().GetResult();
        runner.Report("Service arrêté.");
        return 0;
    }
}

/// <summary>Lance un passage (processus enfant) et l'arrête proprement si le service s'arrête.</summary>
public sealed class PassRunner
{
    private static readonly TimeSpan PassTimeout = TimeSpan.FromMinutes(10);
    private static readonly TimeSpan StopGrace = TimeSpan.FromMinutes(2);
    private readonly string? _settingsPath;
    private readonly string _logDir;
    private readonly string _stopFile = Path.Combine(Path.GetTempPath(), $"xck-publisher-stop-{Environment.ProcessId}");

    public ServiceLoop Loop { get; }

    public PassRunner(string? settingsPath, Settings settings)
    {
        _settingsPath = settingsPath is null ? null : Path.GetFullPath(settingsPath);
        _logDir = Log.ResolveDirectory(string.IsNullOrWhiteSpace(settings.Log.Directory) ? "logs" : settings.Log.Directory)!;
        var interval = TimeSpan.FromMinutes(Math.Clamp(settings.Service.IntervalMinutes, 1, 1440));
        Loop = new ServiceLoop(interval, RunPassAsync, Report);
    }

    /// <summary>Événements du service (démarrage, arrêt, passage anormal) : console et logs/service.log.</summary>
    public void Report(string message)
    {
        string line = $"{DateTime.UtcNow:yyyy-MM-ddTHH:mm:ssZ} {message}";
        Console.WriteLine(line);
        try
        {
            Directory.CreateDirectory(_logDir);
            File.AppendAllText(Path.Combine(_logDir, "service.log"), line + Environment.NewLine);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { }
    }

    /// <summary>
    /// Au démarrage du service : vérifie l'accès git sous le compte du service (xck-publisher --check-git),
    /// résultat dans logs/git-check.txt et service.log. Un échec n'empêche pas le service de tourner :
    /// sans accès git, le passage ne publie rien (git pull impossible).
    /// </summary>
    public async Task CheckGitAsync(CancellationToken ct)
    {
        try
        {
            var psi = PassStartInfo();
            psi.ArgumentList.Add("--check-git");
            using var p = Process.Start(psi) ?? throw new InvalidOperationException("Impossible de lancer la vérification git.");
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
            timeout.CancelAfter(TimeSpan.FromMinutes(2));
            try { await p.WaitForExitAsync(timeout.Token); }
            catch (OperationCanceledException) { p.Kill(entireProcessTree: true); Report("vérification git : pas de réponse en 2 min."); return; }
            string file = Path.Combine(_logDir, GitCheck.ResultFile);
            string result = File.Exists(file) ? File.ReadAllText(file).Trim() : $"code {p.ExitCode}";
            Report($"vérification git : {result}");
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            Report($"vérification git impossible : {ex.Message}");
        }
    }

    private ProcessStartInfo PassStartInfo()
    {
        string exe = Environment.ProcessPath ?? throw new InvalidOperationException("Chemin de l'exécutable inconnu.");
        var psi = new ProcessStartInfo(exe) { UseShellExecute = false, CreateNoWindow = true, WorkingDirectory = AppContext.BaseDirectory };
        // Lancé par « dotnet xck-publisher.dll » (développement) : le passage est la même dll.
        if (Path.GetFileNameWithoutExtension(exe).Equals("dotnet", StringComparison.OrdinalIgnoreCase))
            psi.ArgumentList.Add(Environment.GetCommandLineArgs()[0]);
        if (_settingsPath is not null)
        {
            psi.ArgumentList.Add("--settings");
            psi.ArgumentList.Add(_settingsPath);
        }
        psi.Environment[ServiceHost.StopFileVariable] = _stopFile;
        return psi;
    }

    private async Task RunPassAsync(CancellationToken ct)
    {
        try { File.Delete(_stopFile); } catch (IOException) { }
        using var p = Process.Start(PassStartInfo()) ?? throw new InvalidOperationException("Impossible de lancer le passage.");
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeout.CancelAfter(PassTimeout);
        try
        {
            await p.WaitForExitAsync(timeout.Token);
        }
        catch (OperationCanceledException)
        {
            // Arrêt du service ou passage trop long : le passage s'arrête de lui-même entre deux campagnes.
            File.WriteAllText(_stopFile, "stop");
            using var grace = new CancellationTokenSource(StopGrace);
            try
            {
                await p.WaitForExitAsync(grace.Token);
            }
            catch (OperationCanceledException)
            {
                p.Kill(entireProcessTree: true);
                Report("passage arrêté de force (il ne s'arrêtait pas) ; vérifier status.json au prochain passage.");
            }
            if (!ct.IsCancellationRequested) Report($"passage trop long (plus de {PassTimeout.TotalMinutes} min) : arrêté.");
            ct.ThrowIfCancellationRequested();
            return;
        }
        finally
        {
            try { File.Delete(_stopFile); } catch (IOException) { }
        }
        // 0 = normal, 1 = erreur déjà écrite dans publisher-*.log ; le reste est anormal.
        if (p.ExitCode is not (0 or 1)) Report($"passage terminé avec le code {p.ExitCode} (voir publisher-*.log).");
    }
}

[SupportedOSPlatform("windows")]
internal sealed class WindowsService : ServiceBase
{
    private readonly PassRunner _runner;
    private CancellationTokenSource? _cts;
    private Task? _loop;

    public WindowsService(PassRunner runner)
    {
        _runner = runner;
        ServiceName = ServiceHost.ServiceName;
        CanStop = true;
        CanShutdown = true;
        AutoLog = true;  // démarrage et arrêt dans le journal Application de Windows
    }

    protected override void OnStart(string[] args)
    {
        _cts = new CancellationTokenSource();
        var ct = _cts.Token;
        _loop = Task.Run(async () =>
        {
            await _runner.CheckGitAsync(ct);
            await _runner.Loop.RunAsync(ct);
        });
        _runner.Report($"Service démarré, un passage toutes les {_runner.Loop.Interval.TotalMinutes} min.");
    }

    protected override void OnStop() => StopLoop();
    protected override void OnShutdown() => StopLoop();

    private void StopLoop()
    {
        RequestAdditionalTime(150_000);
        _cts?.Cancel();
        try { _loop?.Wait(TimeSpan.FromSeconds(140)); }
        catch (AggregateException) { }
        _runner.Report("Service arrêté.");
    }
}
