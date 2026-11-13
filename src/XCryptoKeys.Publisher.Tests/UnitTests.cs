using System.Net;
using System.Text.Json.Nodes;
using XCryptoKeys.Publisher;

namespace XCryptoKeys.Publisher.Tests;

public class OAuth1Tests
{
    [Fact]
    public void Signature_matches_the_example_of_the_X_documentation()
    {
        // Exemple « Creating a signature » de la documentation X/Twitter.
        var oauth = new OAuth1("xvz1evFS4wEEPTGEFPHBog", "kAcSOqF21Fu85e7zjz7ZN2U4ZRhfV3WpwPAoE3Z7kBw",
            "370773112-GmHxMAgYyLbNEtIKZeRNFsMKPR9EyMZeS9weJAEb", "LswwdoUaIvS8ltyTt5jkRh4J50vUPVVHtR2YPi5kE");
        string header = oauth.BuildHeader("POST", new Uri("https://api.twitter.com/1.1/statuses/update.json?include_entities=true"),
            [new("status", "Hello Ladies + Gentlemen, a signed OAuth request!")],
            nonce: "kYjzVBB8Y0ZFabxSWbWovY3uYSQ2pTgmZeNu2VS4cg", timestamp: "1318622958");
        Assert.Contains("oauth_signature=\"hCtSmYh%2BiHYCEqBWrE7C7hYmtUk%3D\"", header);
    }

    [Theory]
    [InlineData("Ladies + Gentlemen", "Ladies%20%2B%20Gentlemen")]
    [InlineData("An encoded string!", "An%20encoded%20string%21")]
    [InlineData("Dogs, Cats & Mice", "Dogs%2C%20Cats%20%26%20Mice")]
    [InlineData("☃", "%E2%98%83")]
    public void Percent_encoding_follows_RFC3986(string input, string expected) =>
        Assert.Equal(expected, OAuth1.Encode(input));
}

public class TweetTextTests
{
    [Fact]
    public void Latin_text_counts_one_per_char() => Assert.Equal(5, TweetText.WeightedLength("hello"));

    [Fact]
    public void Heavy_chars_and_emoji_count_two()
    {
        Assert.Equal(2, TweetText.WeightedLength("≈"));
        Assert.Equal(2, TweetText.WeightedLength("→"));
        Assert.Equal(2, TweetText.WeightedLength("🔐"));
        Assert.Equal(2, TweetText.WeightedLength("👍🏽"));
        Assert.Equal(1, TweetText.WeightedLength("é"));
    }

    [Fact]
    public void Urls_count_23() =>
        Assert.Equal(23 + 4, TweetText.WeightedLength("see https://github.com/bitcoin/bitcoin/blob/master/src/consensus/consensus.h"));

    [Fact]
    public void Hashtag_is_appended_once()
    {
        Assert.Equal("Hello #BTC #XCKbtc1", TweetText.AppendHashtag("Hello #BTC", "#XCKbtc1"));
        Assert.Equal("Hello #xckbtc1 #BTC", TweetText.AppendHashtag("Hello #xckbtc1 #BTC", "#XCKbtc1"));
        Assert.Equal("Hello #XCKbtc12 #XCKbtc1", TweetText.AppendHashtag("Hello #XCKbtc12", "#XCKbtc1"));
        Assert.Equal("Hello #XCKbtc1", TweetText.AppendHashtag("Hello", "XCKbtc1"));
        Assert.Equal("Hello", TweetText.AppendHashtag("Hello", null));
    }
}

public class ScheduleTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 12, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    public void First_post_is_due_today_at_post_time() =>
        Assert.Equal(new DateTimeOffset(2026, 11, 12, 9, 0, 0, TimeSpan.Zero), Schedule.NextDue(null, 1, "09:00", Now));

    [Fact]
    public void Next_post_is_due_interval_days_later_at_post_time()
    {
        Assert.Equal(new DateTimeOffset(2026, 11, 12, 9, 0, 0, TimeSpan.Zero), Schedule.NextDue("2026-11-11T09:00:12Z", 1, "09:00", Now));
        Assert.Equal(new DateTimeOffset(2026, 11, 13, 9, 0, 0, TimeSpan.Zero), Schedule.NextDue("2026-11-11", 2, "09:00", Now));
    }

    [Fact]
    public void Late_post_after_outage_restarts_rhythm_from_its_own_day() =>
        // Publié en retard le 12 à 10:00 : le suivant est le 13 à 09:00, pas tout de suite.
        Assert.Equal(new DateTimeOffset(2026, 11, 13, 9, 0, 0, TimeSpan.Zero), Schedule.NextDue("2026-11-12T10:00:00Z", 1, "09:00", Now));

    [Fact]
    public void Invalid_post_time_throws() => Assert.Throws<FormatException>(() => Schedule.ParsePostTime("9h"));
}

public class JsonFileTests
{
    [Fact]
    public void Round_trip_keeps_python_format_and_line_endings()
    {
        string original = "[\r\n  {\r\n    \"index\": 1,\r\n    \"simple\": \"a \\\"q\\\" ≈ → é 🔐\",\r\n    \"card\": {\r\n      \"big\": \"$116M\"\r\n    },\r\n    \"list\": [],\r\n    \"obj\": {},\r\n    \"n\": null,\r\n    \"f\": 1.5\r\n  }\r\n]\r\n";
        string path = Path.GetTempFileName();
        File.WriteAllText(path, original);
        var (node, fmt) = JsonFile.Read(path);
        JsonFile.Write(path, node, fmt);
        Assert.Equal(original, File.ReadAllText(path));
        File.Delete(path);
    }
}

