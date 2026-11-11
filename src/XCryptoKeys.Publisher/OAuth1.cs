using System.Security.Cryptography;
using System.Text;

namespace XCryptoKeys.Publisher;

/// <summary>Signature OAuth 1.0a (HMAC-SHA1), contexte utilisateur.</summary>
public sealed class OAuth1(string consumerKey, string consumerSecret, string token, string tokenSecret)
{
    /// <summary>
    /// Construit l'en-tête Authorization. Seuls les paramètres de query string (et de formulaire
    /// x-www-form-urlencoded, non utilisés ici) entrent dans la signature ; les corps JSON et multipart n'y entrent pas.
    /// </summary>
    public string BuildHeader(string method, Uri uri, IEnumerable<KeyValuePair<string, string>>? extraParams = null,
        string? nonce = null, string? timestamp = null)
    {
        nonce ??= Convert.ToHexString(RandomNumberGenerator.GetBytes(16)).ToLowerInvariant();
        timestamp ??= DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();

        var oauth = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            ["oauth_consumer_key"] = consumerKey,
            ["oauth_nonce"] = nonce,
            ["oauth_signature_method"] = "HMAC-SHA1",
            ["oauth_timestamp"] = timestamp,
            ["oauth_token"] = token,
            ["oauth_version"] = "1.0",
        };

        var all = new List<KeyValuePair<string, string>>(oauth);
        if (!string.IsNullOrEmpty(uri.Query))
        {
            foreach (string part in uri.Query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries))
            {
                int eq = part.IndexOf('=');
                string k = Uri.UnescapeDataString(eq < 0 ? part : part[..eq]);
                string v = eq < 0 ? "" : Uri.UnescapeDataString(part[(eq + 1)..].Replace('+', ' '));
                all.Add(new(k, v));
            }
        }
        if (extraParams is not null) all.AddRange(extraParams);

        string paramString = string.Join("&", all
            .Select(p => (K: Encode(p.Key), V: Encode(p.Value)))
            .OrderBy(p => p.K, StringComparer.Ordinal).ThenBy(p => p.V, StringComparer.Ordinal)
            .Select(p => $"{p.K}={p.V}"));

        string baseUrl = uri.GetLeftPart(UriPartial.Path);
        string signatureBase = $"{method.ToUpperInvariant()}&{Encode(baseUrl)}&{Encode(paramString)}";
        string signingKey = $"{Encode(consumerSecret)}&{Encode(tokenSecret)}";
        using var hmac = new HMACSHA1(Encoding.ASCII.GetBytes(signingKey));
        string signature = Convert.ToBase64String(hmac.ComputeHash(Encoding.ASCII.GetBytes(signatureBase)));

        oauth["oauth_signature"] = signature;
        return "OAuth " + string.Join(", ", oauth.Select(p => $"{Encode(p.Key)}=\"{Encode(p.Value)}\""));
    }

    /// <summary>Encodage RFC 3986 : seuls A-Z a-z 0-9 - . _ ~ restent tels quels.</summary>
    public static string Encode(string value)
    {
        var sb = new StringBuilder();
        foreach (byte b in Encoding.UTF8.GetBytes(value))
        {
            char c = (char)b;
            if ((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || (c >= '0' && c <= '9') || c is '-' or '.' or '_' or '~')
                sb.Append(c);
            else
                sb.Append('%').Append(b.ToString("X2"));
        }
        return sb.ToString();
    }
}
