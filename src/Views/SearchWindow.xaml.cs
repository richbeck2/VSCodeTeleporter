using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using VSCodeTeleporter.Core;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MouseButtonEventArgs = System.Windows.Input.MouseButtonEventArgs;

namespace VSCodeTeleporter.Views;

public partial class SearchWindow
{
    private readonly AppSettings _settings;
    private readonly FolderScanService _scanner = new();
    private readonly FuzzyMatcher _matcher = new();
    private List<string> _allFolders = [];

    public SearchWindow(AppSettings settings)
    {
        _settings = settings;
        InitializeComponent();
        PositionOnPrimaryMonitor();
        Loaded += OnLoaded;
        Deactivated += (_, _) => Hide();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        // Animate fade-in
        var anim = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(120));
        BeginAnimation(OpacityProperty, anim);

        _allFolders = _scanner.GetFolders(_settings.SearchRoot).ToList();
        UpdateList(string.Empty);
        SearchBox.Focus();
    }

    // ── Positioning ─────────────────────────────────────────────────────────

    private void PositionOnPrimaryMonitor()
    {
        // SystemParameters.WorkArea is already in WPF device-independent pixels;
        // it correctly maps to the primary monitor's work area.
        var workArea = SystemParameters.WorkArea;
        Left = workArea.Left + (workArea.Width - Width) / 2;
        Top = workArea.Top + workArea.Height * 0.28;
    }

    // ── Search ───────────────────────────────────────────────────────────────

    private void UpdateList(string query)
    {
        var results = _matcher.Filter(_allFolders, query);
        ResultsList.ItemsSource = results;
        if (results.Count > 0)
        {
            ResultsList.SelectedIndex = 0;
            ResultsList.IsEnabled = true;
        }
        else if (IsGitUrl(query))
        {
            ResultsList.ItemsSource = new[] { "Press Enter to clone and open this repository in VSCode." };
            ResultsList.SelectedIndex = 0;
        }
        SearchPrompt.Visibility = string.IsNullOrEmpty(query)
            ? Visibility.Visible : Visibility.Collapsed;
    }

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        => UpdateList(SearchBox.Text);

    // ── Keyboard navigation ──────────────────────────────────────────────────

    private void SearchBox_KeyDown(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Escape:
                Hide();
                e.Handled = true;
                break;

            case Key.Enter:
                OpenSelected();
                e.Handled = true;
                break;

            case Key.Down:
                MoveSelection(+1);
                e.Handled = true;
                break;

            case Key.Up:
                MoveSelection(-1);
                e.Handled = true;
                break;
        }
    }

    private static bool IsGitUrl(string s)
    {
        if (string.IsNullOrEmpty(s)) return false;
        if (!s.EndsWith(".git", StringComparison.OrdinalIgnoreCase)) return false;
        if (!Uri.TryCreate(s, UriKind.Absolute, out var u)) return false;
        return u.Scheme == Uri.UriSchemeHttp || u.Scheme == Uri.UriSchemeHttps;
    }

    private void CloneGitRepository(string url)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "git",
            ArgumentList = { "clone", url },
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            WorkingDirectory = _settings.SearchRoot
        };

        using var p = Process.Start(psi);
        p?.WaitForExit();
        if (p == null || p.ExitCode != 0)
        {
            var err = p != null ? p.StandardError.ReadToEnd() : "Process failed to start.";
            System.Windows.MessageBox.Show(
                $"Git clone failed:\n{err}",
                "Clone Failed",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            return;
        }
    }

    private void HandleGitUrl(string url)
    {
        try
        {
            // Determine repo folder name
            string repoName;
            try
            {
                var uri = new Uri(url);
                repoName = Path.GetFileName(uri.AbsolutePath);
            }
            catch
            {
                repoName = url.Split('/').Last();
            }
            if (repoName.EndsWith(".git", StringComparison.OrdinalIgnoreCase))
                repoName = repoName[..^4];

            var destPath = Path.Combine(_settings.SearchRoot, repoName);

            if (Directory.Exists(destPath) && Directory.EnumerateFileSystemEntries(destPath).Any())
            {
                LaunchVSCode(destPath);
                return;
            }

            CloneGitRepository(url);

            LaunchVSCode(destPath);
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show(
                $"Error while cloning or opening repository:\n{ex.Message}",
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void MoveSelection(int delta)
    {
        int count = ResultsList.Items.Count;
        if (count == 0) return;
        int next = SelectionHelper.NextIndex(ResultsList.SelectedIndex, delta, count);
        ResultsList.SelectedIndex = next;
        ResultsList.ScrollIntoView(ResultsList.SelectedItem);
    }

    private void ResultsList_SelectionChanged(object sender, SelectionChangedEventArgs e) { }

    private void ResultsList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        => OpenSelected();

    // ── Open ─────────────────────────────────────────────────────────────────

    private void LaunchVSCode(string path)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "code",
                Arguments = $"\"{path}\"",
                UseShellExecute = true,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            });
            Hide();
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show(
                $"Could not launch VSCode.\n\nMake sure 'code' is on your PATH.\n\n{ex.Message}",
                "VSCode Launcher",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void OpenSelected()
    {
        if (ResultsList.SelectedItem is not string folderName) return;

        folderName = folderName.Trim();
        var searchText = SearchBox.Text?.Trim() ?? string.Empty;

        var fullPath = Path.Combine(_settings.SearchRoot, folderName);
        // If fullPath exists, open it, otherwise treat it as a prompt (e.g. git URL) and try to launch that directly
        if (Directory.Exists(fullPath))
            LaunchVSCode(fullPath);
        else if (IsGitUrl(searchText))
            HandleGitUrl(searchText);
    }
}
