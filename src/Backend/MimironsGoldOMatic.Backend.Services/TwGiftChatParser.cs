using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace MimironsGoldOMatic.Backend.Services;

public static partial class TwGiftChatParser
{
    public static bool TryGetCharacterName(string chatText, [NotNullWhen(true)] out string? characterName)
    {
        characterName = null;
        var m = Pattern().Match(chatText.Trim());
        if (!m.Success)
            return false;
        characterName = m.Groups[1].Value;
        return true;
    }

    [GeneratedRegex("^!twgift\\s+(\\S+)\\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex Pattern();
}
