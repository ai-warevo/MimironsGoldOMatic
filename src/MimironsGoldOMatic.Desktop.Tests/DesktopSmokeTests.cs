using Xunit;

namespace MimironsGoldOMatic.Desktop.Tests;

/// <summary>
/// Minimal CI smoke test so the WPF Desktop assembly restores and compiles on windows-latest agents.
/// </summary>
public sealed class DesktopSmokeTests
{
    [Fact]
    public void Desktop_App_type_is_loadable()
    {
        _ = typeof(App);
    }
}
