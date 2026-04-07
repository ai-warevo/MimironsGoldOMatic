using FluentValidation;
using MimironsGoldOMatic.Backend.Configuration;
using MimironsGoldOMatic.Backend.Infrastructure.Persistence;
using MimironsGoldOMatic.Backend.Application.Roulette.Handlers;
using MimironsGoldOMatic.Backend.Application.Roulette.Enrollment;
using MimironsGoldOMatic.Backend.Common;
using Marten;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MimironsGoldOMatic.Backend.IntegrationTests.Support;

internal static class BackendTestHost
{
    /// <param name="devSkipSubscriberCheck">When false, <see cref="PostClaimHandler"/> returns 403 (production subscriber gate).</param>
    internal static ServiceProvider CreateServiceProvider(string connectionString, bool devSkipSubscriberCheck = true)
    {
        var services = new ServiceCollection();
        services.AddLogging(b => b.SetMinimumLevel(LogLevel.Warning));
        services.AddMarten(opts =>
        {
            opts.Connection(connectionString);
            MgmMartenDocumentConfiguration.Configure(opts);
        });
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(PostClaimHandler).Assembly));
        services.Configure<MgmOptions>(m => m.DevSkipSubscriberCheck = devSkipSubscriberCheck);
        services.Configure<TwitchOptions>(_ => { });
        services.AddHttpClient("Helix");
        services.AddSingleton<HelixChatService>();
        services.AddSingleton<ChatEnrollmentService>();
        services.AddValidatorsFromAssemblyContaining<CreatePayoutRequestValidator>();
        return services.BuildServiceProvider();
    }
}