public class PublisherTests
{
    [Fact]
    public void Text_uses_default_version_or_override_and_checks_length()
    {
        var post = new JsonObject { ["simple"] = "Simple text", ["tech"] = "Tech text" };
        Assert.Equal("Simple text #XCKbtc1", Publisher.BuildText(post, "simple", "#XCKbtc1", 280).Text);
        post["use"] = "tech";
        Assert.Equal("Tech text #XCKbtc1", Publisher.BuildText(post, "simple", "#XCKbtc1", 280).Text);
        post["tech"] = new string('a', 275);
        Assert.Null(Publisher.BuildText(post, "simple", "#XCKbtc1", 280).Text);
    }

    [Fact]
    public void Alt_text_comes_from_card()
    {
        var post = new JsonObject { ["card"] = new JsonObject { ["big"] = "$116M", ["title"] = "Honest oracle, thin market", ["sub"] = "MNGO was pushed up." } };
        Assert.Equal("$116M. Honest oracle, thin market. MNGO was pushed up.", Publisher.BuildAltText(post));
    }

    private static string MakeRepo(string status)
    {
        string repo = Path.Combine(Path.GetTempPath(), "xck-test-" + Guid.NewGuid().ToString("N"));
        string threads = Path.Combine(repo, "accounts", "Demo", "threads");
        string c = Path.Combine(threads, "demo-001");
        Directory.CreateDirectory(c);
        Directory.CreateDirectory(Path.Combine(threads, "_template"));
        File.WriteAllText(Path.Combine(threads, "_template", "status.json"), "{\"status\": \"active\"}");
        File.WriteAllText(Path.Combine(c, "status.json"), status);
        File.WriteAllText(Path.Combine(c, "posts.json"), """
            [
              {"index": 1, "simple": "Demo #1: first", "tech": "t1", "status": "checked", "card": {"title": "First"}},
              {"index": 2, "simple": "Demo #2: second", "tech": "t2", "status": "checked"}
            ]
            """);
        File.WriteAllBytes(Path.Combine(c, "post_1.png"), [0x89, 0x50, 0x4E, 0x47]);
        return repo;
    }

