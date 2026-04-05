using System.Runtime.InteropServices;

namespace MimironsGoldOMatic.Desktop.Win32;

/// <summary>Fallback: <c>SendInput</c> with <c>KEYEVENTF_UNICODE</c> after foregrounding WoW (<c>docs/overview/SPEC.md</c> §8).</summary>
public sealed class SendInputWoWInputStrategy : IWoWInputStrategy
{
    private const uint InputKeyboard = 1;
    private const uint KeyeventfKeyup = 0x0002;
    private const uint KeyeventfUnicode = 0x0004;

    public int InterCharacterDelayMs { get; init; } = 18;

    public void InjectChatLine(IntPtr wowMainWindow, string chatLine, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(chatLine);
        WoWForegroundLocator.EnsureForeground(wowMainWindow);
        Thread.Sleep(80);

        foreach (var ch in chatLine)
        {
            cancellationToken.ThrowIfCancellationRequested();
            SendUnicodeKey(ch, keyUp: false);
            SendUnicodeKey(ch, keyUp: true);
            Thread.Sleep(InterCharacterDelayMs);
        }

        cancellationToken.ThrowIfCancellationRequested();
        SendVirtualKey(NativeMethods.VK_RETURN, keyUp: false);
        SendVirtualKey(NativeMethods.VK_RETURN, keyUp: true);
        Thread.Sleep(120);
    }

    private static void SendUnicodeKey(char ch, bool keyUp)
    {
        var flags = KeyeventfUnicode | (keyUp ? KeyeventfKeyup : 0);
        var input = new INPUT
        {
            type = InputKeyboard,
            U = new INPUTUNION
            {
                ki = new KEYBDINPUT
                {
                    wVk = 0,
                    wScan = ch,
                    dwFlags = flags,
                    time = 0,
                    dwExtraInfo = UIntPtr.Zero,
                },
            },
        };
        var size = Marshal.SizeOf<INPUT>();
        if (NativeMethods.SendInput(1, [input], size) != 1)
            throw new InvalidOperationException("SendInput (unicode) failed.");
    }

    private static void SendVirtualKey(ushort vk, bool keyUp)
    {
        var flags = keyUp ? KeyeventfKeyup : 0;
        var input = new INPUT
        {
            type = InputKeyboard,
            U = new INPUTUNION
            {
                ki = new KEYBDINPUT
                {
                    wVk = vk,
                    wScan = 0,
                    dwFlags = flags,
                    time = 0,
                    dwExtraInfo = UIntPtr.Zero,
                },
            },
        };
        var size = Marshal.SizeOf<INPUT>();
        if (NativeMethods.SendInput(1, [input], size) != 1)
            throw new InvalidOperationException("SendInput (virtual key) failed.");
    }
}
