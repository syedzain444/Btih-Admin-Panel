using HospitalAdminPanel.Configuration;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace HospitalAdminPanel.Services;

public class ApiClient : IApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ApiSettings _settings;
    private readonly ITokenSessionService _tokenSession;

    public ApiClient(IHttpClientFactory httpClientFactory, IOptions<ApiSettings> settings, ITokenSessionService tokenSession)
    {
        _httpClientFactory = httpClientFactory;
        _settings = settings.Value;
        _tokenSession = tokenSession;
    }

    private HttpClient CreateClient()
    {
        var client = _httpClientFactory.CreateClient(nameof(ApiClient));
        client.BaseAddress = new Uri(_settings.BaseUrl.TrimEnd('/') + "/");
        client.Timeout = TimeSpan.FromSeconds(_settings.TimeoutSeconds);
        return client;
    }

    public Task<T?> GetAsync<T>(string path, CancellationToken cancellationToken = default) =>
        SendAsync<T>(HttpMethod.Get, path, null, cancellationToken);

    public Task<TResponse?> PostAsync<TRequest, TResponse>(string path, TRequest body, CancellationToken cancellationToken = default) =>
        SendAsync<TResponse>(HttpMethod.Post, path, body, cancellationToken);

    public Task<TResponse?> PutAsync<TRequest, TResponse>(string path, TRequest body, CancellationToken cancellationToken = default) =>
        SendAsync<TResponse>(HttpMethod.Put, path, body, cancellationToken);

    public Task<TResponse?> PatchAsync<TRequest, TResponse>(string path, TRequest body, CancellationToken cancellationToken = default) =>
        SendAsync<TResponse>(HttpMethod.Patch, path, body, cancellationToken);

    public async Task<bool> PostAsync(string path, object body, CancellationToken cancellationToken = default)
    {
        var result = await SendAsync<object>(HttpMethod.Post, path, body, cancellationToken);
        return result != null || true;
    }

    public async Task<bool> PutAsync(string path, object body, CancellationToken cancellationToken = default)
    {
        await SendAsync<object>(HttpMethod.Put, path, body, cancellationToken);
        return true;
    }

    private async Task<T?> SendAsync<T>(HttpMethod method, string path, object? body, CancellationToken cancellationToken)
    {
        using var client = CreateClient();
        using var request = new HttpRequestMessage(method, path.TrimStart('/'));
        ApplyAuth(request);

        if (body != null)
        {
            request.Content = new StringContent(
                JsonSerializer.Serialize(body, JsonOptions),
                Encoding.UTF8,
                "application/json");
        }

        using var response = await client.SendAsync(request, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            var apiMessage = TryExtractMessage(content);
            var isLoginRequest = path.Contains("admin/auth/login", StringComparison.OrdinalIgnoreCase)
                || path.Contains("admin/auth/bootstrap", StringComparison.OrdinalIgnoreCase);
            var hadSession = _tokenSession.IsAuthenticated();

            if (!isLoginRequest && hadSession)
            {
                _tokenSession.Clear();
                throw new ApiException(401, "Session expired. Please sign in again.", content);
            }

            throw new ApiException(401, apiMessage ?? "Invalid username or password.", content);
        }

        if (!response.IsSuccessStatusCode)
        {
            var message = TryExtractMessage(content) ?? $"API request failed ({(int)response.StatusCode}).";
            throw new ApiException((int)response.StatusCode, message, content);
        }

        if (typeof(T) == typeof(object) || string.IsNullOrWhiteSpace(content))
        {
            return default;
        }

        return JsonSerializer.Deserialize<T>(content, JsonOptions);
    }

    public async Task<byte[]> GetBytesAsync(string path, CancellationToken cancellationToken = default)
    {
        using var client = CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, path.TrimStart('/'));
        ApplyAuth(request);

        using var response = await client.SendAsync(request, cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new ApiException(401, TryExtractMessage(content) ?? "Session expired. Please sign in again.", content);
        }

        if (!response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new ApiException((int)response.StatusCode, TryExtractMessage(content) ?? "Download failed.", content);
        }

        return await response.Content.ReadAsByteArrayAsync(cancellationToken);
    }

    public Task<T?> PostMultipartAsync<T>(string path, MultipartFormDataContent content, CancellationToken cancellationToken = default) =>
        SendMultipartAsync<T>(HttpMethod.Post, path, content, cancellationToken);

    public Task<T?> PutMultipartAsync<T>(string path, MultipartFormDataContent content, CancellationToken cancellationToken = default) =>
        SendMultipartAsync<T>(HttpMethod.Put, path, content, cancellationToken);

    public async Task DeleteAsync(string path, CancellationToken cancellationToken = default)
    {
        await SendAsync<object>(HttpMethod.Delete, path, null, cancellationToken);
    }

    private async Task<T?> SendMultipartAsync<T>(
        HttpMethod method,
        string path,
        MultipartFormDataContent content,
        CancellationToken cancellationToken)
    {
        using var client = CreateClient();
        using var request = new HttpRequestMessage(method, path.TrimStart('/'));
        ApplyAuth(request);
        request.Content = content;

        using var response = await client.SendAsync(request, cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            _tokenSession.Clear();
            throw new ApiException(401, "Session expired. Please sign in again.", responseBody);
        }

        if (!response.IsSuccessStatusCode)
        {
            throw new ApiException(
                (int)response.StatusCode,
                TryExtractMessage(responseBody) ?? $"API request failed ({(int)response.StatusCode}).",
                responseBody);
        }

        if (typeof(T) == typeof(object) || string.IsNullOrWhiteSpace(responseBody))
        {
            return default;
        }

        return JsonSerializer.Deserialize<T>(responseBody, JsonOptions);
    }

    private void ApplyAuth(HttpRequestMessage request)
    {
        var token = _tokenSession.GetToken();
        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }

    private static string? TryExtractMessage(string content)
    {
        try
        {
            using var doc = JsonDocument.Parse(content);
            if (doc.RootElement.TryGetProperty("message", out var message))
            {
                return message.GetString();
            }
        }
        catch
        {
            // ignored
        }

        return null;
    }
}