    private sealed class FakeX : HttpMessageHandler
    {
        public List<(string Path, string Body)> Requests { get; } = [];
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            Assert.StartsWith("OAuth ", request.Headers.GetValues("Authorization").Single());
            string body = request.Content is null ? "" : await request.Content.ReadAsStringAsync(ct);
            Requests.Add((request.RequestUri!.AbsolutePath, body));
            string json = request.RequestUri.AbsolutePath switch
            {
                "/2/media/upload" => """{"data":{"id":"777","media_key":"3_777"}}""",
                "/2/media/metadata" => """{"data":{"id":"777"}}""",
                "/2/tweets" => """{"data":{"id":"123456","text":"x"}}""",
                _ => "{}",
            };
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(json) };
        }
    }

    private static Settings MakeSettings(string repo) => new()
    {
        RepoPath = repo,
        DelayBetweenPostsSeconds = 0,
        Git = new Settings.GitSettings { Enabled = false },
        X = new Settings.XSettings { ApiBaseUrl = "https://api.example" },
        Accounts = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Demo"] = new Settings.AccountSettings
            {
                X = new Settings.XSettings { Username = "demoaccount", ApiKey = "k", ApiKeySecret = "s", AccessToken = "t", AccessTokenSecret = "ts" },
            },
        },
    };

    private static Account DemoAccount(string repo) => new(Path.Combine(repo, "accounts", "Demo"));

    [Fact]
    public async Task Publishes_due_post_with_image_and_updates_state()
    {
        string repo = MakeRepo("""{"status": "active", "last_posted_index": 0, "last_posted_at": null, "interval_days": 1, "post_time": "09:00", "hashtag": "#XCKdem1"}""");
        var settings = MakeSettings(repo);
        using var log = new Log(null, 0);
        var fake = new FakeX();
        using var x = new XClient(settings.XFor("Demo"), log, fake);
        var now = new DateTimeOffset(2026, 11, 12, 9, 5, 0, TimeSpan.Zero);

        int n = await new Publisher(settings, DemoAccount(repo), log, x, null, new RunOptions(false, now, null)).RunAsync(default);

        Assert.Equal(1, n);
        Assert.Equal(1, log.Posted);
        Assert.True(log.HasActivity);
        Assert.Equal("1 post(s) publié(s)", log.Result);
        Assert.Equal(["/2/media/upload", "/2/media/metadata", "/2/tweets"], fake.Requests.Select(r => r.Path));
        var tweet = JsonNode.Parse(fake.Requests[2].Body)!;
        Assert.Equal("Demo #1: first #XCKdem1", (string?)tweet["text"]);
        Assert.Equal("777", (string?)tweet["media"]!["media_ids"]![0]);

        string dir = Path.Combine(repo, "accounts", "Demo", "threads", "demo-001");
        var status = JsonNode.Parse(File.ReadAllText(Path.Combine(dir, "status.json")))!;
        Assert.Equal(1, (int)status["last_posted_index"]!);
        Assert.Equal("active", (string?)status["status"]);
        var post = JsonNode.Parse(File.ReadAllText(Path.Combine(dir, "posts.json")))![0]!;
        Assert.Equal("posted", (string?)post["status"]);
        Assert.Equal("https://x.com/demoaccount/status/123456", (string?)post["url"]);

        // Même passage relancé dans la foulée : rien n'est dû.
        int again = await new Publisher(settings, DemoAccount(repo), log, x, null, new RunOptions(false, now.AddMinutes(15), null)).RunAsync(default);
        Assert.Equal(0, again);
        Assert.Equal(3, fake.Requests.Count);
        Directory.Delete(repo, true);
    }

    [Fact]
    public async Task Last_post_without_image_deactivates_campaign()
    {
        string repo = MakeRepo("""{"status": "active", "last_posted_index": 1, "last_posted_at": "2026-11-11T09:00:00Z", "interval_days": 1, "post_time": "09:00"}""");
        var settings = MakeSettings(repo);
        using var log = new Log(null, 0);
        var fake = new FakeX();
        using var x = new XClient(settings.XFor("Demo"), log, fake);

        int n = await new Publisher(settings, DemoAccount(repo), log, x, null, new RunOptions(false, new DateTimeOffset(2026, 11, 20, 12, 0, 0, TimeSpan.Zero), null)).RunAsync(default);

        Assert.Equal(1, n);
        Assert.Equal(["/2/tweets"], fake.Requests.Select(r => r.Path));
        var status = JsonNode.Parse(File.ReadAllText(Path.Combine(repo, "accounts", "Demo", "threads", "demo-001", "status.json")))!;
        Assert.Equal("inactive", (string?)status["status"]);
        Assert.Equal(2, (int)status["last_posted_index"]!);
        Directory.Delete(repo, true);
    }

    [Fact]
    public async Task Past_post_back_to_checked_is_republished_without_moving_the_schedule()
    {
        // #1 supprimé de X et mis en « republish » ; #2 déjà en ligne ; #3 dû seulement demain.
        string statusJson = """{"status": "active", "last_posted_index": 2, "last_posted_at": "2026-11-13T09:00:01Z", "interval_days": 1, "post_time": "09:00", "hashtag": "#XCKdem1"}""";
        string repo = MakeRepo(statusJson);
        string dir = Path.Combine(repo, "accounts", "Demo", "threads", "demo-001");
        File.WriteAllText(Path.Combine(dir, "posts.json"), """
            [
              {"index": 1, "simple": "Demo #1: first", "status": "republish", "card": {"title": "First"}},
              {"index": 2, "simple": "Demo #2: second", "status": "posted", "posted_at": "2026-11-13T09:00:01Z", "tweet_id": "2", "url": "u2"},
              {"index": 3, "simple": "Demo #3: third", "status": "checked"}
            ]
            """);
        var settings = MakeSettings(repo);
        using var log = new Log(null, 0);
        var fake = new FakeX();
        using var x = new XClient(settings.XFor("Demo"), log, fake);
        var now = new DateTimeOffset(2026, 11, 13, 14, 30, 0, TimeSpan.Zero);

        // Dry-run : annonce la republication, n'envoie rien.
        Assert.Equal(1, await new Publisher(settings, DemoAccount(repo), log, null, null, new RunOptions(true, now, null)).RunAsync(default));
        Assert.Empty(fake.Requests);

        int n = await new Publisher(settings, DemoAccount(repo), log, x, null, new RunOptions(false, now, null)).RunAsync(default);

        Assert.Equal(1, n);
        Assert.Equal(["/2/media/upload", "/2/media/metadata", "/2/tweets"], fake.Requests.Select(r => r.Path));
        Assert.Equal("Demo #1: first #XCKdem1", (string?)JsonNode.Parse(fake.Requests[2].Body)!["text"]);
        var posts = JsonNode.Parse(File.ReadAllText(Path.Combine(dir, "posts.json")))!;
        Assert.Equal("posted", (string?)posts[0]!["status"]);
        Assert.Equal("123456", (string?)posts[0]!["tweet_id"]);
        Assert.Equal("2", (string?)posts[1]!["tweet_id"]);
        Assert.Equal("checked", (string?)posts[2]!["status"]);
        // status.json intact : #3 reste prévu demain à 09:00.
        Assert.Equal(JsonNode.Parse(statusJson)!.ToJsonString(), JsonNode.Parse(File.ReadAllText(Path.Combine(dir, "status.json")))!.ToJsonString());

        // Passage suivant : rien à republier, #3 pas encore dû.
        Assert.Equal(0, await new Publisher(settings, DemoAccount(repo), log, x, null, new RunOptions(false, now.AddMinutes(15), null)).RunAsync(default));
        Assert.Equal(3, fake.Requests.Count);
        Directory.Delete(repo, true);
    }

    [Fact]
    public async Task Dry_run_and_inactive_campaigns_send_nothing()
    {
        string repo = MakeRepo("""{"status": "inactive", "last_posted_index": 0, "last_posted_at": null, "interval_days": 1, "post_time": "09:00"}""");
        var settings = MakeSettings(repo);
        using var log = new Log(null, 0);
        int n = await new Publisher(settings, DemoAccount(repo), log, null, null, new RunOptions(true, DateTimeOffset.UtcNow, null)).RunAsync(default);
        Assert.Equal(0, n);
        Directory.Delete(repo, true);
    }

    [Fact]
    public void Real_campaigns_all_fit_in_280_with_their_hashtag()
    {
        // Vérifie le vrai dépôt quand le test tourne depuis src/ du dépôt.
        string? dir = AppContext.BaseDirectory;
        while (dir is not null && !Directory.Exists(Path.Combine(dir, "accounts"))) dir = Path.GetDirectoryName(dir);
        if (dir is null) return;
        var problems = new List<string>();
        foreach (var a in Account.Discover(Path.Combine(dir, "accounts")))
        foreach (var c in a.Campaigns())
        {
            c.Load();
            if (string.IsNullOrWhiteSpace(c.Hashtag)) problems.Add($"{a.Name}/{c.Name}: pas de hashtag");
            foreach (var post in c.Posts.OfType<JsonObject>())
            {
                var (text, error) = Publisher.BuildText(post, "simple", c.Hashtag, 280);
                if (text is null) problems.Add($"{a.Name}/{c.Name} #{post["index"]}: {error}");
            }
        }
        Assert.Empty(problems);
    }

    [Fact]
    public void Real_posts_not_yet_published_have_at_most_two_hashtags()
    {
        // Texte tel que publié (hashtag de campagne ajouté) : préfixe + campagne = 2.
        // Un hashtag = # suivi d'une lettre : la numérotation « #12: » n'en est pas un.
        var hashtag = new System.Text.RegularExpressions.Regex(@"(?<![\w&])#[A-Za-z]\w*");
        string? dir = AppContext.BaseDirectory;
        while (dir is not null && !Directory.Exists(Path.Combine(dir, "accounts"))) dir = Path.GetDirectoryName(dir);
        if (dir is null) return;
        var problems = new List<string>();
        foreach (var a in Account.Discover(Path.Combine(dir, "accounts")))
        foreach (var c in a.Campaigns())
        {
            c.Load();
            foreach (var post in c.Posts.OfType<JsonObject>())
            {
                if ((string?)post["status"] == "posted") continue;
                foreach (string version in new[] { "simple", "tech", "variant" })
                {
                    if ((string?)post[version] is not { Length: > 0 } raw) continue;
                    // La longueur n'est vérifiée que pour la version publiée (test précédent) ; ici, le nombre de hashtags de chaque version.
                    string text = TweetText.AppendHashtag(raw.Trim(), c.Hashtag);
                    int n = hashtag.Matches(text).Count;
                    if (n > 2) problems.Add($"{a.Name}/{c.Name} #{post["index"]} {version}: {n} hashtags");
                }
            }
        }
        Assert.Empty(problems);
    }
}

