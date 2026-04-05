using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace MimironsGoldOMatic.Desktop.Services;

/// <summary>Persists operator settings under <c>%LocalAppData%\MimironsGoldOMatic</c>; API key via DPAPI.</summary>
public sealed class DesktopSettingsStore
{
    private static readonly JsonSerializerOptions JsonOpts = new() { WriteIndented = true };

    public static string DataDirectory =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MimironsGoldOMatic");

    private readonly string _dataDirectory;

    /// <param name="dataDirectory">Optional override for tests; defaults to <see cref="DataDirectory"/>.</param>
    public DesktopSettingsStore(string? dataDirectory = null)
    {
        _dataDirectory = dataDirectory ?? DataDirectory;
    }

    private string SettingsPath => Path.Combine(_dataDirectory, "settings.json");
    private string ApiKeyPath => Path.Combine(_dataDirectory, "api-key.dpapi");

    public DesktopUserSettings LoadSettings()
    {
        try
        {
            if (!File.Exists(SettingsPath))
                return new DesktopUserSettings();
            var json = File.ReadAllText(SettingsPath);
            return JsonSerializer.Deserialize<DesktopUserSettings>(json) ?? new DesktopUserSettings();
        }
        catch
        {
            return new DesktopUserSettings();
        }
    }

    public void SaveSettings(DesktopUserSettings settings)
    {
        Directory.CreateDirectory(_dataDirectory);
        File.WriteAllText(SettingsPath, JsonSerializer.Serialize(settings, JsonOpts));
    }

    public string? LoadApiKey()
    {
        try
        {
            if (!File.Exists(ApiKeyPath))
                return null;
            var encrypted = File.ReadAllBytes(ApiKeyPath);
            var plain = ProtectedData.Unprotect(encrypted, null, DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(plain);
        }
        catch
        {
            return null;
        }
    }

    public void SaveApiKey(string apiKey)
    {
        Directory.CreateDirectory(_dataDirectory);
        var plain = Encoding.UTF8.GetBytes(apiKey);
        var encrypted = ProtectedData.Protect(plain, null, DataProtectionScope.CurrentUser);
        File.WriteAllBytes(ApiKeyPath, encrypted);
    }
}
