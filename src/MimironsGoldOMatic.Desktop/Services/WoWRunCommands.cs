namespace MimironsGoldOMatic.Desktop.Services;

public static class WoWRunCommands
{
    public static string NotifyWinnerWhisper(Guid payoutId, string characterName)
    {
        var n = characterName.Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("\"", "\\\"", StringComparison.Ordinal);
        return $"/run NotifyWinnerWhisper(\"{payoutId:D}\",\"{n}\")";
    }
}
