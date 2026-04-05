using System.Text;
using MimironsGoldOMatic.Shared;

namespace MimironsGoldOMatic.Desktop.Services;

/// <summary>Builds <c>/run ReceiveGold("…")</c> lines each under 255 characters (<c>docs/SPEC.md</c> §8).</summary>
public static class ReceiveGoldCommandChunker
{
    private const int MaxLineLength = 254;
    private const string Prefix = "/run ReceiveGold(\"";
    private const string Suffix = "\")";

    /// <summary>Copper per WoW: 1 gold = 10_000 copper.</summary>
    public static long GoldToCopper(long goldAmount) => goldAmount * 10_000L;

    public static IReadOnlyList<string> BuildRunCommands(IEnumerable<PayoutDto> payouts)
    {
        var entries = payouts
            .Select(p => $"{p.Id:D}:{p.CharacterName}:{GoldToCopper(p.GoldAmount)};")
            .ToList();
        if (entries.Count == 0)
            return [];

        var chunks = new List<string>();
        var current = new StringBuilder();
        foreach (var piece in entries)
        {
            var trialLen = (current.Length == 0 ? 0 : current.Length) + piece.Length;
            var lineLen = Prefix.Length + trialLen + Suffix.Length;
            if (lineLen <= MaxLineLength)
            {
                current.Append(piece);
                continue;
            }

            if (current.Length > 0)
            {
                chunks.Add(Prefix + current + Suffix);
                current.Clear();
            }

            var single = Prefix.Length + piece.Length + Suffix.Length;
            if (single > MaxLineLength)
                throw new InvalidOperationException(
                    $"Single ReceiveGold entry exceeds WoW line limit ({MaxLineLength}): shorten character name or reduce gold.");

            current.Append(piece);
        }

        if (current.Length > 0)
            chunks.Add(Prefix + current + Suffix);

        return chunks;
    }
}
