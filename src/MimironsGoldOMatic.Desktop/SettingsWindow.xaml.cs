using System.Windows;
using MimironsGoldOMatic.Desktop.ViewModels;

namespace MimironsGoldOMatic.Desktop;

public partial class SettingsWindow : Window
{
    private readonly MainViewModel _main;

    public SettingsWindow(MainViewModel main)
    {
        InitializeComponent();
        _main = main;
        InjectionCombo.ItemsSource = new[] { "PostMessage", "SendInput" };
        Loaded += (_, _) => LoadFromModel();
    }

    private void LoadFromModel()
    {
        var s = _main.Connection.Settings;
        BaseUrlBox.Text = s.BaseUrl;
        ApiKeyBox.Password = _main.Connection.ApiKey ?? "";
        WowDirBox.Text = s.WoWInstallDirectory ?? "";
        LogOverrideBox.Text = s.WoWChatLogPathOverride ?? "";
        PollBox.Text = s.PollIntervalSeconds.ToString();
        InjectionCombo.SelectedItem = s.InjectionStrategy.Equals("SendInput", StringComparison.OrdinalIgnoreCase)
            ? "SendInput"
            : "PostMessage";
    }

    private void Ok_Click(object sender, RoutedEventArgs e)
    {
        var s = _main.Connection.Settings;
        s.BaseUrl = BaseUrlBox.Text.Trim();
        s.WoWInstallDirectory = string.IsNullOrWhiteSpace(WowDirBox.Text) ? null : WowDirBox.Text.Trim();
        s.WoWChatLogPathOverride = string.IsNullOrWhiteSpace(LogOverrideBox.Text) ? null : LogOverrideBox.Text.Trim();
        if (int.TryParse(PollBox.Text.Trim(), out var poll))
            s.PollIntervalSeconds = Math.Clamp(poll, 5, 600);
        s.InjectionStrategy = InjectionCombo.SelectedItem as string ?? "PostMessage";

        _main.Connection.ApiKey = ApiKeyBox.Password;
        _main.SettingsStore.SaveSettings(s);
        if (!string.IsNullOrEmpty(ApiKeyBox.Password))
            _main.SettingsStore.SaveApiKey(ApiKeyBox.Password);

        _main.ApplyPollIntervalFromSettings();
        _main.ReloadTailAfterSettingsChanged();
        DialogResult = true;
        Close();
    }

    private async void Test_Click(object sender, RoutedEventArgs e)
    {
        _main.Connection.Settings.BaseUrl = BaseUrlBox.Text.Trim();
        _main.Connection.ApiKey = ApiKeyBox.Password;
        var err = await _main.TestApiConnectionAsync().ConfigureAwait(true);
        MessageBox.Show(
            err is null ? "Connected (GET /api/payouts/pending OK)." : $"Failed: {err}",
            "Test connection",
            MessageBoxButton.OK,
            err is null ? MessageBoxImage.Information : MessageBoxImage.Warning);
    }
}
