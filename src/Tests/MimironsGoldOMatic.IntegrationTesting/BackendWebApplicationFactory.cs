using System.Collections.Generic;
using MimironsGoldOMatic.Backend.Application;
using MimironsGoldOMatic.Backend.Application.Gifts;
using MimironsGoldOMatic.Backend.Application.Roulette;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MimironsGoldOMatic.IntegrationTesting;

/// <summary>Full ASP.NET Core host over a Testcontainers PostgreSQL connection string (matches <c>Program.cs</c> configuration keys).</summary>
public sealed class BackendWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _connectionString;

    public BackendWebApplicationFactory(string connectionString) =>
        _connectionString = connectionString;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        // Highest-priority override so appsettings.Development.json / env cannot point at a local PostgreSQL during tests.
        builder.UseSetting("ConnectionStrings:PostgreSQL", _connectionString);
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Mgm:ApiKey"] = IntegrationTestConstants.DesktopApiKey,
                ["Mgm:DevSkipSubscriberCheck"] = "true",
                ["Twitch:ExtensionClientId"] = "test-extension-client-id",
                ["Twitch:EventSubSecret"] = "",
            });
        });

        builder.ConfigureLogging(logging => logging.SetMinimumLevel(LogLevel.Debug));

        builder.ConfigureTestServices(services =>
        {
            var remove = services.Where(sd =>
                sd.ServiceType == typeof(IHostedService) &&
                sd.ImplementationType is { } t &&
                (t == typeof(RouletteSynchronizerHostedService) ||
                 t == typeof(PayoutExpirationHostedService) ||
                 t == typeof(GiftQueueTimeoutHostedService))).ToList();
            foreach (var sd in remove)
                services.Remove(sd);
        });
    }
}
