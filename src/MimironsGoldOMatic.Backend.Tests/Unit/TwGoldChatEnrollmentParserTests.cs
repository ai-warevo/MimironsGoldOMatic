using MimironsGoldOMatic.Backend.Services;
using Xunit;

namespace MimironsGoldOMatic.Backend.Tests.Unit;

[Trait("Category", "Unit")]
public sealed class TwGoldChatEnrollmentParserTests
{
    [Theory]
    [InlineData("!twgold Abcd", "Abcd")]
    [InlineData("  !twgold   Xyzz  ", "Xyzz")]
    [InlineData("!TWGOLD hero", "hero")]
    [InlineData("!TwGoLd\tNameHere\t", "NameHere")]
    public void TryGetCharacterName_matches_spec_prefix_case_insensitive(string line, string expected)
    {
        Assert.True(TwGoldChatEnrollmentParser.TryGetCharacterName(line, out var name));
        Assert.Equal(expected, name);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("hello")]
    [InlineData("!twgold")]
    [InlineData("!twgold ")]
    [InlineData("!twgold A B")]
    [InlineData("prefix !twgold Abcd")]
    public void TryGetCharacterName_rejects_non_matching_lines(string line)
    {
        Assert.False(TwGoldChatEnrollmentParser.TryGetCharacterName(line, out _));
    }
}
