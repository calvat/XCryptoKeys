using System.Net;
using System.Text.Json.Nodes;

namespace XCryptoKeys.Publisher;

public sealed record RunOptions(bool DryRun, DateTimeOffset Now, string? OnlyCampaign);

/// <summary>Un passage pour un compte : pour chaque campagne active du compte, publie au plus un post s'il est dû.</summary>
public sealed class Publisher(Settings settings, Account account, Log log, XClient? x, Git? git, RunOptions options)
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
        // Le filtre --campaign est vérifié par Program sur l'ensemble des comptes : ici, un compte sans cette campagne ne fait rien.
        var campaigns = account.Campaigns()
            .Where(c => options.OnlyCampaign is null || string.Equals(c.Name, options.OnlyCampaign, StringComparison.OrdinalIgnoreCase))
            .ToList();
        string username = settings.XFor(account.Name).Username;

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
                log.Error($"{Tag(campaign)} lecture impossible : {ex.Message}");
                continue;
            }

            if (!campaign.IsActive)
            {
                if (options.DryRun) log.Info($"{Tag(campaign)} inactive (dernier post publié : {campaign.LastPostedIndex}/{campaign.MaxIndex}).");
                continue;
            }

            // Republication : un post déjà passé (index <= last_posted_index) mis en « republish », par exemple
            // parce qu'il a été supprimé de X. Il part tout de suite, sans toucher à last_posted_index / last_posted_at :
            // le calendrier des posts suivants ne bouge pas. Un seul post par campagne et par passage, comme d'habitude.
            int? lost = campaign.RepublishIndex;
            bool republish = lost is not null;
            int next = lost ?? campaign.LastPostedIndex + 1;
            if (!republish && next > campaign.MaxIndex)
            {
                if (options.DryRun) log.Info($"{Tag(campaign)} tous les posts sont publiés ; la campagne passerait en inactive.");
                else log.Action($"{Tag(campaign)} tous les posts sont publiés ; campagne passée en inactive.");
                if (!options.DryRun)
                {
                    campaign.MarkFinished();
                    CommitCampaign(campaign, $"Publication : {account.Name}/{campaign.Name} terminée");
                }
                continue;
            }

            DateTimeOffset due = options.Now;
            try
            {
                if (!republish) due = Schedule.NextDue(campaign.LastPostedAt, campaign.IntervalDays, campaign.PostTime, options.Now);
            }
            catch (FormatException ex)
            {
                log.Error($"{Tag(campaign)} status.json : {ex.Message}");
                continue;
            }

            if (options.Now < due)
            {
                log.Info($"{Tag(campaign)} prochain post #{next} dû le {due:yyyy-MM-dd HH:mm} UTC.");
                log.NoteNextDue(due, $"{account.Name}/{campaign.Name} #{next}");
                continue;
            }

            var post = campaign.FindPost(next);
            if (post is null)
            {
                log.Error($"{Tag(campaign)} post #{next} absent de posts.json ; campagne bloquée.");
                continue;
            }

            string postStatus = (string?)post["status"] ?? "";
            if (postStatus == "posted")
            {
                log.Warn($"{Tag(campaign)} post #{next} déjà marqué posted ; status.json avancé sans republier.");
                if (!options.DryRun)
                {
                    campaign.RepairLastPosted(next, (string?)post["posted_at"]);
                    CommitCampaign(campaign, $"Publication : {account.Name}/{campaign.Name} état réparé (#{next})");
                }
                continue;
            }
            if (postStatus != "checked" && !republish)
            {
                log.Error($"{Tag(campaign)} post #{next} en statut « {postStatus} », pas « checked » ; campagne bloquée.");
                continue;
            }

            var (text, error) = BuildText(post, settings.DefaultVersion, campaign.Hashtag, settings.MaxWeightedLength);
            if (text is null)
            {
                log.Error($"{Tag(campaign)} post #{next} : {error} ; campagne bloquée.");
                continue;
            }
            if (string.IsNullOrWhiteSpace(campaign.Hashtag))
                log.Warn($"{Tag(campaign)} pas de « hashtag » dans status.json.");

            string? image = settings.AttachImages ? campaign.FindImage(next) : null;
            string? alt = settings.AltText ? BuildAltText(post) : null;

            if (options.DryRun)
            {
                log.Info($"{Tag(campaign)} DRY-RUN {(republish ? "republication du " : "")}post #{next} ({TweetText.WeightedLength(text)} car.)" +
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
                        catch (XApiException ex) { log.Warn($"{Tag(campaign)} texte alternatif non posé : {ex.Message}"); }
                    }
                    media.Add(mediaId);
                }
                tweetId = await x!.CreatePostAsync(text, media, ct);
            }
            catch (XApiException ex) when (IsAuthFailure(ex))
            {
                // Clés refusées : problème du compte, pas du post. On arrête le compte pour ce passage (pas d'appels inutiles).
                AuthFailure = ex;
                log.Error($"[{account.Name}] X refuse les clés du compte (HTTP {(int)ex.Status}) ; compte arrêté. Vérifier avec : xck-publisher --check-x --account {account.Name}. Détail : {ex.Message}");
                return published;
            }
            catch (Exception ex) when (ex is XApiException or HttpRequestException or TaskCanceledException or IOException or InvalidOperationException)
            {
                log.Error($"{Tag(campaign)} échec de publication du post #{next} : {ex.Message}");
                continue;
            }

            string url = string.IsNullOrWhiteSpace(username)
                ? $"https://x.com/i/web/status/{tweetId}"
                : $"https://x.com/{username}/status/{tweetId}";
            log.PostPublished($"{Tag(campaign)} post #{next}/{campaign.MaxIndex} {(republish ? "republié" : "publié")} : {url}");

            try
            {
                if (republish) campaign.MarkRepublished(next, CurrentTime, tweetId, url);
                else campaign.MarkPosted(next, CurrentTime, tweetId, url);
            }
            catch (Exception ex)
            {
                // Le post est en ligne mais l'état n'a pas pu être écrit : il faut corriger à la main pour éviter un doublon.
                log.Error($"{Tag(campaign)} POST #{next} PUBLIÉ ({url}) MAIS status.json/posts.json NON MIS À JOUR : {ex.Message}");
                throw;
            }
            CommitCampaign(campaign, $"Publication : {account.Name}/{campaign.Name} #{next}{(republish ? " (republication)" : "")}\n\n{url}");
            published++;
        }

        return published;
    }

    /// <summary>Clés X refusées pendant ce passage (401, ou 403 de droits / d'offre) ; null sinon.</summary>
    public XApiException? AuthFailure { get; private set; }

    /// <summary>401 : clés refusées. 403 : seulement s'il s'agit des droits de l'app ou de l'offre (pas d'un post refusé, ex. doublon).</summary>
    public static bool IsAuthFailure(XApiException ex) =>
        ex.Status == HttpStatusCode.Unauthorized
        || (ex.Status == HttpStatusCode.Forbidden &&
            (ex.Body.Contains("oauth1-permissions", StringComparison.OrdinalIgnoreCase)
             || ex.Body.Contains("client-not-enrolled", StringComparison.OrdinalIgnoreCase)
             || ex.Body.Contains("not permitted", StringComparison.OrdinalIgnoreCase)
             || ex.Body.Contains("access level", StringComparison.OrdinalIgnoreCase)));

    private string Tag(Campaign campaign) => $"[{account.Name}/{campaign.Name}]";

    private void CommitCampaign(Campaign campaign, string message)
    {
        if (git is null) return;
        string rel(string p) => Path.GetRelativePath(settings.RepoPath, p).Replace('\\', '/');
        if (!git.Commit([rel(campaign.StatusPath), rel(campaign.PostsPath)], message))
            log.Error($"{Tag(campaign)} commit impossible ; il sera refait au prochain passage.");
    }
}
