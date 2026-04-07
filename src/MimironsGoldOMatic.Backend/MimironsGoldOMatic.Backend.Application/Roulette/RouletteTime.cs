namespace MimironsGoldOMatic.Backend.Application.Roulette;

public static class RouletteTime
{
    public static DateTime FloorToFiveMinuteUtc(DateTime utcNow)
    {
        utcNow = DateTime.SpecifyKind(utcNow, DateTimeKind.Utc);
        var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var minutes = (long)Math.Floor((utcNow - epoch).TotalMinutes);
        var block = minutes / 5 * 5;
        return epoch.AddMinutes(block);
    }

    /// <summary>Next UTC :00/:05/... boundary strictly after <paramref name="utcNow"/>.</summary>
    public static DateTime NextSpinBoundaryUtc(DateTime utcNow)
    {
        utcNow = DateTime.SpecifyKind(utcNow, DateTimeKind.Utc);
        var floor = FloorToFiveMinuteUtc(utcNow);
        var next = floor.AddMinutes(5);
        while (next <= utcNow)
            next = next.AddMinutes(5);
        return next;
    }

    public const int CollectingSeconds = 4 * 60;
    public const int SpinningSeconds = 30;
}