public class AccountTests
{
    [Fact]
    public void Discovers_account_folders_with_threads_only()
    {
        string root = Path.Combine(Path.GetTempPath(), "xck-acc-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(root, "Beta", "threads"));
        Directory.CreateDirectory(Path.Combine(root, "Alpha", "threads", "x-001"));
        File.WriteAllText(Path.Combine(root, "Alpha", "threads", "x-001", "status.json"), "{\"status\": \"active\"}");
        Directory.CreateDirectory(Path.Combine(root, "_template", "threads"));
        Directory.CreateDirectory(Path.Combine(root, "NoThreads"));

        var accounts = Account.Discover(root).ToList();

        Assert.Equal(["Alpha", "Beta"], accounts.Select(a => a.Name));
        Assert.True(accounts[0].HasCampaign("X-001"));
        Assert.True(accounts[0].HasActiveCampaign());
        Assert.False(accounts[1].HasActiveCampaign());
        Directory.Delete(root, true);
    }

    [Fact]
    public void Credentials_are_per_account_and_never_shared()
    {
        var settings = new Settings
        {
            X = new Settings.XSettings { ApiBaseUrl = "https://api.example" },
            Accounts = new(StringComparer.OrdinalIgnoreCase)
            {
                ["Alpha"] = new Settings.AccountSettings { X = new Settings.XSettings { ApiKey = "k", ApiKeySecret = "s", AccessToken = "t", AccessTokenSecret = "ts" } },
            },
        };

        var alpha = settings.XFor("alpha");
        Assert.Equal("k", alpha.ApiKey);
        Assert.Equal("https://api.example", alpha.ApiBaseUrl);
        Assert.Equal("alpha", alpha.Username);
        Assert.Empty(settings.MissingCredentials("Alpha"));
        Assert.Equal(["ApiKey", "ApiKeySecret", "AccessToken", "AccessTokenSecret"], settings.MissingCredentials("Beta"));
        Assert.True(settings.AccountSettingsFor("Beta").Enabled);
    }

    [Fact]
    public void Account_credentials_load_from_environment()
    {
        string file = Path.Combine(Path.GetTempPath(), "xck-settings-" + Guid.NewGuid().ToString("N") + ".json");
        File.WriteAllText(file, """{"RepoPath": "/tmp", "Accounts": {"XCryptoKeys": {"X": {"Username": "xcryptokeys"}}}}""");
        Environment.SetEnvironmentVariable("XCK__Accounts__XCryptoKeys__X__ApiKey", "from-env");
        try
        {
            var settings = Settings.Load(file);
            Assert.Equal("from-env", settings.XFor("XCryptoKeys").ApiKey);
            Assert.Equal("xcryptokeys", settings.XFor("XCryptoKeys").Username);
        }
        finally
        {
            Environment.SetEnvironmentVariable("XCK__Accounts__XCryptoKeys__X__ApiKey", null);
            File.Delete(file);
        }
    }

    [Fact]
    public void Missing_or_example_keys_mean_waiting_and_partial_keys_are_incomplete()
    {
        Assert.True(Settings.IsUnset(null));
        Assert.True(Settings.IsUnset("  "));
        Assert.True(Settings.IsUnset("<API Key (Consumer Key)>"));
        Assert.False(Settings.IsUnset("abc123"));

        // Fichier absent.
        var none = TempAccount("None", """{ "x": { "username": "none" } }""", null);
        // Fichier copié depuis l'exemple sans être rempli.
        string? root = FindRepoRoot();
        string examplePath = Path.Combine(root ?? "", "accounts", "XCryptoKeys", "account.local.example.json");
        string example = root is not null && File.Exists(examplePath)
            ? File.ReadAllText(examplePath)
            : """{ "x": { "apiKey": "<API Key>", "apiKeySecret": "<API Key Secret>", "accessToken": "<Access Token>", "accessTokenSecret": "<Access Token Secret>" } }""";
        var placeholders = TempAccount("Placeholders", null, example);
        var partial = TempAccount("Partial", null, """{ "x": { "apiKey": "k", "apiKeySecret": "<API Key Secret>" } }""");
        var settings = new Settings();
        try
        {
            foreach (var a in new[] { none, placeholders, partial }) settings.LoadAccountFiles(a);
            Assert.True(settings.HasNoCredentials("None"));
            Assert.True(settings.HasNoCredentials("Placeholders"));
            Assert.False(settings.HasNoCredentials("Partial"));
            Assert.Equal(["ApiKeySecret", "AccessToken", "AccessTokenSecret"], settings.MissingCredentials("Partial"));
        }
        finally
        {
            foreach (var a in new[] { none, placeholders, partial }) Directory.Delete(Path.GetDirectoryName(a.Directory)!, true);
        }
    }

    private static string? FindRepoRoot()
    {
        string? dir = AppContext.BaseDirectory;
        while (dir is not null && !Directory.Exists(Path.Combine(dir, "accounts"))) dir = Path.GetDirectoryName(dir);
        return dir;
    }

    private static Account TempAccount(string name, string? accountJson, string? localJson)
    {
        string dir = Path.Combine(Path.GetTempPath(), "xck-accfiles-" + Guid.NewGuid().ToString("N"), name);
        Directory.CreateDirectory(Path.Combine(dir, "threads"));
        if (accountJson is not null) File.WriteAllText(Path.Combine(dir, Settings.AccountFile), accountJson);
        if (localJson is not null) File.WriteAllText(Path.Combine(dir, Settings.AccountLocalFile), localJson);
        return new Account(dir);
    }

    [Fact]
    public void Account_files_merge_account_json_then_local_then_settings()
    {
        var account = TempAccount("Alpha",
            """{ "enabled": true, "x": { "username": "alpha_x", "apiKey": "from-account-json" } }""",
            """
            // commentaire accepté
            { "x": { "apiKey": "from-local", "apiKeySecret": "s", "accessToken": "t", "accessTokenSecret": "ts", }, }
            """);
        string file = Path.Combine(Path.GetTempPath(), "xck-settings-" + Guid.NewGuid().ToString("N") + ".json");
        File.WriteAllText(file, """{"RepoPath": "/tmp", "Accounts": {"alpha": {"X": {"AccessToken": "from-settings"}}}}""");
        try
        {
            var settings = Settings.Load(file);
            settings.LoadAccountFiles(account);
            var x = settings.XFor("Alpha");
            Assert.Equal("alpha_x", x.Username);
            Assert.Equal("from-local", x.ApiKey);
            Assert.Equal("s", x.ApiKeySecret);
            Assert.Equal("from-settings", x.AccessToken);
            Assert.Empty(settings.MissingCredentials("Alpha"));
            // Un autre compte ne récupère jamais ces clés.
            Assert.Equal(4, settings.MissingCredentials("Beta").Count());
        }
        finally
        {
            File.Delete(file);
            Directory.Delete(Path.GetDirectoryName(account.Directory)!, true);
        }
    }

    [Fact]
    public void Account_json_can_disable_and_settings_without_load_still_override()
    {
        var account = TempAccount("Gamma", """{ "enabled": false, "x": { "username": "gamma" } }""", null);
        var settings = new Settings
        {
            Accounts = new(StringComparer.OrdinalIgnoreCase)
            {
                ["gamma"] = new Settings.AccountSettings { X = new Settings.XSettings { ApiKey = "k" } },
            },
        };
        try
        {
            settings.LoadAccountFiles(account);
            Assert.False(settings.AccountSettingsFor("Gamma").Enabled);
            Assert.Equal("gamma", settings.XFor("Gamma").Username);
            Assert.Equal("k", settings.XFor("Gamma").ApiKey);
        }
        finally
        {
            Directory.Delete(Path.GetDirectoryName(account.Directory)!, true);
        }
    }

    [Fact]
    public void Invalid_account_file_throws_and_leaves_other_accounts_alone()
    {
        var bad = TempAccount("Bad", null, """{ "x": { "apiKey": """);
        var good = TempAccount("Good", """{ "x": { "username": "good" } }""", null);
        var settings = new Settings();
        try
        {
            var e = Assert.Throws<InvalidDataException>(() => settings.LoadAccountFiles(bad));
            Assert.Contains(Settings.AccountLocalFile, e.Message);
            settings.LoadAccountFiles(good);
            Assert.Equal("good", settings.XFor("Good").Username);
            Assert.Throws<InvalidDataException>(() => settings.LoadAccountFiles(TempAccount("Arr", "[1, 2]", null)));
        }
        finally
        {
            Directory.Delete(Path.GetDirectoryName(bad.Directory)!, true);
            Directory.Delete(Path.GetDirectoryName(good.Directory)!, true);
        }
    }

    [Fact]
    public void Real_account_files_parse_and_hold_no_secrets()
    {
        string? dir = AppContext.BaseDirectory;
        while (dir is not null && !Directory.Exists(Path.Combine(dir, "accounts"))) dir = Path.GetDirectoryName(dir);
        if (dir is null) return;
        foreach (var a in Account.Discover(Path.Combine(dir, "accounts")))
        {
            string file = Path.Combine(a.Directory, Settings.AccountFile);
            Assert.True(File.Exists(file), $"{a.Name} : {Settings.AccountFile} manquant");
            var node = JsonNode.Parse(File.ReadAllText(file))!.AsObject();
            var x = node["x"] as JsonObject;
            Assert.False(string.IsNullOrWhiteSpace((string?)x?["username"]), $"{a.Name} : x.username manquant");
            foreach (string secret in new[] { "apiKey", "apiKeySecret", "accessToken", "accessTokenSecret" })
                Assert.False(x!.Any(p => string.Equals(p.Key, secret, StringComparison.OrdinalIgnoreCase)), $"{a.Name} : {secret} ne doit pas être dans {Settings.AccountFile}");
            var settings = new Settings();
            settings.LoadAccountFiles(new Account(a.Directory) );
        }
    }
}

public class LogTests
{
    private static string TempDir() => Path.Combine(Path.GetTempPath(), "xck-log-" + Guid.NewGuid().ToString("N"));

