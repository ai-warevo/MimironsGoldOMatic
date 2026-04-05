using MimironsGoldOMatic.Desktop.Services;
using Xunit;

namespace MimironsGoldOMatic.Desktop.UnitTests;

public sealed class DesktopConnectionContextTests
{
    [Fact]
    public void GetConnection_returns_settings_base_url_and_api_key_or_empty()
    {
        var ctx = new DesktopConnectionContext
        {
            Settings = new DesktopUserSettings { BaseUrl = "https://api.example/" },
            ApiKey = "secret",
        };
        var (url, key) = ctx.GetConnection();
        Assert.Equal("https://api.example/", url);
        Assert.Equal("secret", key);
    }

    [Fact]
    public void GetConnection_api_key_null_becomes_empty_string()
    {
        var ctx = new DesktopConnectionContext { Settings = new DesktopUserSettings(), ApiKey = null };
        Assert.Equal("", ctx.GetConnection().ApiKey);
    }

    [Theory]
    [InlineData(1, 5)]
    [InlineData(4, 5)]
    [InlineData(5, 5)]
    [InlineData(15, 15)]
    [InlineData(600, 600)]
    [InlineData(601, 600)]
    [InlineData(9000, 600)]
    public void GetClampedPollIntervalSeconds_clamps_to_5_600(int input, int expected)
    {
        var ctx = new DesktopConnectionContext { Settings = new DesktopUserSettings { PollIntervalSeconds = input } };
        Assert.Equal(expected, ctx.GetClampedPollIntervalSeconds());
    }
}
