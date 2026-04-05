using MimironsGoldOMatic.Desktop.Services;
using Xunit;

namespace MimironsGoldOMatic.Desktop.UnitTests;

public sealed class WoWRunCommandsTests
{
    [Fact]
    public void NotifyWinnerWhisper_escapes_backslash_and_quotes()
    {
        var id = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var line = WoWRunCommands.NotifyWinnerWhisper(id, "A\"B\\C");
        Assert.Equal("/run NotifyWinnerWhisper(\"11111111-1111-1111-1111-111111111111\",\"A\\\"B\\\\C\")", line);
    }

    [Fact]
    public void NotifyWinnerWhisper_plain_name()
    {
        var id = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var line = WoWRunCommands.NotifyWinnerWhisper(id, "Thrall");
        Assert.Equal("/run NotifyWinnerWhisper(\"22222222-2222-2222-2222-222222222222\",\"Thrall\")", line);
    }
}