    [Fact]
    public void Quiet_run_writes_no_log_file_only_last_run()
    {
        string dir = TempDir();
        var log = new Log(dir, 30);
        log.Info("Début du passage");
        log.Info("[Demo/demo-001] prochain post #2 dû le 2026-11-13 09:00 UTC.");
        log.NoteNextDue(new DateTimeOffset(2026, 11, 13, 9, 0, 0, TimeSpan.Zero), "Demo/demo-001 #2");
        log.NoteWaitingForKeys("Autre");
        log.Info("Fin du passage");
        Assert.Null(Directory.Exists(dir) ? Directory.GetFiles(dir, "publisher-*.log").FirstOrDefault() : null);
        log.Dispose();
        Assert.Empty(Directory.GetFiles(dir, "publisher-*.log"));
        string lastRun = File.ReadAllText(Path.Combine(dir, Log.LastRunFile));
        Assert.Contains("rien à faire ; prochain post dû : Demo/demo-001 #2 le 2026-11-13 09:00 UTC ; en attente des clés X : Autre", lastRun);
        Directory.Delete(dir, true);
    }

    [Fact]
    public void Warning_writes_the_whole_run_and_the_rest_goes_straight_to_the_file()
    {
        string dir = TempDir();
        var log = new Log(dir, 30);
        log.Info("Début du passage");
        log.Warn("quelque chose");
        string file = Directory.GetFiles(dir, "publisher-*.log").Single();
        Assert.Equal(2, File.ReadAllLines(file).Length);  // écrit tout de suite, sans attendre la fin
        log.Info("Fin du passage");
        log.Dispose();
        var lines = File.ReadAllLines(file);
        Assert.Contains("INFO  Début du passage", lines[0]);
        Assert.Contains("WARN  quelque chose", lines[1]);
        Assert.Contains("INFO  Fin du passage", lines[2]);
        Assert.Contains("1 avertissement(s), rien publié", File.ReadAllText(Path.Combine(dir, Log.LastRunFile)));
        Directory.Delete(dir, true);
    }

