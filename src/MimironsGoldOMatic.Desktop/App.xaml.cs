using System.Windows;
using MimironsGoldOMatic.Desktop.Services;
using MimironsGoldOMatic.Desktop.Services.Updates;
using MimironsGoldOMatic.Desktop.ViewModels;
using MimironsGoldOMatic.Desktop.Win32;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using Polly;
using Polly.Extensions.Http;

namespace MimironsGoldOMatic.Desktop;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var store = new DesktopSettingsStore();
        var connection = new DesktopConnectionContext
        {
            Settings = store.LoadSettings(),
            ApiKey = store.LoadApiKey(),
        };

        var services = new ServiceCollection();
        services.AddSingleton(connection);
        services.AddSingleton(store);
        services.AddSingleton<PayoutSnapshotCache>();
        services.AddSingleton<NotifiedWhisperStore>();
        services.AddHttpClient(nameof(EbsDesktopClient))
            .AddPolicyHandler(HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                .WaitAndRetryAsync(3, attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt))));
        services.AddSingleton(sp => new EbsDesktopClient(
            sp.GetRequiredService<IHttpClientFactory>(),
            () => sp.GetRequiredService<DesktopConnectionContext>().GetConnection()));
        services.AddSingleton<IEbsDesktopClient>(sp => sp.GetRequiredService<EbsDesktopClient>());
        services.AddSingleton<IAppVersionProvider, AssemblyAppVersionProvider>();
        services.AddSingleton<IUpdateService, UpdateService>();
        services.AddSingleton<WoWInjectionCoordinator>();
        services.AddSingleton<MainViewModel>();

        var provider = services.BuildServiceProvider();
        var main = provider.GetRequiredService<MainViewModel>();
        var window = new MainWindow { DataContext = main };
        window.Show();
    }
}
