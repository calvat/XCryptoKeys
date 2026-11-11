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
