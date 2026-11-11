using System.Diagnostics;

namespace XCryptoKeys.Publisher;

/// <summary>Appels à la commande git dans le dépôt.</summary>
public sealed class Git(string repoPath, Settings.GitSettings settings, Log log)
{
    public sealed record Result(int ExitCode, string Output, string Error)
    {
        public bool Ok => ExitCode == 0;
    }

    public Result Run(params string[] args)
    {
        var psi = new ProcessStartInfo("git")
        {
            WorkingDirectory = repoPath,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };
        // Pas de prompt interactif : sans identifiants, git échoue au lieu de rester bloqué.
        psi.Environment["GIT_TERMINAL_PROMPT"] = "0";
        if (!string.IsNullOrWhiteSpace(settings.UserName))
        {
            psi.ArgumentList.Add("-c");
            psi.ArgumentList.Add($"user.name={settings.UserName}");
        }
        if (!string.IsNullOrWhiteSpace(settings.UserEmail))
        {
            psi.ArgumentList.Add("-c");
            psi.ArgumentList.Add($"user.email={settings.UserEmail}");
        }
        foreach (string a in args) psi.ArgumentList.Add(a);

        using var p = Process.Start(psi) ?? throw new InvalidOperationException("Impossible de lancer git.");
        var stdout = p.StandardOutput.ReadToEndAsync();
        var stderr = p.StandardError.ReadToEndAsync();
        p.WaitForExit();
        var r = new Result(p.ExitCode, stdout.Result.Trim(), stderr.Result.Trim());
        if (!r.Ok) log.Warn($"git {string.Join(' ', args)} a échoué ({r.ExitCode}) : {r.Error} {r.Output}".Trim());
        return r;
    }

    private string[] RemoteBranch => string.IsNullOrWhiteSpace(settings.Branch)
        ? [settings.Remote]
        : [settings.Remote, settings.Branch];

    /// <summary>
    /// Met le dépôt à jour. Si un run précédent s'est arrêté après avoir écrit status.json/posts.json
    /// sans commit, on commit d'abord ces fichiers pour ne jamais perdre la trace d'un post publié.
    /// </summary>
    public bool Pull(string stateFolder)
    {
        CommitPendingState(stateFolder, "Publication : récupération d'un état non commité");
        return Run(["pull", "--rebase", "--autostash", .. RemoteBranch]).Ok;
    }

    public bool CommitPendingState(string stateFolder, string message)
    {
        var status = Run("status", "--porcelain", "--", stateFolder);
        if (!status.Ok || status.Output.Length == 0) return true;
        var files = status.Output.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(l => l.StartsWith("M ") || l.StartsWith(" M") || l.StartsWith("MM"))
            .Select(l => l[2..].Trim().Trim('"'))
            .Where(f => f.EndsWith("/status.json") || f.EndsWith("/posts.json"))
            .ToList();
        if (files.Count == 0) return true;
        log.Warn($"Fichiers d'état modifiés non commités, commit de récupération : {string.Join(", ", files)}");
        return Commit(files, message);
    }

    public bool Commit(IReadOnlyList<string> files, string message)
    {
        if (!Run(["add", "--", .. files]).Ok) return false;
        // « -- fichiers » : on ne commite que ces fichiers, même si d'autres sont indexés.
        return Run(["commit", "-m", message, "--", .. files]).Ok;
    }

    /// <summary>Pousse les commits ; en cas de refus (le dépôt a bougé), rebase puis réessaie.</summary>
    public bool Push()
    {
        int attempts = Math.Max(1, settings.PushRetries);
        for (int i = 1; i <= attempts; i++)
        {
            if (Run(["push", .. RemoteBranch]).Ok) return true;
            if (i < attempts)
            {
                Run(["pull", "--rebase", "--autostash", .. RemoteBranch]);
                Thread.Sleep(TimeSpan.FromSeconds(2 * i));
            }
        }
        return false;
    }
}
