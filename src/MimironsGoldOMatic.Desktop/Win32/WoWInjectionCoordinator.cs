using MimironsGoldOMatic.Desktop.Services;

namespace MimironsGoldOMatic.Desktop.Win32;

/// <summary>Selects PostMessage vs SendInput from operator settings and falls back on failure.</summary>
public sealed class WoWInjectionCoordinator(DesktopConnectionContext connection)
{
    public void InjectChatLine(string chatLine, CancellationToken cancellationToken)
    {
        var hWnd = WoWForegroundLocator.TryGetForegroundWoWWindow();
        if (hWnd == IntPtr.Zero)
            throw new InvalidOperationException("Foreground WoW window not found. Focus WoW and retry.");

        var preferSendInput = connection.Settings.InjectionStrategy.Equals("SendInput", StringComparison.OrdinalIgnoreCase);
        var primary = preferSendInput ? (IWoWInputStrategy)new SendInputWoWInputStrategy() : new PostMessageWoWInputStrategy();
        var fallback = preferSendInput ? (IWoWInputStrategy)new PostMessageWoWInputStrategy() : new SendInputWoWInputStrategy();

        try
        {
            primary.InjectChatLine(hWnd, chatLine, cancellationToken);
        }
        catch (Exception) when (!cancellationToken.IsCancellationRequested)
        {
            fallback.InjectChatLine(hWnd, chatLine, cancellationToken);
        }
    }
}
