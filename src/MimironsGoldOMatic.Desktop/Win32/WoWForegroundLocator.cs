using System.Diagnostics;

namespace MimironsGoldOMatic.Desktop.Win32;

/// <summary>MVP: use the <b>foreground</b> window only when its owning process is <c>WoW</c> (<c>docs/overview/SPEC.md</c> §8).</summary>
public static class WoWForegroundLocator
{
    public static IntPtr TryGetForegroundWoWWindow()
    {
        var hWnd = NativeMethods.GetForegroundWindow();
        if (hWnd == IntPtr.Zero)
            return IntPtr.Zero;

        _ = NativeMethods.GetWindowThreadProcessId(hWnd, out var pid);
        try
        {
            using var proc = Process.GetProcessById((int)pid);
            return proc.ProcessName.Equals("WoW", StringComparison.OrdinalIgnoreCase) ? hWnd : IntPtr.Zero;
        }
        catch (ArgumentException)
        {
            return IntPtr.Zero;
        }
    }

    /// <summary>Brings the window to the foreground if minimized; required before injection on some 3.3.5a setups.</summary>
    public static void EnsureForeground(IntPtr hWnd)
    {
        _ = NativeMethods.ShowWindow(hWnd, NativeMethods.SW_RESTORE);
        _ = NativeMethods.SetForegroundWindow(hWnd);
    }
}
