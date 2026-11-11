using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace XCryptoKeys.Publisher;

/// <summary>
/// Longueur d'un post comme X la compte (règles twitter-text v3) :
/// la plupart des caractères latins comptent 1, les autres (CJK, emoji, « ≈ », « → »…) comptent 2,
/// et chaque URL compte 23.
/// </summary>
public static partial class TweetText
{
    public const int UrlLength = 23;

    [GeneratedRegex(@"https?://\S+", RegexOptions.IgnoreCase)]
    private static partial Regex UrlRegex();

    public static int WeightedLength(string text)
    {
        text = text.Normalize(NormalizationForm.FormC);
        int length = 0;
        foreach (Match m in UrlRegex().Matches(text)) length += UrlLength;
        string rest = UrlRegex().Replace(text, "");

        var e = StringInfo.GetTextElementEnumerator(rest);
        while (e.MoveNext())
        {
            string element = (string)e.Current;
            int cp = char.ConvertToUtf32(element, 0);
            bool multi = element.Length > (cp > 0xFFFF ? 2 : 1);
            if (multi || cp > 0xFFFF)
            {
                // Emoji (avec modificateurs ou ZWJ) ou caractère hors BMP : 2.
                length += 2;
                continue;
            }
            length += IsLight(cp) ? 1 : 2;
        }
        return length;
    }

    private static bool IsLight(int cp) =>
        cp <= 4351 || (cp >= 8192 && cp <= 8205) || (cp >= 8208 && cp <= 8223) || (cp >= 8242 && cp <= 8247);

    /// <summary>Ajoute le hashtag de campagne à la fin du texte s'il n'y est pas déjà (insensible à la casse).</summary>
    public static string AppendHashtag(string text, string? hashtag)
    {
        if (string.IsNullOrWhiteSpace(hashtag)) return text;
        string tag = hashtag.Trim();
        if (!tag.StartsWith('#')) tag = "#" + tag;
        var already = new Regex(@"(?<![\w#])" + Regex.Escape(tag) + @"(?!\w)", RegexOptions.IgnoreCase);
        if (already.IsMatch(text)) return text;
        return text.TrimEnd() + " " + tag;
    }
}
