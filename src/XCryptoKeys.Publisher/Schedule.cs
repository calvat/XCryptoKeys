using System.Globalization;

namespace XCryptoKeys.Publisher;

public static class Schedule
{
    /// <summary>
    /// Date et heure (UTC) où le prochain post est dû.
    /// Rien encore publié : aujourd'hui à post_time. Sinon : jour de last_posted_at + interval_days, à post_time.
    /// Après une coupure, le post en retard part au premier passage, puis le rythme reprend à partir de ce jour-là.
    /// </summary>
    public static DateTimeOffset NextDue(string? lastPostedAt, int intervalDays, string postTime, DateTimeOffset now)
    {
        TimeSpan time = ParsePostTime(postTime);
        DateOnly day;
        if (string.IsNullOrWhiteSpace(lastPostedAt))
        {
            day = DateOnly.FromDateTime(now.UtcDateTime);
        }
        else
        {
            day = ParseDay(lastPostedAt).AddDays(Math.Max(1, intervalDays));
        }
        return new DateTimeOffset(day.ToDateTime(TimeOnly.FromTimeSpan(time)), TimeSpan.Zero);
    }

    public static TimeSpan ParsePostTime(string postTime)
    {
        if (TimeSpan.TryParseExact(postTime, ["hh\\:mm", "h\\:mm", "hh\\:mm\\:ss"], CultureInfo.InvariantCulture, out var t)
            && t >= TimeSpan.Zero && t < TimeSpan.FromDays(1))
            return t;
        throw new FormatException($"post_time invalide : « {postTime} » (attendu HH:MM en UTC).");
    }

    public static DateOnly ParseDay(string value)
    {
        if (DateOnly.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d))
            return d;
        if (DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var dt))
            return DateOnly.FromDateTime(dt.UtcDateTime);
        throw new FormatException($"last_posted_at invalide : « {value} » (attendu ISO 8601).");
    }
}
