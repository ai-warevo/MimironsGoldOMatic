using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace MimironsGoldOMatic.Backend.Services;

/// <summary>Parses EventSub chat lines for <c>!twgold &lt;CharacterName&gt;</c> (<c>docs/SPEC.md</c> §5).</summary>
public static partial class TwGoldChatEnrollmentParser
{
    /// <summary>
    /// When the trimmed message matches <c>!twgold &lt;name&gt;</c> (prefix case-insensitive, single token name),
    /// returns the raw name token (not validated against <see cref="MimironsGoldOMatic.Shared.CharacterNameRules"/>).
    /// </summary>
    public static bool TryGetCharacterName(string chatText, [NotNullWhen(true)] out string? characterName)
    {
        characterName = null;
        var trimmed = chatText.Trim();
        var m = Pattern().Match(trimmed);
        if (!m.Success)
            return false;
        characterName = m.Groups[1].Value;
        return true;
    }

    [GeneratedRegex("^!twgold\\s+(\\S+)\\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex Pattern();
}
