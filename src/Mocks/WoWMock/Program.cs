using MimironsGoldOMatic.Mocks.WoWMock.Api;
using MimironsGoldOMatic.Mocks.WoWMock.Configuration;
using MimironsGoldOMatic.Mocks.WoWMock;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MimironsGoldOMatic.Mocks.WoWMock;

public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        var host = CreateHostBuilder(args).Build();

        var logger = host.Services.GetRequiredService<ILoggerFactory>().CreateLogger("WoWMock");

        var settings = host.Services.GetRequiredService<IOptions<MockSettings>>().Value;
        ConfigureOptionalFileDiagnostics(host.Services, settings, logger);

        var chatLog = host.Services.GetRequiredService<ChatLogSimulator>();
        await chatLog.EnsureCreatedAsync(CancellationToken.None);
        logger.LogInformation("WoWMock chat log: {LogFilePath}", chatLog.LogFilePath);

        await host.RunAsync();
        return 0;
    }

    private static IHostBuilder CreateHostBuilder(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((ctx, cfg) =>
            {
                cfg.SetBasePath(AppContext.BaseDirectory);
                cfg.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                cfg.AddEnvironmentVariables(prefix: "WOWMOCK_");
                if (args is { Length: > 0 })
                {
                    cfg.AddCommandLine(args);
                }
            })
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddSimpleConsole(o =>
                {
                    o.SingleLine = true;
                    o.TimestampFormat = "HH:mm:ss ";
                });
            })
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
                webBuilder.UseKestrel((ctx, kestrel) =>
                {
                    var settings = ctx.Configuration.GetSection(MockSettings.SectionName).Get<MockSettings>()
                                   ?? new MockSettings();
                    kestrel.ListenAnyIP(settings.ApiPort);
                });
            });
    }

    private static void ConfigureOptionalFileDiagnostics(IServiceProvider services, MockSettings settings, ILogger logger)
    {
        if (!settings.WriteDiagnosticsToFile)
        {
            return;
        }

        try
        {
            var resolved = Path.GetFullPath(settings.DiagnosticsLogPath);
            var dir = Path.GetDirectoryName(resolved);
            if (!string.IsNullOrWhiteSpace(dir))
            {
                Directory.CreateDirectory(dir);
            }

            var lifetime = services.GetRequiredService<IHostApplicationLifetime>();
            lifetime.ApplicationStarted.Register(() =>
            {
                var factory = services.GetRequiredService<ILoggerFactory>();
                factory.AddProvider(new SimpleFileLoggerProvider(resolved));
                logger.LogInformation("Diagnostics file logging enabled at: {Path}", resolved);
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to enable diagnostics file logging.");
        }
    }

    private sealed class SimpleFileLoggerProvider : ILoggerProvider
    {
        private readonly string _path;
        private readonly SemaphoreSlim _lock = new(1, 1);

        public SimpleFileLoggerProvider(string path) => _path = path;

        public ILogger CreateLogger(string categoryName) => new SimpleFileLogger(_path, _lock, categoryName);

        public void Dispose() => _lock.Dispose();

        private sealed class SimpleFileLogger : ILogger
        {
            private readonly string _path;
            private readonly SemaphoreSlim _lock;
            private readonly string _category;

            public SimpleFileLogger(string path, SemaphoreSlim @lock, string category)
            {
                _path = path;
                _lock = @lock;
                _category = category;
            }

            public IDisposable BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;
            public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Information;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
                Func<TState, Exception?, string> formatter)
            {
                if (!IsEnabled(logLevel))
                {
                    return;
                }

                var msg = formatter(state, exception);
                var line = $"{DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss.fff} [{logLevel}] {_category}: {msg}";
                if (exception is not null)
                {
                    line += Environment.NewLine + exception;
                }

                _ = WriteLineAsync(line + Environment.NewLine);
            }

            private async Task WriteLineAsync(string line)
            {
                await _lock.WaitAsync();
                try
                {
                    await File.AppendAllTextAsync(_path, line);
                }
                finally
                {
                    _lock.Release();
                }
            }
        }

        private sealed class NullScope : IDisposable
        {
            public static readonly NullScope Instance = new();

            private NullScope()
            {
            }

            public void Dispose()
            {
            }
        }
    }
}

