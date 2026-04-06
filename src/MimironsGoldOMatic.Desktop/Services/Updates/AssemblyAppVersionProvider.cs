using System.Reflection;

namespace MimironsGoldOMatic.Desktop.Services.Updates;

public sealed class AssemblyAppVersionProvider : IAppVersionProvider
{
    public string GetCurrentVersion()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var informational = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        if (!string.IsNullOrWhiteSpace(informational))
        {
            return Normalize(informational);
        }

        return Normalize(assembly.GetName().Version?.ToString() ?? "0.0.0");
    }

    private static string Normalize(string value)
    {
        var trimmed = value.Trim();
        var plus = trimmed.IndexOf('+');
        if (plus >= 0)
        {
            trimmed = trimmed[..plus];
        }

        var semicolon = trimmed.IndexOf(';');
        if (semicolon >= 0)
        {
            trimmed = trimmed[..semicolon];
        }

        return trimmed;
    }
}
