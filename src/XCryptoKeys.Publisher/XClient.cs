using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json.Nodes;

namespace XCryptoKeys.Publisher;

public sealed class XApiException(string message, HttpStatusCode status, string body) : Exception(message)
{
    public HttpStatusCode Status { get; } = status;
    public string Body { get; } = body;
}

/// <summary>Client minimal de l'API X v2 : upload d'image, texte alternatif, création de post. Auth OAuth 1.0a.</summary>
public sealed class XClient : IDisposable
{
    private readonly HttpClient _http;
    private readonly OAuth1 _oauth;
    private readonly string _base;
    private readonly Log _log;

    /// <summary>Nombre d'appels envoyés à l'API (utile en pay-per-use).</summary>
    public int Calls { get; private set; }

    public XClient(Settings.XSettings s, Log log, HttpMessageHandler? handler = null)
    {
        _http = handler is null ? new HttpClient() : new HttpClient(handler);
        _http.Timeout = TimeSpan.FromSeconds(100);
        _http.DefaultRequestHeaders.UserAgent.ParseAdd("xck-publisher/1.0");
        _oauth = new OAuth1(s.ApiKey, s.ApiKeySecret, s.AccessToken, s.AccessTokenSecret);
        _base = s.ApiBaseUrl.TrimEnd('/');
        _log = log;
    }

    /// <summary>POST /2/media/upload (upload simple, multipart). Renvoie l'id du média.</summary>
    public async Task<string> UploadImageAsync(string path, CancellationToken ct)
    {
        var info = new FileInfo(path);
        if (info.Length > 5 * 1024 * 1024)
            throw new InvalidOperationException($"Image trop lourde pour X ({info.Length / 1024} Ko > 5 Mo) : {path}");

        string mime = Path.GetExtension(path).ToLowerInvariant() switch
        {
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".webp" => "image/webp",
            ".gif" => "image/gif",
            _ => "application/octet-stream",
        };

        using var content = new MultipartFormDataContent();
        var file = new ByteArrayContent(await File.ReadAllBytesAsync(path, ct));
        file.Headers.ContentType = new MediaTypeHeaderValue(mime);
        content.Add(file, "media", Path.GetFileName(path));
        content.Add(new StringContent("tweet_image"), "media_category");
        content.Add(new StringContent(mime), "media_type");

        var json = await SendAsync(HttpMethod.Post, "/2/media/upload", content, ct);
        string? id = (string?)json["data"]?["id"] ?? (string?)json["media_id_string"];
        return id ?? throw new XApiException("Réponse d'upload sans id de média.", HttpStatusCode.OK, json.ToJsonString());
    }

    /// <summary>POST /2/media/metadata : texte alternatif de l'image (1000 caractères max).</summary>
    public async Task SetAltTextAsync(string mediaId, string altText, CancellationToken ct)
    {
        if (altText.Length > 1000) altText = altText[..1000];
        var body = new JsonObject
        {
            ["id"] = mediaId,
            ["metadata"] = new JsonObject { ["alt_text"] = new JsonObject { ["text"] = altText } },
        };
        await SendAsync(HttpMethod.Post, "/2/media/metadata", Json(body), ct);
    }

    /// <summary>POST /2/tweets. Renvoie l'id du post créé.</summary>
    public async Task<string> CreatePostAsync(string text, IReadOnlyList<string> mediaIds, CancellationToken ct)
    {
        var body = new JsonObject { ["text"] = text };
        if (mediaIds.Count > 0)
            body["media"] = new JsonObject { ["media_ids"] = new JsonArray(mediaIds.Select(id => (JsonNode)JsonValue.Create(id)!).ToArray()) };
        var json = await SendAsync(HttpMethod.Post, "/2/tweets", Json(body), ct);
        return (string?)json["data"]?["id"]
            ?? throw new XApiException("Réponse de création de post sans id.", HttpStatusCode.OK, json.ToJsonString());
    }

    private static StringContent Json(JsonNode body) =>
        new(body.ToJsonString(), Encoding.UTF8, "application/json");

    private async Task<JsonObject> SendAsync(HttpMethod method, string path, HttpContent content, CancellationToken ct)
    {
        var uri = new Uri(_base + path);
        using var req = new HttpRequestMessage(method, uri) { Content = content };
        req.Headers.TryAddWithoutValidation("Authorization", _oauth.BuildHeader(method.Method, uri));

        Calls++;
        using var resp = await _http.SendAsync(req, ct);
        string text = await resp.Content.ReadAsStringAsync(ct);
        _log.Info($"API X {method.Method} {path} -> {(int)resp.StatusCode}");

        if (!resp.IsSuccessStatusCode)
        {
            string detail = text.Length > 1000 ? text[..1000] : text;
            if (resp.StatusCode == HttpStatusCode.TooManyRequests &&
                resp.Headers.TryGetValues("x-rate-limit-reset", out var reset) &&
                long.TryParse(reset.FirstOrDefault(), out long epoch))
            {
                detail += $" (limite levée à {DateTimeOffset.FromUnixTimeSeconds(epoch):yyyy-MM-ddTHH:mm:ssZ})";
            }
            throw new XApiException($"API X {method.Method} {path} : HTTP {(int)resp.StatusCode} {detail}", resp.StatusCode, text);
        }

        if (string.IsNullOrWhiteSpace(text)) return new JsonObject();
        var node = JsonNode.Parse(text) as JsonObject ?? new JsonObject();
        if (node["errors"] is JsonArray errs && errs.Count > 0 && node["data"] is null)
            throw new XApiException($"API X {method.Method} {path} : {errs.ToJsonString()}", resp.StatusCode, text);
        return node;
    }

    public void Dispose() => _http.Dispose();
}
