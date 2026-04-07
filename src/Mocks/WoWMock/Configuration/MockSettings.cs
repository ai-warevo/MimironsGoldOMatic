namespace MimironsGoldOMatic.Mocks.WoWMock.Configuration;

public sealed class MockSettings
{
    public const string SectionName = "MockSettings";

    public string LogFilePath { get; init; } = "Logs/WoWChatLog.txt";
    public int ApiPort { get; init; } = 5001;
    public int CommandProcessingDelayMs { get; init; } = 100;
    public bool AutoConfirmAccepts { get; init; } = false;

    public bool EchoRunCommandsToChatLog { get; init; } = false;

    public bool WriteDiagnosticsToFile { get; init; } = false;
    public string DiagnosticsLogPath { get; init; } = "WoWMock.log";
}

