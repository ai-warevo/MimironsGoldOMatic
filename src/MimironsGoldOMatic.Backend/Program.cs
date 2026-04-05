// <!-- Updated: 2026-04-05 (Tier B integration & first run) -->
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using FluentValidation;
using MimironsGoldOMatic.Backend.Application;
using MimironsGoldOMatic.Backend.Auth;
using MimironsGoldOMatic.Backend.Configuration;
using MimironsGoldOMatic.Backend.Persistence;
using MimironsGoldOMatic.Backend.Services;
using MimironsGoldOMatic.Shared;
using Marten;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<MgmOptions>(builder.Configuration.GetSection(MgmOptions.SectionName));
builder.Services.Configure<TwitchOptions>(builder.Configuration.GetSection(TwitchOptions.SectionName));

builder.Services.AddControllers().AddJsonOptions(o =>
{
    o.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    o.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
});

builder.Services.AddOpenApi();

var pg = builder.Configuration.GetConnectionString("PostgreSQL");
if (string.IsNullOrWhiteSpace(pg))
    throw new InvalidOperationException("ConnectionStrings:PostgreSQL is required for Marten.");

builder.Services.AddMarten(opts =>
{
    opts.Connection(pg);
    MgmMartenDocumentConfiguration.Configure(opts);
});

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(PostClaimHandler).Assembly));

builder.Services.AddSingleton<ChatEnrollmentService>();
builder.Services.AddSingleton<IChatEnrollmentIngest>(sp => sp.GetRequiredService<ChatEnrollmentService>());
builder.Services.AddSingleton<HelixChatService>();
builder.Services.AddHostedService<RouletteSynchronizerHostedService>();
builder.Services.AddHostedService<PayoutExpirationHostedService>();

var twitch = builder.Configuration.GetSection(TwitchOptions.SectionName).Get<TwitchOptions>() ?? new TwitchOptions();
var helixBase = string.IsNullOrWhiteSpace(twitch.HelixApiBaseUrl)
    ? "https://api.twitch.tv/"
    : twitch.HelixApiBaseUrl.Trim().TrimEnd('/') + "/";
builder.Services.AddHttpClient("Helix", c =>
{
    c.BaseAddress = new Uri(helixBase);
    c.DefaultRequestHeaders.Add("Accept", "application/json");
    c.Timeout = TimeSpan.FromSeconds(30);
});

byte[] extensionKey;
if (!string.IsNullOrEmpty(twitch.ExtensionSecret))
    extensionKey = Convert.FromBase64String(twitch.ExtensionSecret);
else if (builder.Environment.IsDevelopment())
    extensionKey = SHA256.HashData(Encoding.UTF8.GetBytes("mgm-dev-extension-secret-change-me"));
else
    throw new InvalidOperationException("Twitch:ExtensionSecret (base64) is required outside Development.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = !string.IsNullOrEmpty(twitch.ExtensionClientId),
            ValidAudience = string.IsNullOrEmpty(twitch.ExtensionClientId) ? null : twitch.ExtensionClientId,
            ValidateLifetime = true,
            IssuerSigningKey = new SymmetricSecurityKey(extensionKey),
            NameClaimType = "user_id",
        };
    })
    .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>("ApiKey", _ => { });

builder.Services.AddAuthorization();

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
    {
        if (context.Request.Path.StartsWithSegments("/api/twitch/eventsub"))
            return RateLimitPartition.GetNoLimiter("eventsub");

        var key = context.User.FindFirst("user_id")?.Value
                  ?? context.Connection.RemoteIpAddress?.ToString()
                  ?? "anon";
        return RateLimitPartition.GetFixedWindowLimiter(
            key,
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0,
            });
    });
});

builder.Services.AddValidatorsFromAssemblyContaining<CreatePayoutRequestValidator>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

await using (var scope = app.Services.CreateAsyncScope())
{
    var store = scope.ServiceProvider.GetRequiredService<IDocumentStore>();
    await store.Storage.ApplyAllConfiguredChangesToDatabaseAsync();
}

// After authentication so Extension JWT user_id partitions limits (before auth, all traffic keyed by IP).
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();
app.MapControllers();

app.Run();
