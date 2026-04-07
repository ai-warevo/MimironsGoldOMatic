using MimironsGoldOMatic.Backend.Application.Roulette.Enrollment;
using Xunit;

namespace MimironsGoldOMatic.Backend.UnitTests.Unit;

/// <summary>Covers <see cref="TwGoldChatEnrollmentParser"/> per EventSub chat text rules (single token name, prefix match).</summary>
[Trait("Category", "Unit")]
public sealed class TwGoldChatEnrollmentParserTests
{
    [Theory]
    [InlineData("!twgold Abcd", "Abcd")]
    [InlineData("  !twgold   Xyzz  ", "Xyzz")]
    [InlineData("!TWGOLD hero", "hero")]
    [InlineData("!TwGoLd\tNameHere\t", "NameHere")]
    public void Should_ReturnCharacterName_WhenLineMatchesPrefixAndSingleToken(string line, string expected)
    {
        Assert.True(TwGoldChatEnrollmentParser.TryGetCharacterName(line, out var name));
        Assert.Equal(expected, name);
    }

    /// <summary>Boundary: longest single token still accepted by regex (parser does not enforce 2–12 letter rule).</summary>
    [Fact]
    public void Should_AcceptLongSingleToken_AsRawName()
    {
        var token = new string('a', 50);
        Assert.True(TwGoldChatEnrollmentParser.TryGetCharacterName($"!twgold {token}", out var name));
        Assert.Equal(token, name);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("hello")]
    [InlineData("!twgold")]
    [InlineData("!twgold ")]
    [InlineData("!twgold A B")]
    [InlineData("prefix !twgold Abcd")]
    public void Should_ReturnFalse_WhenLineDoesNotMatch(string line)
    {
        Assert.False(TwGoldChatEnrollmentParser.TryGetCharacterName(line, out _));
    }
}
