using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using MimironsGoldOMatic.Desktop.Api;
using MimironsGoldOMatic.Shared;

namespace MimironsGoldOMatic.Desktop.Services;

public sealed class EbsDesktopClient : IEbsDesktopClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly Func<(string BaseUrl, string ApiKey)> _getConnection;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
    };

    public EbsDesktopClient(IHttpClientFactory httpClientFactory, Func<(string BaseUrl, string ApiKey)> getConnection)
    {
        _httpClientFactory = httpClientFactory;
        _getConnection = getConnection;
    }

    private HttpClient CreateClient()
    {
        var (baseUrl, apiKey) = _getConnection();
        if (string.IsNullOrWhiteSpace(baseUrl))
            throw new InvalidOperationException("API base URL is not configured.");
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidOperationException("X-MGM-ApiKey is not configured.");

        var c = _httpClientFactory.CreateClient(nameof(EbsDesktopClient));
        c.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");
        c.DefaultRequestHeaders.Remove("X-MGM-ApiKey");
        c.DefaultRequestHeaders.TryAddWithoutValidation("X-MGM-ApiKey", apiKey);
        c.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        return c;
    }

    public async Task<IReadOnlyList<PayoutDto>> GetPendingAsync(CancellationToken ct)
    {
        using var c = CreateClient();
        using var resp = await c.GetAsync("api/payouts/pending", ct).ConfigureAwait(false);
        var body = await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
        if (!resp.IsSuccessStatusCode)
            throw new HttpRequestException($"GET pending failed {(int)resp.StatusCode}: {body}");

        var list = JsonSerializer.Deserialize<List<PayoutDto>>(body, JsonOptions)
                   ?? [];
        return list;
    }

    public async Task PatchPayoutStatusAsync(Guid id, PayoutStatus status, CancellationToken ct)
    {
        using var c = CreateClient();
        var json = JsonSerializer.Serialize(new PatchPayoutStatusBody(status), JsonOptions);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");
        using var resp = await c.PatchAsync($"api/payouts/{id:D}/status", content, ct).ConfigureAwait(false);
        var body = await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
        if (!resp.IsSuccessStatusCode)
            throw new HttpRequestException($"PATCH status failed {(int)resp.StatusCode}: {body}");
    }

    public async Task ConfirmAcceptanceAsync(Guid id, string characterName, CancellationToken ct)
    {
        using var c = CreateClient();
        var json = JsonSerializer.Serialize(new ConfirmAcceptanceBody(characterName), JsonOptions);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");
        using var resp = await c.PostAsync($"api/payouts/{id:D}/confirm-acceptance", content, ct).ConfigureAwait(false);
        var body = await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
        if (resp.StatusCode == HttpStatusCode.OK)
            return;
        throw new HttpRequestException($"confirm-acceptance failed {(int)resp.StatusCode}: {body}");
    }

    public async Task VerifyCandidateAsync(VerifyCandidateRequestDto dto, CancellationToken ct)
    {
        using var c = CreateClient();
        var json = JsonSerializer.Serialize(dto, JsonOptions);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");
        using var resp = await c.PostAsync("api/roulette/verify-candidate", content, ct).ConfigureAwait(false);
        var body = await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
        if (resp.IsSuccessStatusCode)
            return;
        throw new HttpRequestException($"verify-candidate failed {(int)resp.StatusCode}: {body}");
    }

    public async Task<VersionInfoDto> GetVersionInfoAsync(CancellationToken ct)
    {
        using var c = CreateClient();
        using var resp = await c.GetAsync("api/version", ct).ConfigureAwait(false);
        var body = await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
        if (!resp.IsSuccessStatusCode)
            throw new HttpRequestException($"GET version failed {(int)resp.StatusCode}: {body}");

        var dto = JsonSerializer.Deserialize<VersionInfoDto>(body, JsonOptions);
        if (dto is null || string.IsNullOrWhiteSpace(dto.Version))
            throw new JsonException("Version payload is missing required 'version' field.");

        return dto;
    }
}
