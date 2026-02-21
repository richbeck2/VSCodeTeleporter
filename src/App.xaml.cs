using System.Windows;
using VSCodeTeleporter.Core;
using VSCodeTeleporter.Services;
using VSCodeTeleporter.Views;

namespace VSCodeTeleporter;

public partial class App
{
    private GlobalHotkeyService? _hotkey;
    private NotifyIcon? _trayIcon;
    private AppSettings _settings = new();

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        _settings = SettingsLoader.Load();

        // System tray
        _trayIcon = BuildTrayIcon();

        // Global hotkey
        _hotkey = new GlobalHotkeyService();
        _hotkey.HotkeyTriggered += OnHotkeyTriggered;
        _hotkey.Register(_settings.HotkeyModifiers, _settings.HotkeyKey);
    }

    private void OnHotkeyTriggered(object? sender, EventArgs e)
    {
        ShowSearchWindow();
    }

    private void ShowSearchWindow()
    {
        // Only open one instance at a time
        foreach (Window w in Windows)
        {
            if (w is SearchWindow existing)
            {
                // If already open, just bring it to front and clear the search box
                existing.SearchBox.Text = string.Empty;
                existing.Show();
                existing.Activate();
                return;
            }
        }

        var window = new SearchWindow(_settings);
        window.Show();
        window.Activate();
    }

    // ── Tray icon ────────────────────────────────────────────────────────────

    private NotifyIcon BuildTrayIcon()
    {
        var icon = new NotifyIcon
        {
            Icon = CreateDefaultIcon(),
            Visible = true,
            Text = $"VSCode Launcher ({_settings.HotkeyModifiers}+{_settings.HotkeyKey})"
        };

        var menu = new ContextMenuStrip();

        var openItem = new ToolStripMenuItem("Open Search");
        openItem.Click += (_, _) => ShowSearchWindow();
        menu.Items.Add(openItem);

        menu.Items.Add(new ToolStripSeparator());

        var startupItem = new ToolStripMenuItem("Start with Windows")
        {
            CheckOnClick = true,
            Checked = _settings.StartWithWindows
        };
        startupItem.CheckedChanged += OnStartupToggled;
        menu.Items.Add(startupItem);

        menu.Items.Add(new ToolStripSeparator());

        var exitItem = new ToolStripMenuItem("Exit");
        exitItem.Click += (_, _) => Shutdown();
        menu.Items.Add(exitItem);

        icon.ContextMenuStrip = menu;
        icon.DoubleClick += (_, _) => ShowSearchWindow();

        return icon;
    }

    /// <summary>Programmatically draw a small VS-Code-ish icon (16x16).</summary>
    private static Icon CreateDefaultIcon()
    {
        using var bmp = new Bitmap(16, 16);
        using var g = Graphics.FromImage(bmp);
        g.Clear(Color.Transparent);
        using var brush = new SolidBrush(Color.FromArgb(0x00, 0x7A, 0xCC));
        g.FillRectangle(brush, 1, 1, 14, 14);
        using var pen = new Pen(Color.White, 2);
        // Draw a simple ">" symbol
        g.DrawLines(pen, new System.Drawing.Point[] {
            new(4, 4), new(8, 8), new(4, 12)
        });
        // Draw an underline
        g.DrawLine(pen, 8, 12, 13, 12);
        return Icon.FromHandle(bmp.GetHicon());
    }

    private void OnStartupToggled(object? sender, EventArgs e)
    {
        if (sender is not ToolStripMenuItem item) return;
        StartupManager.SetStartWithWindows(item.Checked);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _hotkey?.Dispose();
        _trayIcon?.Dispose();
        base.OnExit(e);
    }
}
