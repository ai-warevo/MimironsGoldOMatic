using System.Windows;
using MimironsGoldOMatic.Desktop.ViewModels;

namespace MimironsGoldOMatic.Desktop;

public partial class EventLogWindow : Window
{
    public EventLogWindow(MainViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;
    }

    private void Clear_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel vm)
            vm.DeliveryLog.Clear();
    }
}
