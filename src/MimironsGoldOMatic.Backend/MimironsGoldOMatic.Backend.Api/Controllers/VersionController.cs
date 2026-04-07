using System.Reflection;
using MimironsGoldOMatic.Backend.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace MimironsGoldOMatic.Backend.Api.Controllers;

[ApiController]
[Route("api/version")]
[AllowAnonymous]
public sealed class VersionController(IOptions<VersionOptions> options) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(VersionInfoDto), StatusCodes.Status200OK)]
    public ActionResult<VersionInfoDto> Get()
    {
        var cfg = options.Value;
        var version = ResolveVersion(cfg.CurrentVersion);

        var response = new VersionInfoDto(
            Version: version,
            ReleaseNotesUrl: cfg.ReleaseNotesUrl,
            MinimumDesktopVersion: cfg.MinimumDesktopVersion,
            MinimumAddonVersion: cfg.MinimumAddonVersion,
            MinimumExtensionVersion: cfg.MinimumExtensionVersion);

        return Ok(response);
    }

    private static string ResolveVersion(string? configuredVersion)
    {
        if (!string.IsNullOrWhiteSpace(configuredVersion))
            return configuredVersion.Trim();

        var assembly = Assembly.GetExecutingAssembly();
        var informational = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        if (!string.IsNullOrWhiteSpace(informational))
            return informational.Trim();

        return assembly.GetName().Version?.ToString() ?? "0.0.0";
    }
}

