using MimironsGoldOMatic.Desktop.Services;
using MimironsGoldOMatic.Desktop.Services.Updates;
using Moq;
using Xunit;

namespace MimironsGoldOMatic.Desktop.UnitTests;

public sealed class UpdateServiceTests
{
    [Fact]
    public async Task CheckForUpdatesAsync_when_remote_higher_sets_update_available()
    {
        var api = new Mock<IEbsDesktopClient>(MockBehavior.Strict);
        api.Setup(x => x.GetVersionInfoAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new VersionInfoDto("1.2.4", "https://example/releases/1.2.4", null, null, null));
        var provider = Mock.Of<IAppVersionProvider>(x => x.GetCurrentVersion() == "1.2.3");

        var service = new UpdateService(api.Object, provider);
        var result = await service.CheckForUpdatesAsync();

        Assert.True(result.IsSuccess);
        Assert.True(result.IsUpdateAvailable);
        Assert.Equal("1.2.3", result.CurrentVersion);
        Assert.Equal("1.2.4", result.LatestVersion);
    }

    [Fact]
    public async Task CheckForUpdatesAsync_when_remote_equal_sets_up_to_date()
    {
        var api = new Mock<IEbsDesktopClient>(MockBehavior.Strict);
        api.Setup(x => x.GetVersionInfoAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new VersionInfoDto("2.0.0", null, null, null, null));
        var provider = Mock.Of<IAppVersionProvider>(x => x.GetCurrentVersion() == "2.0.0");

        var service = new UpdateService(api.Object, provider);
        var result = await service.CheckForUpdatesAsync();

        Assert.True(result.IsSuccess);
        Assert.False(result.IsUpdateAvailable);
    }

    [Fact]
    public async Task CheckForUpdatesAsync_when_api_fails_returns_failure_result()
    {
        var api = new Mock<IEbsDesktopClient>(MockBehavior.Strict);
        api.Setup(x => x.GetVersionInfoAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("down"));
        var provider = Mock.Of<IAppVersionProvider>(x => x.GetCurrentVersion() == "3.1.0");

        var service = new UpdateService(api.Object, provider);
        var result = await service.CheckForUpdatesAsync();

        Assert.False(result.IsSuccess);
        Assert.Equal("3.1.0", result.CurrentVersion);
    }

    [Fact]
    public void CompareSemanticVersion_when_invalid_falls_back_to_string_compare()
    {
        var result = UpdateService.CompareSemanticVersion("foo", "bar");
        Assert.True(result > 0);
    }
}
