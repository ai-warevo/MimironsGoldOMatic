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

    public static string RequestAllInventoryItems(Guid giftRequestId)
    {
        return $"/run MGM_RequestGiftItems(\"{giftRequestId:D}\")";
    }

    public static string RequestGiftConfirmation(Guid giftRequestId, string characterName)
    {
        var n = EscapeLuaString(characterName);
        return $"/run MGM_RequestGiftConfirmation(\"{giftRequestId:D}\",\"{n}\")";
    }

    private static string EscapeLuaString(string value) =>
        value.Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("\"", "\\\"", StringComparison.Ordinal);
}
