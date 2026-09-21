using HospitalAdminPanel.Models.Api;

namespace HospitalAdminPanel.Services;

public interface ITokenSessionService
{
    string? GetToken();
    AdminUserApiModel? GetUser();
    bool IsAuthenticated();
    void SetSession(string token, AdminUserApiModel user, DateTime expiresAt);
    void Clear();
}
