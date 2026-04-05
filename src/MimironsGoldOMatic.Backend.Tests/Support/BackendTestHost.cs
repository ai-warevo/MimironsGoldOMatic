using FluentValidation;
using MimironsGoldOMatic.Backend.Application;
using MimironsGoldOMatic.Backend.Configuration;
using MimironsGoldOMatic.Backend.Persistence;
using MimironsGoldOMatic.Backend.Services;
using MimironsGoldOMatic.Shared;
using Marten;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MimironsGoldOMatic.Backend.Tests.Support;

internal static class BackendTestHost
{
    internal static ServiceProvider CreateServiceProvider(string connectionString)
    {
        var services = new ServiceCollection();
        services.AddLogging(b => b.SetMinimumLevel(LogLevel.Warning));
        services.AddMarten(opts =>
        {
            opts.Connection(connectionString);
            MgmMartenDocumentConfiguration.Configure(opts);
        });
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(PostClaimHandler).Assembly));
        services.Configure<MgmOptions>(m => m.DevSkipSubscriberCheck = true);
        services.Configure<TwitchOptions>(_ => { });
        services.AddHttpClient("Helix");
        services.AddSingleton<HelixChatService>();
        services.AddValidatorsFromAssemblyContaining<CreatePayoutRequestValidator>();
        return services.BuildServiceProvider();
    }
}
