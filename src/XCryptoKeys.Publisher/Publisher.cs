using System.Text.Json.Nodes;

namespace XCryptoKeys.Publisher;

public sealed record RunOptions(bool DryRun, DateTimeOffset Now, string? OnlyCampaign);

/// <summary>Un passage : pour chaque campagne active, publie au plus un post s'il est dû.</summary>
public sealed class Publisher(Settings settings, Log log, XClient? x, Git? git, RunOptions options)
{
    private readonly System.Diagnostics.Stopwatch _clock = System.Diagnostics.Stopwatch.StartNew();

    /// <summary>Heure courante du passage (options.Now + temps écoulé), pour que --now reste cohérent.</summary>
    private DateTimeOffset CurrentTime => options.Now + _clock.Elapsed;

    /// <summary>Texte à publier pour un post, avec le hashtag de campagne ajouté. Null + raison si le post n'est pas publiable.</summary>
    public static (string? Text, string? Error) BuildText(JsonObject post, string defaultVersion, string? hashtag, int maxLength)
    {
        string version = (string?)post["use"] is { Length: > 0 } use ? use : defaultVersion;
        string? text = (string?)post[version];
        if (string.IsNullOrWhiteSpace(text))
            return (null, $"pas de texte pour la version « {version} »");
        text = TweetText.AppendHashtag(text.Trim(), hashtag);
        int len = TweetText.WeightedLength(text);
        if (len > maxLength)
            return (null, $"version « {version} » trop longue : {len} caractères pour X (max {maxLength}), hashtag de campagne compris");
        return (text, null);
    }

    /// <summary>Texte alternatif de l'image, tiré du champ card (big, title, sub).</summary>
    public static string? BuildAltText(JsonObject post)
    {
        if (post["card"] is not JsonObject card) return null;
        var parts = new[] { (string?)card["big"], (string?)card["title"], (string?)card["sub"] }
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Select(s => s!.Trim().TrimEnd('.'));
        string alt = string.Join(". ", parts);
        return alt.Length == 0 ? null : alt + ".";
    }