    [Fact]
    public void Verbose_writes_every_run()
    {
        string dir = TempDir();
        var log = new Log(dir, 30, verbose: true);
        log.Info("Début du passage");
        log.Info("Fin du passage");
        log.Dispose();
        Assert.Equal(2, File.ReadAllLines(Directory.GetFiles(dir, "publisher-*.log").Single()).Count(l => l.Length > 0));
        Directory.Delete(dir, true);
    }

    [Fact]
    public void Dry_run_log_without_directory_writes_nothing() 
    {
        var log = new Log(null, 30);
        log.Error("visible dans la console seulement");
        log.Dispose();
        Assert.Null(log.FilePath);
        Assert.Null(log.LastRunPath);
    }
}

public class ServiceLoopTests
{
    [Fact]
    public void Next_pass_is_aligned_on_the_interval()
    {
        var i = TimeSpan.FromMinutes(15);
        Assert.Equal(new DateTimeOffset(2026, 11, 12, 8, 30, 0, TimeSpan.Zero), ServiceLoop.NextAligned(new(2026, 11, 12, 8, 17, 42, TimeSpan.Zero), i));
        Assert.Equal(new DateTimeOffset(2026, 11, 12, 8, 45, 0, TimeSpan.Zero), ServiceLoop.NextAligned(new(2026, 11, 12, 8, 30, 0, TimeSpan.Zero), i));
        // Heure locale : l'alignement se fait en UTC.
        Assert.Equal(new DateTimeOffset(2026, 11, 12, 9, 0, 0, TimeSpan.Zero), ServiceLoop.NextAligned(new(2026, 11, 12, 9, 59, 0, TimeSpan.FromHours(1)), i));
    }

    /// <summary>Horloge simulée : chaque attente fait avancer l'heure ; arrêt après <paramref name="maxPasses"/> passages.</summary>
    private static (ServiceLoop Loop, List<TimeSpan> Waits, List<string> Reports) Make(Func<int, Task> pass, int maxPasses, CancellationTokenSource cts)
    {
        var clock = new DateTimeOffset(2026, 11, 12, 8, 17, 0, TimeSpan.Zero);
        var waits = new List<TimeSpan>();
        var reports = new List<string>();
        int n = 0;
        ServiceLoop? loop = null;
        loop = new ServiceLoop(TimeSpan.FromMinutes(15),
            async ct => { n++; clock = clock.AddSeconds(40); if (n >= maxPasses) cts.Cancel(); await pass(n); },
            reports.Add,
            () => clock,
            (d, ct) => { waits.Add(d); clock += d; ct.ThrowIfCancellationRequested(); return Task.CompletedTask; });
        return (loop, waits, reports);
    }

    [Fact]
    public async Task Runs_a_pass_at_start_then_on_each_quarter_hour()
    {
        using var cts = new CancellationTokenSource();
        var (loop, waits, _) = Make(_ => Task.CompletedTask, 3, cts);
        await loop.RunAsync(cts.Token);
        Assert.Equal(3, loop.Passes);
        // 08:17:40 -> 08:30, puis 08:30:40 -> 08:45 : pas de dérive malgré la durée du passage.
        Assert.Equal(TimeSpan.FromSeconds(740), waits[0]);
        Assert.Equal(TimeSpan.FromSeconds(860), waits[1]);
    }

    [Fact]
    public async Task A_failing_pass_does_not_stop_the_loop()
    {
        using var cts = new CancellationTokenSource();
        var (loop, _, reports) = Make(n => n == 1 ? throw new InvalidOperationException("boum") : Task.CompletedTask, 3, cts);
        await loop.RunAsync(cts.Token);
        Assert.Equal(3, loop.Passes);
        Assert.Contains(reports, r => r.Contains("boum"));
    }

    [Fact]
    public async Task Stop_during_a_pass_ends_the_loop_without_error()
    {
        using var cts = new CancellationTokenSource();
        var (loop, _, reports) = Make(_ => { cts.Token.ThrowIfCancellationRequested(); return Task.CompletedTask; }, 1, cts);
        await loop.RunAsync(cts.Token);
        Assert.Equal(1, loop.Passes);
        Assert.Empty(reports);
    }

    [Fact]
    public async Task Each_pass_is_a_new_call_so_settings_are_reread()
    {
        // Chaque passage du service est un nouveau processus qui relit les réglages : la boucle rappelle le passage à chaque fois.
        using var cts = new CancellationTokenSource();
        var seen = new List<int>();
        var (loop, _, _) = Make(n => { seen.Add(n); return Task.CompletedTask; }, 4, cts);
        await loop.RunAsync(cts.Token);
        Assert.Equal([1, 2, 3, 4], seen);
    }
}

public class GitExecutableTests
{
    private static string? RepoRoot()
    {
        string? dir = AppContext.BaseDirectory;
        while (dir is not null && !Directory.Exists(Path.Combine(dir, ".git"))) dir = Path.GetDirectoryName(dir);
        return dir;
    }

