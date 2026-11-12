namespace XCryptoKeys.Publisher;

/// <summary>
/// Boucle du mode service : un passage tout de suite, puis un à chaque multiple de l'intervalle (heure UTC),
/// par exemple :00, :15, :30, :45 pour 15 minutes, sans dériver avec la durée des passages.
/// Un passage en échec est signalé et la boucle continue ; l'arrêt est pris en compte entre deux passages.
/// </summary>
public sealed class ServiceLoop(
    TimeSpan interval,
    Func<CancellationToken, Task> pass,
    Action<string> report,
    Func<DateTimeOffset>? now = null,
    Func<TimeSpan, CancellationToken, Task>? delay = null)
{
    private readonly Func<DateTimeOffset> _now = now ?? (() => DateTimeOffset.UtcNow);
    private readonly Func<TimeSpan, CancellationToken, Task> _delay = delay ?? ((d, ct) => Task.Delay(d, ct));

    public TimeSpan Interval => interval;
    /// <summary>Nombre de passages lancés.</summary>
    public int Passes { get; private set; }

    /// <summary>Prochain multiple de l'intervalle strictement après <paramref name="now"/> (en UTC).</summary>
    public static DateTimeOffset NextAligned(DateTimeOffset now, TimeSpan interval)
    {
        long step = interval.Ticks;
        long next = (now.UtcTicks / step + 1) * step;
        return new DateTimeOffset(next, TimeSpan.Zero);
    }

    public async Task RunAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                Passes++;
                await pass(ct);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                report($"passage en échec : {ex.Message}");
            }

            var wait = NextAligned(_now(), interval) - _now();
            if (wait < TimeSpan.Zero) wait = TimeSpan.Zero;
            try
            {
                await _delay(wait, ct);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }
}