    public async Task<int> RunAsync(CancellationToken ct)
    {
        var campaigns = Campaign.Discover(settings.ThreadsPath)
            .Where(c => options.OnlyCampaign is null || string.Equals(c.Name, options.OnlyCampaign, StringComparison.OrdinalIgnoreCase))
            .ToList();
        if (options.OnlyCampaign is not null && campaigns.Count == 0)
        {
            log.Error($"Campagne introuvable : {options.OnlyCampaign}");
            return 0;
        }

        int published = 0;
        bool needDelay = false;
        foreach (var campaign in campaigns)
        {
            ct.ThrowIfCancellationRequested();
            try
            {
                campaign.Load();
            }
            catch (Exception ex)
            {
                log.Error($"[{campaign.Name}] lecture impossible : {ex.Message}");
                continue;
            }

            if (!campaign.IsActive)
            {
                if (options.DryRun) log.Info($"[{campaign.Name}] inactive (dernier post publié : {campaign.LastPostedIndex}/{campaign.MaxIndex}).");
                continue;
            }

            int next = campaign.LastPostedIndex + 1;
            if (next > campaign.MaxIndex)
            {
                log.Info($"[{campaign.Name}] tous les posts sont publiés ; campagne passée en inactive.");
                if (!options.DryRun)
                {
                    campaign.MarkFinished();
                    CommitCampaign(campaign, $"Publication : {campaign.Name} terminée");
                }
                continue;
            }

            DateTimeOffset due;
            try
            {
                due = Schedule.NextDue(campaign.LastPostedAt, campaign.IntervalDays, campaign.PostTime, options.Now);
            }
            catch (FormatException ex)
            {
                log.Error($"[{campaign.Name}] status.json : {ex.Message}");
                continue;
            }

            if (options.Now < due)
            {
                log.Info($"[{campaign.Name}] prochain post #{next} dû le {due:yyyy-MM-dd HH:mm} UTC.");
                continue;
            }

            var post = campaign.FindPost(next);
            if (post is null)
            {
                log.Error($"[{campaign.Name}] post #{next} absent de posts.json ; campagne bloquée.");
                continue;
            }

            string postStatus = (string?)post["status"] ?? "";
            if (postStatus == "posted")
            {
                log.Warn($"[{campaign.Name}] post #{next} déjà marqué posted ; status.json avancé sans republier.");
                if (!options.DryRun)
                {
                    campaign.RepairLastPosted(next, (string?)post["posted_at"]);
                    CommitCampaign(campaign, $"Publication : {campaign.Name} état réparé (#{next})");
                }
                continue;
            }
            if (postStatus != "checked")
            {
                log.Error($"[{campaign.Name}] post #{next} en statut « {postStatus} », pas « checked » ; campagne bloquée.");
                continue;
            }

            var (text, error) = BuildText(post, settings.DefaultVersion, campaign.Hashtag, settings.MaxWeightedLength);
            if (text is null)
            {
                log.Error($"[{campaign.Name}] post #{next} : {error} ; campagne bloquée.");
                continue;
            }
            if (string.IsNullOrWhiteSpace(campaign.Hashtag))
                log.Warn($"[{campaign.Name}] pas de « hashtag » dans status.json.");

            string? image = settings.AttachImages ? campaign.FindImage(next) : null;
            string? alt = settings.AltText ? BuildAltText(post) : null;

            if (options.DryRun)
            {
                log.Info($"[{campaign.Name}] DRY-RUN post #{next} ({TweetText.WeightedLength(text)} car.)" +
                         (image is null ? " sans image" : $" avec {Path.GetFileName(image)}") + $" :\n{text}" +
                         (alt is null ? "" : $"\n  alt : {alt}"));
                published++;
                continue;
            }

            if (needDelay && settings.DelayBetweenPostsSeconds > 0)
                await Task.Delay(TimeSpan.FromSeconds(settings.DelayBetweenPostsSeconds), ct);
            needDelay = true;

            string tweetId;
            try
            {
                var media = new List<string>();
                if (image is not null)
                {
                    string mediaId = await x!.UploadImageAsync(image, ct);
                    if (alt is not null)
                    {
                        try { await x.SetAltTextAsync(mediaId, alt, ct); }
                        catch (XApiException ex) { log.Warn($"[{campaign.Name}] texte alternatif non posé : {ex.Message}"); }
                    }
                    media.Add(mediaId);
                }
                tweetId = await x!.CreatePostAsync(text, media, ct);
            }
            catch (Exception ex) when (ex is XApiException or HttpRequestException or TaskCanceledException or IOException or InvalidOperationException)
            {
                log.Error($"[{campaign.Name}] échec de publication du post #{next} : {ex.Message}");
                continue;
            }

            string url = string.IsNullOrWhiteSpace(settings.X.Username)
                ? $"https://x.com/i/web/status/{tweetId}"
                : $"https://x.com/{settings.X.Username}/status/{tweetId}";
            log.Info($"[{campaign.Name}] post #{next} publié : {url}");

            try
            {
                campaign.MarkPosted(next, CurrentTime, tweetId, url);
            }
            catch (Exception ex)
            {
                // Le post est en ligne mais l'état n'a pas pu être écrit : il faut corriger à la main pour éviter un doublon.
                log.Error($"[{campaign.Name}] POST #{next} PUBLIÉ ({url}) MAIS status.json/posts.json NON MIS À JOUR : {ex.Message}");
                throw;
            }
            CommitCampaign(campaign, $"Publication : {campaign.Name} #{next}\n\n{url}");
            published++;
        }

        return published;
    }

    private void CommitCampaign(Campaign campaign, string message)
    {
        if (git is null) return;
        string rel(string p) => Path.GetRelativePath(settings.RepoPath, p).Replace('\\', '/');
        if (!git.Commit([rel(campaign.StatusPath), rel(campaign.PostsPath)], message))
            log.Error($"[{campaign.Name}] commit impossible ; il sera refait au prochain passage.");
    }
}