    [Fact]
    public void Missing_git_is_an_error_not_a_crash()
    {
        string? repo = RepoRoot();
        if (repo is null) return;
        using var log = new Log(null, 0);
        var git = new Git(repo, new Settings.GitSettings { Executable = @"C:\nexiste\pas\git.exe" }, log);
        var r = git.Run("status");
        Assert.False(r.Ok);
        Assert.Contains("git introuvable", r.Error);
        Assert.Equal(1, log.Warnings);
    }

    [Fact]
    public void Configured_git_executable_is_used()
    {
        string? repo = RepoRoot();
        if (repo is null) return;
        using var log = new Log(null, 0);
        var r = new Git(repo, new Settings.GitSettings { Executable = "git" }, log).Run("--version");
        Assert.True(r.Ok);
        Assert.StartsWith("git version", r.Output);
    }

    [Fact]
    public void Check_git_says_why_it_fails()
    {
        string? repo = RepoRoot();
        if (repo is null) return;
        var bogus = new Settings { RepoPath = repo, Git = new Settings.GitSettings { Executable = @"C:\nexiste\pas\git.exe" } };
        var (ok, message) = GitCheck.Check(bogus);
        Assert.False(ok);
        Assert.Contains("git introuvable", message);

        var noRepo = new Settings { RepoPath = Path.Combine(Path.GetTempPath(), "xck-nexiste-" + Guid.NewGuid().ToString("N")) };
        (ok, message) = GitCheck.Check(noRepo);
        Assert.False(ok);
        Assert.Contains("répertoire du dépôt inaccessible", message);
    }
}

public class GitLocalModeTests
{
    private static readonly string[] RemoteCommands = ["pull", "push", "fetch", "ls-remote", "clone"];

    /// <summary>Dépôt temporaire avec un remote inaccessible et un status.json modifié non commité.</summary>
    private static string TempRepo()
    {
        string dir = Path.Combine(Path.GetTempPath(), "xck-gitlocal-" + Guid.NewGuid().ToString("N"));
        string camp = Path.Combine(dir, "accounts", "Demo", "threads", "demo-001");
        Directory.CreateDirectory(camp);
        File.WriteAllText(Path.Combine(camp, "status.json"), "{\"last_posted_index\": 0}\n");
        using var log = new Log(null, 0);
        var g = new Git(dir, new Settings.GitSettings { UserName = "Test", UserEmail = "test@example.com" }, log);
        g.Run("init", "-q", "-b", "main");
        g.Run("add", "-A");
        g.Run("commit", "-q", "-m", "init");
        g.Run("remote", "add", "origin", Path.Combine(dir, "nexiste", "remote.git"));
        File.WriteAllText(Path.Combine(camp, "status.json"), "{\"last_posted_index\": 1}\n");
        return dir;
    }

    private static void Delete(string dir)
    {
        foreach (var f in Directory.GetFiles(dir, "*", SearchOption.AllDirectories)) File.SetAttributes(f, FileAttributes.Normal);
        Directory.Delete(dir, true);
    }

    [Fact]
    public void Local_mode_never_contacts_the_remote_and_still_commits_the_state()
    {
        if (!new Git(Path.GetTempPath(), new Settings.GitSettings(), new Log(null, 0)).Run("--version").Ok) return;
        string dir = TempRepo();
        try
        {
            using var log = new Log(null, 0);
            var git = new Git(dir, new Settings.GitSettings { Sync = false, UserName = "Bot", UserEmail = "bot@example.com" }, log);
            Assert.True(git.StartPass("accounts"));   // commit de récupération du status.json modifié
            Assert.True(git.EndPass());               // pas de push
            Assert.DoesNotContain(git.Commands, c => RemoteCommands.Contains(c));
            Assert.Contains("commit", git.Commands);
            var status = git.Run("status", "--porcelain");
            Assert.Equal("", status.Output);
            Assert.Equal(0, log.Errors);

            var (ok, message) = GitCheck.Check(new Settings { RepoPath = dir, Git = new Settings.GitSettings { Sync = false } });
            Assert.True(ok, message);
            Assert.Contains("git local", message);
        }
        finally { Delete(dir); }
    }

    [Fact]
    public void Sync_mode_refuses_the_pass_when_the_remote_is_unreachable()
    {
        if (!new Git(Path.GetTempPath(), new Settings.GitSettings(), new Log(null, 0)).Run("--version").Ok) return;
        string dir = TempRepo();
        try
        {
            using var log = new Log(null, 0);
            var git = new Git(dir, new Settings.GitSettings { Sync = true, UserName = "Bot", UserEmail = "bot@example.com" }, log);
            Assert.False(git.StartPass("accounts"));
            Assert.Contains("pull", git.Commands);
        }
        finally { Delete(dir); }
    }
}

public class XAuthTests
{
    private sealed class Handler(Func<HttpRequestMessage, HttpResponseMessage> respond) : HttpMessageHandler
    {
        public List<string> Paths { get; } = [];
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            Assert.StartsWith("OAuth ", request.Headers.GetValues("Authorization").Single());
            Paths.Add(request.RequestUri!.AbsolutePath);
            return Task.FromResult(respond(request));
        }
    }

