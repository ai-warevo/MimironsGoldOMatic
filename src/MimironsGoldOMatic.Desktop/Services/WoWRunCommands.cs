namespace MimironsGoldOMatic.Desktop.Services;

public static class WoWRunCommands
{
    public static string NotifyWinnerWhisper(Guid payoutId, string characterName)
    {
        var n = characterName.Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("\"", "\\\"", StringComparison.Ordinal);
        return $"/run NotifyWinnerWhisper(\"{payoutId:D}\",\"{n}\")";
    }

    public static string ChatFrameMessage(string message)
    {
        var escaped = EscapeLuaString(message);
        return $"/run DEFAULT_CHAT_FRAME:AddMessage(\"{escaped}\")";
    }

    private static string EscapeLuaString(string value) =>
        value.Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("\"", "\\\"", StringComparison.Ordinal);
}
