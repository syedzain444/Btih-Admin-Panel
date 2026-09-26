namespace HospitalAdminPanel.Services;

public interface IApiClient
{
    Task<T?> GetAsync<T>(string path, CancellationToken cancellationToken = default);
    Task<TResponse?> PostAsync<TRequest, TResponse>(string path, TRequest body, CancellationToken cancellationToken = default);
    Task<TResponse?> PutAsync<TRequest, TResponse>(string path, TRequest body, CancellationToken cancellationToken = default);
    Task<TResponse?> PatchAsync<TRequest, TResponse>(string path, TRequest body, CancellationToken cancellationToken = default);
    Task<bool> PostAsync(string path, object body, CancellationToken cancellationToken = default);
    Task<bool> PutAsync(string path, object body, CancellationToken cancellationToken = default);
    Task<byte[]> GetBytesAsync(string path, CancellationToken cancellationToken = default);
    Task<T?> PostMultipartAsync<T>(string path, MultipartFormDataContent content, CancellationToken cancellationToken = default);
    Task<T?> PutMultipartAsync<T>(string path, MultipartFormDataContent content, CancellationToken cancellationToken = default);
    Task DeleteAsync(string path, CancellationToken cancellationToken = default);
}

public class ApiException : Exception
{
    public int StatusCode { get; }
    public string? ResponseBody { get; }

    public ApiException(int statusCode, string message, string? responseBody = null) : base(message)
    {
        StatusCode = statusCode;
        ResponseBody = responseBody;
    }
}
