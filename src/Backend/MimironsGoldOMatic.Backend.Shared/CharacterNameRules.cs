using System.Globalization;
using System.Text;

namespace MimironsGoldOMatic.Backend.Shared;

/// <summary>
/// Shared rules for <c>CharacterName</c> per <c>docs/overview/SPEC.md</c> §4 (length, Latin/Cyrillic letters only).
/// </summary>
public static class CharacterNameRules
{
    /// <summary>
    /// Returns true if <paramref name="value"/> is non-empty after trim, length 2–12 (inclusive),
    /// and every UTF-32 scalar is a Unicode letter in a Latin or Cyrillic script block.
    /// </summary>
    public static bool IsValid(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        var trimmed = value.Trim();
        if (trimmed.Length is < 2 or > 12)
            return false;

        foreach (var rune in trimmed.EnumerateRunes())
        {
            if (!IsLatinOrCyrillicLetter(rune))
                return false;
        }

        return true;
    }

    private static bool IsLatinOrCyrillicLetter(Rune rune)
    {
        var cat = Rune.GetUnicodeCategory(rune);
        if (cat is not UnicodeCategory.UppercaseLetter
            and not UnicodeCategory.LowercaseLetter
            and not UnicodeCategory.TitlecaseLetter
            and not UnicodeCategory.ModifierLetter
            and not UnicodeCategory.OtherLetter)
            return false;

        var v = rune.Value;
        return IsCyrillicScriptLetter(v) || IsLatinScriptLetter(v);
    }

    private static bool IsCyrillicScriptLetter(int v) =>
        v is >= 0x0400 and <= 0x04FF // Cyrillic
            or >= 0x0500 and <= 0x052F // Cyrillic Supplement
            or >= 0x1C80 and <= 0x1C8F // Cyrillic Extended-C
            or >= 0x2DE0 and <= 0x2DFF // Cyrillic Extended-A
            or >= 0xA640 and <= 0xA69F; // Cyrillic Extended-B

    /// <summary>
    /// Latin script letters in Unicode blocks used for typical realm/character names (MVP).
    /// </summary>
    private static bool IsLatinScriptLetter(int v) =>
        v is >= 0x0041 and <= 0x005A // A–Z
            or >= 0x0061 and <= 0x007A // a–z
            or >= 0x00C0 and <= 0x00D6 // Latin-1 letters (before ×)
            or >= 0x00D8 and <= 0x00F6 // (after ÷)
            or >= 0x00F8 and <= 0x00FF // (after ø)
            or >= 0x0100 and <= 0x024F // Latin Extended-A / B
            or >= 0x1E00 and <= 0x1EFF // Latin Extended Additional
            or >= 0x2C60 and <= 0x2C7F // Latin Extended-C
            or (>= 0xA722 and <= 0xA787); // Latin Extended-D (letter subrange)
}
