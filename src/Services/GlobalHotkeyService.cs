using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace VSCodeTeleporter.Services;

/// <summary>
/// Registers a global hotkey via Win32 RegisterHotKey and fires an event
/// when it is pressed.  The service creates a hidden HwndSource to receive
/// WM_HOTKEY messages without needing a visible window.
/// </summary>
public sealed class GlobalHotkeyService : IDisposable
{
    private const int WmHotkey = 0x0312;
    private const int HotkeyId = 9000;

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    // Modifier flags
    private const uint ModAlt = 0x0001;
    private const uint ModControl = 0x0002;
    private const uint ModShift = 0x0004;
    private const uint ModWin = 0x0008;
    private const uint ModNorepeat = 0x4000;

    private HwndSource? _source;
    private bool _disposed;

    public event EventHandler? HotkeyTriggered;

    /// <summary>
    /// Registers the hotkey described by the app settings.
    /// Must be called after the WPF Dispatcher has been initialised
    /// (i.e., from within Application.OnStartup or later).
    /// </summary>
    public void Register(string modifiers, string key)
    {
        var parms = new HwndSourceParameters("GlobalHotkeyHost")
        {
            Width = 0, Height = 0,
            WindowStyle = 0 // WS_OVERLAPPED, invisible
        };
        _source = new HwndSource(parms);
        _source.AddHook(WndProc);

        uint mods = ParseModifiers(modifiers) | ModNorepeat;
        uint vk = ParseKey(key);

        if (!RegisterHotKey(_source.Handle, HotkeyId, mods, vk))
            throw new InvalidOperationException(
                $"Could not register hotkey {modifiers}+{key}. " +
                "It may already be in use by another application.");
    }

    public void Unregister()
    {
        if (_source is not null)
            UnregisterHotKey(_source.Handle, HotkeyId);
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == WmHotkey && wParam.ToInt32() == HotkeyId)
        {
            HotkeyTriggered?.Invoke(this, EventArgs.Empty);
            handled = true;
        }
        return IntPtr.Zero;
    }

    private static uint ParseModifiers(string mods)
    {
        uint result = 0;
        foreach (var part in mods.Split('+'))
        {
            result |= part.Trim().ToLowerInvariant() switch
            {
                "ctrl" or "control" => ModControl,
                "alt" => ModAlt,
                "shift" => ModShift,
                "win" => ModWin,
                _ => 0
            };
        }
        return result;
    }

    private static uint ParseKey(string key)
    {
        // Single letter/digit → use char code
        if (key.Length == 1)
            return char.ToUpperInvariant(key[0]);

        // Function keys
        if (key.StartsWith("F", StringComparison.OrdinalIgnoreCase) &&
            int.TryParse(key[1..], out int fn) && fn is >= 1 and <= 24)
            return (uint)(0x6F + fn); // VK_F1 = 0x70

        // Named keys
        return key.ToUpperInvariant() switch
        {
            "SPACE" => 0x20,
            "RETURN" or "ENTER" => 0x0D,
            "ESCAPE" or "ESC" => 0x1B,
            _ => throw new ArgumentException($"Unsupported hotkey key: '{key}'")
        };
    }

    public void Dispose()
    {
        if (_disposed) return;
        Unregister();
        _source?.Dispose();
        _disposed = true;
    }
}
