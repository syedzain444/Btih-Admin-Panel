using HospitalAdminPanel.Models.Api;
using System.Text.Json;

namespace HospitalAdminPanel.Services;

public class TokenSessionService : ITokenSessionService
{
    private const string TokenKey = "AdminApiToken";
    private const string UserKey = "AdminApiUser";
    private const string ExpiryKey = "AdminApiTokenExpiry";

    private readonly IHttpContextAccessor _httpContextAccessor;

    public TokenSessionService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ISession Session => _httpContextAccessor.HttpContext!.Session;

    public string? GetToken()
    {
        var expiry = Session.GetString(ExpiryKey);
        if (!string.IsNullOrEmpty(expiry) &&
            DateTime.TryParse(expiry, out var expiresAt) &&
            expiresAt <= DateTime.UtcNow)
        {
            Clear();
            return null;
        }

        return Session.GetString(TokenKey);
    }

    public AdminUserApiModel? GetUser()
    {
        var json = Session.GetString(UserKey);
        return string.IsNullOrWhiteSpace(json)
            ? null
            : JsonSerializer.Deserialize<AdminUserApiModel>(json);
    }

    public bool IsAuthenticated() => !string.IsNullOrWhiteSpace(GetToken());

    public void SetSession(string token, AdminUserApiModel user, DateTime expiresAt)
    {
        Session.SetString(TokenKey, token);
        Session.SetString(UserKey, JsonSerializer.Serialize(user));
        Session.SetString(ExpiryKey, expiresAt.ToUniversalTime().ToString("O"));
    }

    public void Clear()
    {
        Session.Remove(TokenKey);
        Session.Remove(UserKey);
        Session.Remove(ExpiryKey);
    }
}