    /// <summary>Dépôt temporaire : un compte Demo (clés factices) avec deux campagnes dues.</summary>
    private static (string Repo, Account Account) MakeRepo()
    {
        string repo = Path.Combine(Path.GetTempPath(), "xck-auth-" + Guid.NewGuid().ToString("N"));
        string acc = Path.Combine(repo, "accounts", "Demo");
        foreach (string c in new[] { "a-001", "b-001" })
        {
            string dir = Path.Combine(acc, "threads", c);
            Directory.CreateDirectory(dir);
            File.WriteAllText(Path.Combine(dir, "status.json"), """{"status": "active", "last_posted_index": 0, "last_posted_at": null, "interval_days": 1, "post_time": "09:00"}""");
            File.WriteAllText(Path.Combine(dir, "posts.json"), """[{"index": 1, "simple": "Demo #1: first", "status": "checked"}]""");
        }
        File.WriteAllText(Path.Combine(acc, Settings.AccountFile), """{"x": {"username": "demoaccount"}}""");
        File.WriteAllText(Path.Combine(acc, Settings.AccountLocalFile), """{"x": {"apiKey": "k", "apiKeySecret": "s", "accessToken": "1-t", "accessTokenSecret": "ts"}}""");
        return (repo, new Account(acc));
    }

    private static Settings MakeSettings(string repo, Account account)
    {
        var s = new Settings { RepoPath = repo, DelayBetweenPostsSeconds = 0, Git = new Settings.GitSettings { Enabled = false }, X = new Settings.XSettings { ApiBaseUrl = "https://api.example" } };
        s.LoadAccountFiles(account);
        return s;
    }

    [Fact]
    public async Task A_401_stops_the_account_at_the_first_call()
    {
        var (repo, account) = MakeRepo();
        try
        {
            var settings = MakeSettings(repo, account);
            var handler = new Handler(_ => new HttpResponseMessage(HttpStatusCode.Unauthorized) { Content = new StringContent("""{"title":"Unauthorized","status":401}""") });
            using var log = new Log(null, 0);
            using var x = new XClient(settings.XFor("Demo"), log, handler);
            var publisher = new Publisher(settings, account, log, x, null, new RunOptions(false, new DateTimeOffset(2026, 11, 12, 9, 5, 0, TimeSpan.Zero), null));
            int n = await publisher.RunAsync(default);
            Assert.Equal(0, n);
            Assert.Single(handler.Paths);              // la 2e campagne n'a pas été tentée
            Assert.NotNull(publisher.AuthFailure);
            Assert.Equal(1, log.Errors);
            Assert.Contains("--check-x", log.FirstError);
        }
        finally { Directory.Delete(repo, true); }
    }

    [Fact]
    public void A_refused_account_stays_paused_until_its_keys_change()
    {
        var (repo, account) = MakeRepo();
        string logs = Path.Combine(repo, "logs");
        try
        {
            Assert.Null(XAuthBlock.Blocked(logs, account));
            XAuthBlock.Block(logs, account, "HTTP 401");
            Assert.Contains("HTTP 401", XAuthBlock.Blocked(logs, account));
            // Nouvelles clés : le fichier change, le compte est réessayé.
            File.WriteAllText(Path.Combine(account.Directory, Settings.AccountLocalFile), """{"x": {"apiKey": "k2", "apiKeySecret": "s2", "accessToken": "1-t2", "accessTokenSecret": "ts2"}}""");
            Assert.Null(XAuthBlock.Blocked(logs, account));
        }
        finally { Directory.Delete(repo, true); }
    }

    [Fact]
    public async Task Check_x_reports_the_account_rights_and_clock()
    {
        var (repo, account) = MakeRepo();
        try
        {
            var settings = MakeSettings(repo, account);
            var ok = new Handler(_ =>
            {
                var r = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("""{"data":{"id":"1","username":"DemoAccount"}}""") };
                r.Headers.Add("x-access-level", "read-write");
                r.Headers.Date = DateTimeOffset.UtcNow;
                return r;
            });
            var (good, lines) = await XCheck.CheckAccountAsync(settings, account, ok);
            Assert.True(good, string.Join("\n", lines));
            Assert.Equal(["/2/users/me"], ok.Paths);
            Assert.Contains(lines, l => l.Contains("@DemoAccount") && !l.Contains("DIFFÉRENT"));
            Assert.DoesNotContain(lines, l => l.Contains("ts") && l.Contains("AccessTokenSecret :") && l.Contains("ts2"));

            var refused = new Handler(_ =>
            {
                var r = new HttpResponseMessage(HttpStatusCode.Unauthorized) { Content = new StringContent("""{"title":"Unauthorized"}""") };
                r.Headers.Date = DateTimeOffset.UtcNow.AddMinutes(-5);
                return r;
            });
            var (bad, badLines) = await XCheck.CheckAccountAsync(settings, account, refused);
            Assert.False(bad);
            Assert.Contains(badLines, l => l.Contains("HTTP 401"));
            Assert.Contains(badLines, l => l.Contains("TROP GRAND"));
            Assert.Contains(badLines, l => l.Contains("Bearer Token"));  // l'explication des causes
        }
        finally { Directory.Delete(repo, true); }
    }

    [Fact]
    public void Key_description_never_shows_the_key()
    {
        string secret = "AAAAAAAAAAAAAAAAAAAAAMLheAAAAAAA0%2BuSeid%2BULvsea4JtiGRiSDSJSI%3DEUifiRBkKG5E2XzMDjRfl76ZC9Ub0wnz4XsNiRVBChTYbJcE3F";
        var d = XCheck.DescribeKey("AccessToken", " " + secret + "\n").Single();
        Assert.DoesNotContain(secret[10..30], d);
        Assert.Contains("Bearer Token", d);
        Assert.Contains("espaces", d);
    }
}

public class ClientIdDetectionTests
{
    [Fact]
    public void Oauth2_client_id_is_recognised()
    {
        // Base64 de « …:1:ci », la forme des Client ID OAuth 2.0 de X (valeur inventée).
        string fakeClientId = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes("M1Y5R3BMVy13Qmp:1:ci")).TrimEnd('=');
        Assert.True(XCheck.LooksLikeOAuth2ClientId(fakeClientId));
        Assert.Contains("Client ID OAuth 2.0", XCheck.DescribeKey("AccessToken", fakeClientId).Single());
        Assert.False(XCheck.LooksLikeOAuth2ClientId("1234567890-AbCdEfGhIjKlMnOpQrStUvWxYz0123456789ab"));
    }
}
