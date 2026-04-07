using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using FluentValidation;
using JasperFx.Events;
using Marten;
using Marten.Events;
using MimironsGoldOMatic.Backend.Configuration;
using MimironsGoldOMatic.Backend.Domain;
using MimironsGoldOMatic.Backend.Persistence;
using MimironsGoldOMatic.Backend.Services;
using MimironsGoldOMatic.Backend.Services.Mediatr;
using MimironsGoldOMatic.Backend.Shared;
using MimironsGoldOMatic.Backend.Infrastructure.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace MimironsGoldOMatic.Backend.Infrastructure;

public static class BackendCompositionExtensions
{
    /// <summary>
    /// Compose the full backend dependency graph for the new <c>Backend.Api</c> host.
    /// Kept in Infrastructure to centralize composition-root wiring.
    /// </summary>
    public static IServiceCollection AddMgmBackend(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.Configure<MgmOptions>(configuration.GetSection(MgmOptions.SectionName));
        services.Configure<TwitchOptions>(configuration.GetSection(TwitchOptions.SectionName));
        services.Configure<VersionOptions>(configuration.GetSection(VersionOptions.SectionName));

        // Controllers use camelCase + string enums; keep serialization config aligned with legacy.
        services.AddControllers().AddJsonOptions(o =>
        {
            o.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            o.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
        });

        services.AddOpenApi();

        // Auth: JWT bearer for Extension, plus ApiKey scheme for Desktop.
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(o =>
            {
                var twitch = configuration.GetSection(TwitchOptions.SectionName).Get<TwitchOptions>() ?? new TwitchOptions();

                byte[] extensionKey;
                if (!string.IsNullOrEmpty(twitch.ExtensionSecret))
                    extensionKey = Convert.FromBase64String(twitch.ExtensionSecret);
                else if (environment.IsDevelopment())
                    extensionKey = SHA256.HashData(Encoding.UTF8.GetBytes("mgm-dev-extension-secret-change-me"));
                else
                    throw new InvalidOperationException("Twitch:ExtensionSecret (base64) is required outside Development.");

                o.RequireHttpsMetadata = !environment.IsDevelopment();
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = !string.IsNullOrEmpty(twitch.ExtensionClientId),
                    ValidAudience = string.IsNullOrEmpty(twitch.ExtensionClientId) ? null : twitch.ExtensionClientId,
                    ValidateLifetime = true,
                    IssuerSigningKey = new SymmetricSecurityKey(extensionKey),
                    NameClaimType = "user_id"
                };
            })
            .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>("ApiKey", _ => { });

        services.AddAuthorization();

        // Validators: prefer the new Backend.Shared validators where possible.
        services.AddValidatorsFromAssemblyContaining<CreatePayoutRequestValidator>();

        // HttpClient("Helix")
        var twitchOptions = configuration.GetSection(TwitchOptions.SectionName).Get<TwitchOptions>() ?? new TwitchOptions();
        var helixBase = string.IsNullOrWhiteSpace(twitchOptions.HelixApiBaseUrl)
            ? "https://api.twitch.tv/"
            : twitchOptions.HelixApiBaseUrl.Trim().TrimEnd('/') + "/";

        services.AddHttpClient("Helix", c =>
        {
            c.BaseAddress = new Uri(helixBase);
            c.DefaultRequestHeaders.Add("Accept", "application/json");
            c.Timeout = TimeSpan.FromSeconds(30);
        });

        // Persistence / Marten
        var pg = configuration.GetConnectionString("PostgreSQL");
        if (string.IsNullOrWhiteSpace(pg))
            throw new InvalidOperationException("ConnectionStrings:PostgreSQL is required for Marten.");

        services.AddMarten(opts =>
        {
            opts.Connection(pg);
            MgmMartenDocumentConfiguration.Configure(opts);
            // Some event types are not yet finalized; keep defaults that compile safely.
            opts.Events.StreamIdentity = StreamIdentity.AsGuid;
        });

        // MediatR: contracts in Domain, handlers in Services.
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<PostClaimHandler>());

        // Service registrations required by some middleware/controllers.
        services.AddSingleton<ChatEnrollmentService>();
        services.AddSingleton<IChatEnrollmentIngest>(sp => sp.GetRequiredService<ChatEnrollmentService>());
        services.AddSingleton<HelixChatService>();
        services.AddSingleton<GiftQueueService>();
        services.AddSingleton<ITwitchSubscriberVerifier, HelixSubscriberVerifier>();
        // For API-local parsing logic; kept to ensure no missing DI registrations later.
        services.AddSingleton<HelixSubscriberVerifier>();

        services.AddHostedService<RouletteSynchronizerHostedService>();
        services.AddHostedService<PayoutExpirationHostedService>();
        services.AddHostedService<GiftQueueTimeoutHostedService>();

        return services;
    }
}

