using MimironsGoldOMatic.Mocks.WoWMock.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MimironsGoldOMatic.Mocks.WoWMock.Api;

public sealed class Startup
{
    private readonly IConfiguration _configuration;

    public Startup(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();

        services.AddOptions<MockSettings>()
            .Bind(_configuration.GetSection(MockSettings.SectionName))
            .Validate(s => s.ApiPort is > 0 and < 65536, "ApiPort must be a valid TCP port.")
            .ValidateOnStart();

        services.AddSingleton<ChatLogSimulator>();
        services.AddSingleton<CommandProcessor>();
        services.AddSingleton(sp => new MockState(
            sp.GetRequiredService<ChatLogSimulator>(),
            sp.GetRequiredService<CommandProcessor>()));
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseRouting();

        app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
    }
}

public sealed class MockState
{
    public MockState(ChatLogSimulator chatLog, CommandProcessor commandProcessor)
    {
        ChatLog = chatLog;
        CommandProcessor = commandProcessor;
    }

    public ChatLogSimulator ChatLog { get; }
    public CommandProcessor CommandProcessor { get; }

    private readonly SemaphoreSlim _resetLock = new(1, 1);

    public async Task ResetAsync(CancellationToken ct)
    {
        await _resetLock.WaitAsync(ct);
        try
        {
            CommandProcessor.Reset();
            await ChatLog.ResetAsync(ct);
        }
        finally
        {
            _resetLock.Release();
        }
    }
}

