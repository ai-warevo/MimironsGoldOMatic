using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using MimironsGoldOMatic.Mocks.WoWMock.Configuration;
using MimironsGoldOMatic.Mocks.WoWMock.Models;
using Microsoft.Extensions.Options;

namespace MimironsGoldOMatic.Mocks.WoWMock;

public sealed class CommandProcessor
{
    private static readonly Regex MgmAcceptRegex =
        new(@"\[MGM_ACCEPT:(?<id>[0-9a-fA-F-]{8,64})\]", RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private readonly MockSettings _settings;
    private readonly ChatLogSimulator _chatLog;
    private readonly ConcurrentQueue<MockCommand> _commands = new();

    private volatile bool _echoRunCommands;
    private volatile bool _autoConfirmAccepts;
    private volatile int _delayMs;

    public CommandProcessor(IOptions<MockSettings> settings, ChatLogSimulator chatLog)
    {
        _settings = settings.Value;
        _chatLog = chatLog;

        _delayMs = Math.Max(0, _settings.CommandProcessingDelayMs);
        _autoConfirmAccepts = _settings.AutoConfirmAccepts;
        _echoRunCommands = _settings.EchoRunCommandsToChatLog;
    }

    public IReadOnlyList<MockCommand> GetCommandsSnapshot()
        => _commands.ToArray();

    public void Reset()
    {
        while (_commands.TryDequeue(out _))
        {
        }
    }

    public void ConfigureResponse(bool autoConfirmAccepts, bool echoRunCommandsToChatLog, int? commandProcessingDelayMs)
    {
        _autoConfirmAccepts = autoConfirmAccepts;
        _echoRunCommands = echoRunCommandsToChatLog;
        if (commandProcessingDelayMs is not null)
        {
            _delayMs = Math.Max(0, commandProcessingDelayMs.Value);
        }
    }

    public async Task ReceiveInjectedMessageAsync(string content, CancellationToken ct)
    {
        if (content.StartsWith("/run ", StringComparison.OrdinalIgnoreCase))
        {
            await ReceiveRunCommandAsync(content, ct);
            return;
        }

        var accept = MgmAcceptRegex.Match(content);
        if (accept.Success && _autoConfirmAccepts)
        {
            var id = accept.Groups["id"].Value;
            await Task.Delay(_delayMs, ct);
            await _chatLog.AppendAsync($"[MGM_CONFIRM:{id}]", DateTimeOffset.Now, ct);
        }
    }

    private async Task ReceiveRunCommandAsync(string command, CancellationToken ct)
    {
        _commands.Enqueue(new MockCommand(command, DateTime.UtcNow));

        if (_delayMs > 0)
        {
            await Task.Delay(_delayMs, ct);
        }

        if (_echoRunCommands)
        {
            await _chatLog.AppendAsync($"[MGM_RUN_RECEIVED] {command}", DateTimeOffset.Now, ct);
        }
    }
}

