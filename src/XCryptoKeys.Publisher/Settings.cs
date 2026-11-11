using System.Text.Json;
using System.Text.Json.Nodes;

namespace XCryptoKeys.Publisher;

public sealed class Settings
{
    /// <summary>Chemin du clone local du dépôt XCryptoKeys.</summary>
    public string RepoPath { get; set; } = "";
    /// <summary>Dossier des campagnes, relatif à RepoPath.</summary>
    public string ThreadsFolder { get; set; } = "threads";
    /// <summary>Version de texte utilisée par défaut : simple, tech, variant…</summary>
    public string DefaultVersion { get; set; } = "simple";
    public bool AttachImages { get; set; } = true;
    /// <summary>Pose le texte alternatif des images (un appel API de plus par image).</summary>
    public bool AltText { get; set; } = true;
    public int MaxWeightedLength { get; set; } = 280;
    public int DelayBetweenPostsSeconds { get; set; } = 30;
    public LogSettings Log { get; set; } = new();
    public GitSettings Git { get; set; } = new();
    public XSettings X { get; set; } = new();

    public string ThreadsPath => Path.Combine(RepoPath, ThreadsFolder);

    public sealed class LogSettings
    {
        /// <summary>Dossier des logs ; relatif au dossier de l'exécutable s'il n'est pas absolu.</summary>
        public string Directory { get; set; } = "logs";
        public int RetentionDays { get; set; } = 30;
    }

    public sealed class GitSettings
    {
        public bool Enabled { get; set; } = true;
        public string Remote { get; set; } = "origin";
        /// <summary>Branche à pousser ; vide = branche courante.</summary>
        public string Branch { get; set; } = "";
        public string UserName { get; set; } = "";
        public string UserEmail { get; set; } = "";
        public int PushRetries { get; set; } = 3;
    }

    public sealed class XSettings
    {
        public string ApiBaseUrl { get; set; } = "https://api.x.com";
        public string Username { get; set; } = "";
        public string ApiKey { get; set; } = "";
        public string ApiKeySecret { get; set; } = "";
        public string AccessToken { get; set; } = "";
        public string AccessTokenSecret { get; set; } = "";
    }

    /// <summary>
    /// Charge appsettings.json, puis appsettings.local.json (secrets, ignoré par git) par-dessus,
    /// puis les variables d'environnement XCK__Section__Cle (ex. XCK__X__ApiKey).
    /// </summary>
    public static Settings Load(string? explicitPath)
    {
        JsonObject merged = new();
        IEnumerable<string> files;
        if (explicitPath is not null)
        {
            if (!File.Exists(explicitPath))
                throw new FileNotFoundException($"Fichier de settings introuvable : {explicitPath}");
            string dir = Path.GetDirectoryName(Path.GetFullPath(explicitPath))!;
            string local = Path.Combine(dir, Path.GetFileNameWithoutExtension(explicitPath) + ".local.json");
            files = [explicitPath, local];
        }
        else
        {
            string dir = AppContext.BaseDirectory;
            files = [Path.Combine(dir, "appsettings.json"), Path.Combine(dir, "appsettings.local.json")];
        }

        foreach (string file in files)
        {
            if (!File.Exists(file)) continue;
            var node = JsonNode.Parse(File.ReadAllText(file),
                documentOptions: new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true });
            if (node is JsonObject obj) Merge(merged, obj);
        }

        foreach (System.Collections.DictionaryEntry e in Environment.GetEnvironmentVariables())
        {
            string key = (string)e.Key;
            if (!key.StartsWith("XCK__", StringComparison.OrdinalIgnoreCase)) continue;
            string[] path = key["XCK__".Length..].Split("__");
            SetPath(merged, path, (string?)e.Value ?? "");
        }

        var settings = merged.Deserialize<Settings>(new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString,
        }) ?? new Settings();
        return settings;
    }

    public IEnumerable<string> Validate(bool dryRun)
    {
        if (string.IsNullOrWhiteSpace(RepoPath)) yield return "RepoPath est vide.";
        else if (!Directory.Exists(ThreadsPath)) yield return $"Dossier des campagnes introuvable : {ThreadsPath}";
        if (string.IsNullOrWhiteSpace(DefaultVersion)) yield return "DefaultVersion est vide.";
        if (!dryRun)
        {
            if (string.IsNullOrWhiteSpace(X.ApiKey)) yield return "X.ApiKey est vide.";
            if (string.IsNullOrWhiteSpace(X.ApiKeySecret)) yield return "X.ApiKeySecret est vide.";
            if (string.IsNullOrWhiteSpace(X.AccessToken)) yield return "X.AccessToken est vide.";
            if (string.IsNullOrWhiteSpace(X.AccessTokenSecret)) yield return "X.AccessTokenSecret est vide.";
        }
    }

    private static void Merge(JsonObject target, JsonObject source)
    {
        foreach (var (key, value) in source)
        {
            string? existingKey = target.Select(p => p.Key).FirstOrDefault(k => string.Equals(k, key, StringComparison.OrdinalIgnoreCase));
            if (value is JsonObject srcObj && existingKey is not null && target[existingKey] is JsonObject dstObj)
            {
                Merge(dstObj, srcObj);
                continue;
            }
            if (existingKey is not null) target.Remove(existingKey);
            target[key] = value?.DeepClone();
        }
    }

    private static void SetPath(JsonObject root, string[] path, string value)
    {
        JsonObject current = root;
        for (int i = 0; i < path.Length; i++)
        {
            string? existingKey = current.Select(p => p.Key).FirstOrDefault(k => string.Equals(k, path[i], StringComparison.OrdinalIgnoreCase));
            string key = existingKey ?? path[i];
            if (i == path.Length - 1)
            {
                // true/false deviennent des booléens ; les nombres restent des chaînes
                // (AllowReadingFromString les lit pour les champs int, et une clé numérique reste une chaîne).
                JsonNode node = bool.TryParse(value, out bool b) ? JsonValue.Create(b) : JsonValue.Create(value);
                current[key] = node;
                return;
            }
            if (current[key] is not JsonObject next)
            {
                next = new JsonObject();
                current[key] = next;
            }
            current = next;
        }
    }
}
