using System.Windows;
using MimironsGoldOMatic.Desktop.ViewModels;

namespace MimironsGoldOMatic.Desktop;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void MenuSettings_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not MainViewModel vm)
            return;
        var w = new SettingsWindow(vm) { Owner = this };
        w.ShowDialog();
    }

    private void MenuLog_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not MainViewModel vm)
            return;
        new EventLogWindow(vm) { Owner = this }.Show();
    }

    private void MenuExit_Click(object sender, RoutedEventArgs e) => Close();

    protected override void OnClosed(EventArgs e)
    {
        if (DataContext is MainViewModel vm)
            vm.Dispose();
        base.OnClosed(e);
    }
}
