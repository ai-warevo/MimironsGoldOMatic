namespace MimironsGoldOMatic.Desktop.Win32;

/// <summary>
/// Primary strategy: <c>PostMessage(WM_CHAR)</c> into the WoW main HWND after <see cref="WoWForegroundLocator.EnsureForeground"/>.
/// Works for many 3.3.5a clients when the main window consumes chat input; may fail if anti-cheat blocks posted messages — use <see cref="SendInputWoWInputStrategy"/> in settings.
/// </summary>
public sealed class PostMessageWoWInputStrategy : IWoWInputStrategy
{
    /// <summary>Delay between keystrokes; slightly conservative for slower clients.</summary>
    public int InterCharacterDelayMs { get; init; } = 22;

    public void InjectChatLine(IntPtr wowMainWindow, string chatLine, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(chatLine);
        WoWForegroundLocator.EnsureForeground(wowMainWindow);
        Thread.Sleep(60);

        foreach (var ch in chatLine)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!NativeMethods.PostMessageW(wowMainWindow, NativeMethods.WM_CHAR, (UIntPtr)ch, IntPtr.Zero))
                throw new InvalidOperationException("PostMessage WM_CHAR failed (WoW may be blocking synthetic input).");
            Thread.Sleep(InterCharacterDelayMs);
        }

        cancellationToken.ThrowIfCancellationRequested();
        PressEnter(wowMainWindow);
        Thread.Sleep(120);
    }

    private static void PressEnter(IntPtr hWnd)
    {
        _ = NativeMethods.PostMessageW(hWnd, NativeMethods.WM_KEYDOWN, (UIntPtr)NativeMethods.VK_RETURN, IntPtr.Zero);
        Thread.Sleep(20);
        _ = NativeMethods.PostMessageW(hWnd, NativeMethods.WM_KEYUP, (UIntPtr)NativeMethods.VK_RETURN, IntPtr.Zero);
    }
}
