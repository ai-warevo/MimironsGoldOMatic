using MimironsGoldOMatic.Mocks.WoWMock.Models;
using Microsoft.AspNetCore.Mvc;

namespace MimironsGoldOMatic.Mocks.WoWMock.Api;

[ApiController]
[Route("api/mock")]
public sealed class MockController : ControllerBase
{
    private readonly MockState _state;

    public MockController(MockState state)
    {
        _state = state;
    }

    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new { ok = true, logFilePath = _state.ChatLog.LogFilePath });
    }

    [HttpPost("reset")]
    public async Task<IActionResult> Reset(CancellationToken ct)
    {
        await _state.ResetAsync(ct);
        return Ok(new { ok = true, logFilePath = _state.ChatLog.LogFilePath });
    }

    public sealed record AddMessageRequest(string Content);

    [HttpPost("add-message")]
    public async Task<IActionResult> AddMessage([FromBody] AddMessageRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.Content))
        {
            return BadRequest(new { error = "Content is required." });
        }

        var now = DateTimeOffset.Now;
        await _state.ChatLog.AppendAsync(req.Content, now, ct);
        await _state.CommandProcessor.ReceiveInjectedMessageAsync(req.Content, ct);

        return Ok(new MockMessage(req.Content, now.UtcDateTime));
    }

    [HttpGet("commands")]
    public IActionResult GetCommands()
    {
        return Ok(_state.CommandProcessor.GetCommandsSnapshot());
    }

    public sealed record SetResponseRequest(
        bool AutoConfirmAccepts,
        bool EchoRunCommandsToChatLog,
        int? CommandProcessingDelayMs);

    [HttpPost("set-response")]
    public IActionResult SetResponse([FromBody] SetResponseRequest req)
    {
        _state.CommandProcessor.ConfigureResponse(
            autoConfirmAccepts: req.AutoConfirmAccepts,
            echoRunCommandsToChatLog: req.EchoRunCommandsToChatLog,
            commandProcessingDelayMs: req.CommandProcessingDelayMs);

        return Ok(new { ok = true });
    }
}

