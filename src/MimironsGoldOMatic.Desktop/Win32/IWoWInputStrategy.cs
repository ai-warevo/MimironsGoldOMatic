namespace MimironsGoldOMatic.Desktop.Win32;

/// <summary>Injects a slash command into WoW chat (<c>docs/overview/SPEC.md</c> §8).</summary>
public interface IWoWInputStrategy
{
    /// <param name="wowMainWindow">Main window HWND from <see cref="WoWForegroundLocator"/>.</param>
    /// <param name="chatLine">Full line including leading <c>/</c> (e.g. <c>/run ReceiveGold("…")</c>).</param>
    void InjectChatLine(IntPtr wowMainWindow, string chatLine, CancellationToken cancellationToken);
}
