using System.Globalization;
using System.Text.Json.Nodes;

namespace XCryptoKeys.Publisher;

/// <summary>Une campagne : accounts/&lt;compte&gt;/threads/&lt;nom&gt;-&lt;NNN&gt;/ avec posts.json, status.json et les images post_N.png.</summary>
public sealed class Campaign
{
    public static readonly string[] ImageExtensions = [".png", ".jpg", ".jpeg", ".webp", ".gif"];

    public string Name { get; }
    public string Directory { get; }
    public string StatusPath => Path.Combine(Directory, "status.json");
    public string PostsPath => Path.Combine(Directory, "posts.json");

    public JsonObject Status { get; private set; } = new();
    public JsonFile.Format StatusFormat { get; private set; } = new("\n", true);
    public JsonArray Posts { get; private set; } = new();
    public JsonFile.Format PostsFormat { get; private set; } = new("\n", true);

    private Campaign(string directory)
    {
        Directory = directory;
        Name = Path.GetFileName(directory);
    }

    /// <summary>Toutes les campagnes (dossiers avec status.json, sauf ceux qui commencent par « _ »), triées par nom.</summary>
    public static IEnumerable<Campaign> Discover(string threadsPath) =>
        System.IO.Directory.GetDirectories(threadsPath)
            .Where(d => !Path.GetFileName(d).StartsWith('_') && File.Exists(Path.Combine(d, "status.json")))
            .OrderBy(d => d, StringComparer.Ordinal)
            .Select(d => new Campaign(d));

    public void Load()
    {
        var (status, sf) = JsonFile.Read(StatusPath);
        Status = status as JsonObject ?? throw new InvalidDataException($"{StatusPath} doit être un objet JSON.");
        StatusFormat = sf;
        var (posts, pf) = JsonFile.Read(PostsPath);
        Posts = posts as JsonArray ?? throw new InvalidDataException($"{PostsPath} doit être un tableau JSON.");
        PostsFormat = pf;
    }

    public bool IsActive => string.Equals((string?)Status["status"], "active", StringComparison.OrdinalIgnoreCase);
    public int LastPostedIndex => Status["last_posted_index"]?.GetValue<int>() ?? 0;
    public string? LastPostedAt => (string?)Status["last_posted_at"];
    public int IntervalDays => Status["interval_days"]?.GetValue<int>() ?? 1;
    public string PostTime => (string?)Status["post_time"] ?? "09:00";
    public string? Hashtag => (string?)Status["hashtag"];

    public int MaxIndex => Posts.OfType<JsonObject>().Select(p => p["index"]?.GetValue<int>() ?? 0).DefaultIfEmpty(0).Max();

    public JsonObject? FindPost(int index) =>
        Posts.OfType<JsonObject>().FirstOrDefault(p => p["index"]?.GetValue<int>() == index);

    public string? FindImage(int index) =>
        ImageExtensions.Select(ext => Path.Combine(Directory, $"post_{index}{ext}")).FirstOrDefault(File.Exists);

    /// <summary>
    /// Enregistre un post publié. status.json est écrit en premier : c'est lui qui décide du prochain post,
    /// donc un arrêt brutal entre les deux écritures ne peut pas provoquer de doublon sur X.
    /// </summary>
    public void MarkPosted(int index, DateTimeOffset postedAt, string tweetId, string url)
    {
        string at = postedAt.UtcDateTime.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);
        Status["last_posted_index"] = index;
        Status["last_posted_at"] = at;
        if (index >= MaxIndex) Status["status"] = "inactive";
        JsonFile.Write(StatusPath, Status, StatusFormat);

        var post = FindPost(index)!;
        post["status"] = "posted";
        post["posted_at"] = at;
        post["tweet_id"] = tweetId;
        post["url"] = url;
        JsonFile.Write(PostsPath, Posts, PostsFormat);
    }

    /// <summary>Le post suivant est déjà marqué posted (état incohérent) : on avance status.json sans republier.</summary>
    public void RepairLastPosted(int index, string? postedAt)
    {
        Status["last_posted_index"] = index;
        if (postedAt is not null) Status["last_posted_at"] = postedAt;
        if (index >= MaxIndex) Status["status"] = "inactive";
        JsonFile.Write(StatusPath, Status, StatusFormat);
    }

    public void MarkFinished()
    {
        Status["status"] = "inactive";
        JsonFile.Write(StatusPath, Status, StatusFormat);
    }
}
