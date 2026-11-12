namespace XCryptoKeys.Publisher;

/// <summary>
/// xck-publisher --check-git : vérifie que le bot peut lire et pousser sur le dépôt distant (git ls-remote, puis
/// git push --dry-run : rien n'est envoyé), avec les réglages git du bot (Git.Remote, Git.SshCommand…).
/// Résultat en une ligne dans logs/git-check.txt (« OK : … » ou « KO : … ») ; code de sortie 0 si OK, 1 sinon.
/// Lancé par le service à son démarrage, donc sous son compte (LocalService).
/// </summary>
public static class GitCheck
{
    public const string ResultFile = "git-check.txt";

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
        string? dir = Log.ResolveDirectory(settings.Log.Directory);
        var (ok, message) = Check(settings);
        string line = $"{DateTime.UtcNow:yyyy-MM-ddTHH:mm:ssZ} {(ok ? "OK" : "KO")} : {message}";
        Console.WriteLine(line);
        if (dir is not null)
        {
            try
            {
                Directory.CreateDirectory(dir);
                File.WriteAllText(Path.Combine(dir, ResultFile), line + Environment.NewLine);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                Console.Error.WriteLine($"Écriture impossible de {ResultFile} : {ex.Message}");
            }
        }
        return ok ? 0 : 1;
    }

    /// <summary>Vérification elle-même ; jamais d'exception, un message clair en cas d'échec.</summary>
    public static (bool Ok, string Message) Check(Settings settings)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(settings.RepoPath) || !Directory.Exists(Path.Combine(settings.RepoPath, ".git")))
                return (false, $"dépôt introuvable ou pas un clone git : « {settings.RepoPath} »");
            using var log = new Log(null, 0);
            var git = new Git(settings.RepoPath, settings.Git, log);
            string remote = settings.Git.Remote;

            var read = git.Run("ls-remote", "--heads", remote);
            if (!read.Ok) return (false, $"lecture impossible sur {remote} : {FirstLine(read)}");

            string branch = settings.Git.Branch;
            if (string.IsNullOrWhiteSpace(branch))
            {
                var head = git.Run("rev-parse", "--abbrev-ref", "HEAD");
                branch = head.Ok ? head.Output : "main";
            }
            var write = git.Run("push", "--dry-run", "--porcelain", remote, $"HEAD:refs/heads/{branch}");
            string all = $"{write.Output}\n{write.Error}";
            // Refus pour retard (« rejected », non fast-forward) : l'accès en écriture marche, le passage fera un pull d'abord.
            bool writeOk = write.Ok || (all.Contains("[rejected]") && !all.Contains("denied", StringComparison.OrdinalIgnoreCase));
            string hint = string.IsNullOrWhiteSpace(settings.Git.SshCommand) ? "" : " (clé de déploiement sans « Allow write access » ?)";
            if (!writeOk) return (false, $"écriture impossible sur {remote}{hint} : {FirstLine(write)}");
            return (true, $"lecture et écriture possibles sur {remote} ({branch})");
        }
        catch (Exception ex)
        {
            return (false, $"git n'a pas pu être lancé : {ex.Message}");
        }
    }

    private static string FirstLine(Git.Result r)
    {
        string text = string.IsNullOrWhiteSpace(r.Error) ? r.Output : r.Error;
        var lines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        // La ligne utile est en général la dernière qui commence par « fatal: » ou « ERROR: », sinon la première.
        return lines.LastOrDefault(l => l.StartsWith("fatal:") || l.StartsWith("ERROR:") || l.Contains("denied"))
            ?? lines.FirstOrDefault() ?? $"code {r.ExitCode}";
    }
}
