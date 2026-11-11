namespace XCryptoKeys.Publisher;

/// <summary>Un compte X : accounts/&lt;nom&gt;/ avec ses campagnes dans threads/.</summary>
public sealed class Account
{
    public const string ThreadsFolder = "threads";

    public string Name { get; }
    public string Directory { get; }
    public string ThreadsPath => Path.Combine(Directory, ThreadsFolder);

    public Account(string directory)
    {
        Directory = directory;
        Name = Path.GetFileName(directory);
    }

    /// <summary>Tous les comptes (dossiers avec un sous-dossier threads/, sauf ceux qui commencent par « _ » ou « . »), triés par nom.</summary>
    public static IEnumerable<Account> Discover(string accountsPath) =>
        System.IO.Directory.GetDirectories(accountsPath)
            .Where(d => Path.GetFileName(d) is var n && !n.StartsWith('_') && !n.StartsWith('.'))
            .Where(d => System.IO.Directory.Exists(Path.Combine(d, ThreadsFolder)))
            .OrderBy(d => d, StringComparer.Ordinal)
            .Select(d => new Account(d));

    public IEnumerable<Campaign> Campaigns() => Campaign.Discover(ThreadsPath);

    public bool HasCampaign(string name) =>
        Campaigns().Any(c => string.Equals(c.Name, name, StringComparison.OrdinalIgnoreCase));

    /// <summary>Vrai si au moins une campagne lisible est en active.</summary>
    public bool HasActiveCampaign()
    {
        foreach (var c in Campaigns())
        {
            try
            {
                c.Load();
                if (c.IsActive) return true;
            }
            catch (Exception)
            {
                // Une campagne illisible sera signalée par le passage lui-même.
                return true;
            }
        }
        return false;
    }
}
