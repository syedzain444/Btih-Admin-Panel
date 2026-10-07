using HospitalAdminPanel.Models.Api;
using HospitalAdminPanel.Repositories.Interfaces;
using HospitalAdminPanel.Services;

namespace HospitalAdminPanel.Repositories.Implementations;

public class AuthRepository : IAuthRepository
{
    private readonly IApiClient _api;

    public AuthRepository(IApiClient api) => _api = api;

    public Task<AdminLoginResponse?> LoginAsync(AdminLoginRequest request, CancellationToken cancellationToken = default) =>
        _api.PostAsync<AdminLoginRequest, AdminLoginResponse>("api/admin/auth/login", request, cancellationToken);
}

public class DashboardRepository : IDashboardRepository
{
    private readonly IApiClient _api;

    public DashboardRepository(IApiClient api) => _api = api;

    public async Task<DashboardCountsApiModel?> GetCountsAsync(CancellationToken cancellationToken = default)
    {
        var response = await _api.GetAsync<ApiSuccessResponse<DashboardCountsApiModel>>("api/admin/dashboard", cancellationToken);
        return response?.Data;
    }

    public async Task<bool> IsApiHealthyAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var health = await _api.GetAsync<HealthStatusApiModel>("api/Health", cancellationToken);
            return health?.Status?.Equals("Healthy", StringComparison.OrdinalIgnoreCase) == true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<UserAnalyticsApiModel?> GetUserAnalyticsAsync(CancellationToken cancellationToken = default)
    {
        var response = await _api.GetAsync<ApiSuccessResponse<UserAnalyticsApiModel>>(
            "api/admin/analytics/users", cancellationToken);
        return response?.Data;
    }

    public async Task<VisitAnalyticsApiModel?> GetVisitAnalyticsAsync(CancellationToken cancellationToken = default)
    {
        var response = await _api.GetAsync<ApiSuccessResponse<VisitAnalyticsApiModel>>(
            "api/admin/analytics/visits", cancellationToken);
        return response?.Data;
    }

    public async Task<EngagementAnalyticsApiModel?> GetEngagementAnalyticsAsync(CancellationToken cancellationToken = default)
    {
        var response = await _api.GetAsync<ApiSuccessResponse<EngagementAnalyticsApiModel>>(
            "api/admin/analytics/engagement", cancellationToken);
        return response?.Data;
    }
}

public class MessageRepository : IMessageRepository
{
    private readonly IApiClient _api;

    public MessageRepository(IApiClient api) => _api = api;

    public async Task<List<MessageThreadApiModel>> GetThreadsAsync(CancellationToken cancellationToken = default)
    {
        var response = await _api.GetAsync<ApiSuccessResponse<List<MessageThreadApiModel>>>("api/admin/messages/threads", cancellationToken);
        return response?.Data ?? new List<MessageThreadApiModel>();
    }

    public async Task<List<MessageItemApiModel>> GetThreadMessagesAsync(int threadId, CancellationToken cancellationToken = default)
    {
        var response = await _api.GetAsync<PagedApiResponse<MessageItemApiModel>>(
            $"api/admin/messages/threads/{threadId}/messages?pageNumber=1&pageSize=200",
            cancellationToken);
        return response?.Data ?? new List<MessageItemApiModel>();
    }

    public async Task<MessageItemApiModel?> ReplyAsync(AdminReplyRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _api.PostAsync<AdminReplyRequest, ApiSuccessResponse<MessageItemApiModel>>(
            "api/admin/messages/reply", request, cancellationToken);
        return response?.Data;
    }
}

public class AppointmentRepository : IAppointmentRepository
{
    private readonly IApiClient _api;

    public AppointmentRepository(IApiClient api) => _api = api;

    public async Task<List<AppointmentApiModel>> GetByMrNoAsync(string mrNo, CancellationToken cancellationToken = default)
    {
        try
        {
            var list = await _api.GetAsync<List<AppointmentApiModel>>($"api/Patient/appointments/{Uri.EscapeDataString(mrNo)}", cancellationToken);
            return list ?? new List<AppointmentApiModel>();
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return new List<AppointmentApiModel>();
        }
    }

    public async Task<PagedApiResponse<AppointmentApiModel>> GetAllAsync(
        string? status,
        string? from,
        string? to,
        string? search,
        int page = 1,
        int pageSize = 500,
        CancellationToken cancellationToken = default)
    {
        var query = new List<string> { $"page={page}", $"pageSize={pageSize}" };
        if (!string.IsNullOrWhiteSpace(status)) query.Add($"status={Uri.EscapeDataString(status)}");
        if (!string.IsNullOrWhiteSpace(from)) query.Add($"from={Uri.EscapeDataString(from)}");
        if (!string.IsNullOrWhiteSpace(to)) query.Add($"to={Uri.EscapeDataString(to)}");
        if (!string.IsNullOrWhiteSpace(search)) query.Add($"search={Uri.EscapeDataString(search)}");

        var response = await _api.GetAsync<PagedApiResponse<AppointmentApiModel>>(
            $"api/admin/appointments?{string.Join('&', query)}",
            cancellationToken);

        return response ?? new PagedApiResponse<AppointmentApiModel>();
    }

    public Task<bool> ApproveAsync(string appointmentId, string? notes, CancellationToken cancellationToken = default) =>
        _api.PostAsync($"api/admin/appointments/{Uri.EscapeDataString(appointmentId)}/approve", new AdminAppointmentActionRequest { Notes = notes }, cancellationToken);

    public Task<bool> RejectAsync(string appointmentId, string? notes, CancellationToken cancellationToken = default) =>
        _api.PostAsync($"api/admin/appointments/{Uri.EscapeDataString(appointmentId)}/reject", new AdminAppointmentActionRequest { Notes = notes }, cancellationToken);
}

public class UserRepository : IUserRepository
{
    private readonly IApiClient _api;

    public UserRepository(IApiClient api) => _api = api;

    public async Task<PatientProfileApiModel?> GetByMrNoAsync(string mrNo, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _api.GetAsync<PatientProfileWrapper>(
                $"api/Patient?MR_NO={Uri.EscapeDataString(mrNo)}&visitPageNumber=1&visitPageSize=1",
                cancellationToken);
            return response?.Profile;
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return null;
        }
    }

    public async Task<PagedApiResponse<AdminPortalUserApiModel>> GetAllAsync(
        string? search,
        int page = 1,
        int pageSize = 500,
        CancellationToken cancellationToken = default)
    {
        var query = $"page={page}&pageSize={pageSize}";
        if (!string.IsNullOrWhiteSpace(search))
        {
            query += $"&search={Uri.EscapeDataString(search)}";
        }

        var response = await _api.GetAsync<PagedApiResponse<AdminPortalUserApiModel>>(
            $"api/admin/users?{query}",
            cancellationToken);

        return response ?? new PagedApiResponse<AdminPortalUserApiModel>();
    }
}

public class ReportRepository : IReportRepository
{
    private readonly IApiClient _api;

    public ReportRepository(IApiClient api) => _api = api;

    public async Task<DashboardCountsApiModel?> GetSummaryAsync(CancellationToken cancellationToken = default)
    {
        var response = await _api.GetAsync<ApiSuccessResponse<DashboardCountsApiModel>>("api/admin/dashboard", cancellationToken);
        return response?.Data;
    }

    public async Task<List<AuditLogApiModel>> GetRecentAuditAsync(int take = 50, CancellationToken cancellationToken = default)
    {
        var response = await _api.GetAsync<ApiSuccessResponse<List<AuditLogApiModel>>>($"api/admin/audit?take={take}", cancellationToken);
        return response?.Data ?? new List<AuditLogApiModel>();
    }

    public async Task<RegistrationReportApiModel?> GetRegistrationsAsync(string? from, string? to, CancellationToken cancellationToken = default)
    {
        var query = BuildDateQuery(from, to);
        var response = await _api.GetAsync<ApiSuccessResponse<RegistrationReportApiModel>>(
            $"api/admin/reports/registrations{query}",
            cancellationToken);
        return response?.Data;
    }

    public async Task<AppointmentReportApiModel?> GetAppointmentsAsync(string? status, string? from, string? to, CancellationToken cancellationToken = default)
    {
        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(status)) parts.Add($"status={Uri.EscapeDataString(status)}");
        if (!string.IsNullOrWhiteSpace(from)) parts.Add($"from={Uri.EscapeDataString(from)}");
        if (!string.IsNullOrWhiteSpace(to)) parts.Add($"to={Uri.EscapeDataString(to)}");
        var query = parts.Count == 0 ? string.Empty : "?" + string.Join('&', parts);

        var response = await _api.GetAsync<ApiSuccessResponse<AppointmentReportApiModel>>(
            $"api/admin/reports/appointments{query}",
            cancellationToken);
        return response?.Data;
    }

    public Task<byte[]> ExportEngagementAsync(string? from, string? to, CancellationToken cancellationToken = default)
    {
        var query = BuildDateQuery(from, to);
        return _api.GetBytesAsync($"api/admin/reports/engagement/export{query}", cancellationToken);
    }

    private static string BuildDateQuery(string? from, string? to)
    {
        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(from)) parts.Add($"from={Uri.EscapeDataString(from)}");
        if (!string.IsNullOrWhiteSpace(to)) parts.Add($"to={Uri.EscapeDataString(to)}");
        return parts.Count == 0 ? string.Empty : "?" + string.Join('&', parts);
    }
}

public class RefillRepository : IRefillRepository
{
    private readonly IApiClient _api;

    public RefillRepository(IApiClient api) => _api = api;

    public async Task<List<RefillRequestApiModel>> GetPendingAsync(CancellationToken cancellationToken = default)
    {
        var response = await _api.GetAsync<ApiSuccessResponse<List<RefillRequestApiModel>>>("api/admin/refills/pending", cancellationToken);
        return response?.Data ?? new List<RefillRequestApiModel>();
    }

    public async Task<bool> UpdateStatusAsync(UpdateRefillRequest request, CancellationToken cancellationToken = default)
    {
        await _api.PutAsync("api/admin/refills/status", request, cancellationToken);
        return true;
    }
}

public class SupportTicketRepository : ISupportTicketRepository
{
    private readonly IApiClient _api;

    public SupportTicketRepository(IApiClient api) => _api = api;

    public async Task<List<SupportTicketApiModel>> GetOpenAsync(CancellationToken cancellationToken = default)
    {
        var response = await _api.GetAsync<ApiSuccessResponse<List<SupportTicketApiModel>>>("api/admin/support/tickets", cancellationToken);
        return response?.Data ?? new List<SupportTicketApiModel>();
    }

    public async Task<bool> UpdateAsync(UpdateTicketRequest request, CancellationToken cancellationToken = default)
    {
        await _api.PutAsync("api/admin/support/tickets", request, cancellationToken);
        return true;
    }
}

public class AuditRepository : IAuditRepository
{
    private readonly IApiClient _api;

    public AuditRepository(IApiClient api) => _api = api;

    public async Task<List<AuditLogApiModel>> GetRecentAsync(int take = 100, string? mrNo = null, CancellationToken cancellationToken = default)
    {
        var path = $"api/admin/audit?take={take}";
        if (!string.IsNullOrWhiteSpace(mrNo))
        {
            path += $"&mrNo={Uri.EscapeDataString(mrNo)}";
        }

        var response = await _api.GetAsync<ApiSuccessResponse<List<AuditLogApiModel>>>(path, cancellationToken);
        return response?.Data ?? new List<AuditLogApiModel>();
    }
}

public class PromotionRepository : IPromotionRepository
{
    private readonly IApiClient _api;

    public PromotionRepository(IApiClient api) => _api = api;

    public async Task<(List<PromotionApiModel> Promotions, int DisplayLimit)> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var response = await _api.GetAsync<PromotionListApiResponse>(
            "api/admin/promotions",
            cancellationToken);
        return (
            response?.Data ?? new List<PromotionApiModel>(),
            response?.DisplayLimit > 0 ? response.DisplayLimit : 5);
    }

    public async Task CreateAsync(MultipartFormDataContent content, CancellationToken cancellationToken = default)
    {
        await _api.PostMultipartAsync<object>("api/admin/promotions", content, cancellationToken);
    }

    public async Task UpdateAsync(int promotionId, MultipartFormDataContent content, CancellationToken cancellationToken = default)
    {
        await _api.PutMultipartAsync<object>($"api/admin/promotions/{promotionId}", content, cancellationToken);
    }

    public async Task DeleteAsync(int promotionId, CancellationToken cancellationToken = default)
    {
        await _api.DeleteAsync($"api/admin/promotions/{promotionId}", cancellationToken);
    }

    public async Task SetDisplayLimitAsync(int displayLimit, CancellationToken cancellationToken = default)
    {
        await _api.PutAsync(
            "api/admin/promotions/display-limit",
            new PromotionDisplayLimitRequest { DisplayLimit = displayLimit },
            cancellationToken);
    }
}

public class OfferRepository : IOfferRepository
{
    private readonly IApiClient _api;

    public OfferRepository(IApiClient api) => _api = api;

    public async Task<List<OfferApiModel>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var response = await _api.GetAsync<OfferListApiResponse>(
            "api/admin/offers",
            cancellationToken);
        return response?.Data ?? new List<OfferApiModel>();
    }

    public async Task CreateAsync(MultipartFormDataContent content, CancellationToken cancellationToken = default)
    {
        await _api.PostMultipartAsync<object>("api/admin/offers", content, cancellationToken);
    }

    public async Task UpdateAsync(int offerId, MultipartFormDataContent content, CancellationToken cancellationToken = default)
    {
        await _api.PutMultipartAsync<object>($"api/admin/offers/{offerId}", content, cancellationToken);
    }

    public async Task DeleteAsync(int offerId, CancellationToken cancellationToken = default)
    {
        await _api.DeleteAsync($"api/admin/offers/{offerId}", cancellationToken);
    }
}

public class RbacRepository : IRbacRepository
{
    private readonly IApiClient _api;

    public RbacRepository(IApiClient api) => _api = api;

    public async Task<List<AdminPermissionApiModel>> GetPermissionsAsync(CancellationToken cancellationToken = default)
    {
        var response = await _api.GetAsync<ApiSuccessResponse<List<AdminPermissionApiModel>>>(
            "api/admin/rbac/permissions", cancellationToken);
        return response?.Data ?? new List<AdminPermissionApiModel>();
    }

    public async Task<List<AdminRoleApiModel>> GetRolesAsync(bool includeInactive = true, CancellationToken cancellationToken = default)
    {
        var response = await _api.GetAsync<ApiSuccessResponse<List<AdminRoleApiModel>>>(
            $"api/admin/rbac/roles?includeInactive={(includeInactive ? "true" : "false")}",
            cancellationToken);
        return response?.Data ?? new List<AdminRoleApiModel>();
    }

    public async Task<AdminRoleApiModel?> GetRoleAsync(int roleId, CancellationToken cancellationToken = default)
    {
        var response = await _api.GetAsync<ApiSuccessResponse<AdminRoleApiModel>>(
            $"api/admin/rbac/roles/{roleId}", cancellationToken);
        return response?.Data;
    }

    public Task CreateRoleAsync(object body, CancellationToken cancellationToken = default) =>
        _api.PostAsync("api/admin/rbac/roles", body, cancellationToken);

    public Task UpdateRoleAsync(int roleId, object body, CancellationToken cancellationToken = default) =>
        _api.PutAsync($"api/admin/rbac/roles/{roleId}", body, cancellationToken);

    public Task SetRolePermissionsAsync(int roleId, IEnumerable<string> permissions, CancellationToken cancellationToken = default) =>
        _api.PutAsync($"api/admin/rbac/roles/{roleId}/permissions", new { permissions }, cancellationToken);

    public Task DeleteRoleAsync(int roleId, CancellationToken cancellationToken = default) =>
        _api.DeleteAsync($"api/admin/rbac/roles/{roleId}", cancellationToken);

    public async Task<List<AdminStaffUserApiModel>> GetUsersAsync(CancellationToken cancellationToken = default)
    {
        var response = await _api.GetAsync<ApiSuccessResponse<List<AdminStaffUserApiModel>>>(
            "api/admin/rbac/users", cancellationToken);
        return response?.Data ?? new List<AdminStaffUserApiModel>();
    }

    public Task CreateUserAsync(object body, CancellationToken cancellationToken = default) =>
        _api.PostAsync("api/admin/rbac/users", body, cancellationToken);

    public Task UpdateUserAsync(int adminId, object body, CancellationToken cancellationToken = default) =>
        _api.PutAsync($"api/admin/rbac/users/{adminId}", body, cancellationToken);

    public Task ResetPasswordAsync(int adminId, string newPassword, CancellationToken cancellationToken = default) =>
        _api.PostAsync($"api/admin/rbac/users/{adminId}/reset-password", new { newPassword }, cancellationToken);
}

public class SupportContentRepository : ISupportContentRepository
{
    private readonly IApiClient _api;

    public SupportContentRepository(IApiClient api) => _api = api;

    public async Task<SupportContactApiModel?> GetContactAsync(CancellationToken cancellationToken = default)
    {
        var response = await _api.GetAsync<SupportContactApiResponse>(
            "api/admin/support/contact",
            cancellationToken);
        return response?.Data;
    }

    public async Task UpdateContactAsync(SupportContactApiModel contact, CancellationToken cancellationToken = default)
    {
        await _api.PutAsync("api/admin/support/contact", contact, cancellationToken);
    }

    public async Task<List<FaqAdminApiModel>> GetFaqsAsync(CancellationToken cancellationToken = default)
    {
        var response = await _api.GetAsync<FaqAdminListApiResponse>(
            "api/admin/support/faq",
            cancellationToken);
        return response?.Data ?? new List<FaqAdminApiModel>();
    }

    public async Task CreateFaqAsync(FaqAdminApiModel faq, CancellationToken cancellationToken = default)
    {
        await _api.PostAsync("api/admin/support/faq", faq, cancellationToken);
    }

    public async Task UpdateFaqAsync(int faqId, FaqAdminApiModel faq, CancellationToken cancellationToken = default)
    {
        await _api.PutAsync($"api/admin/support/faq/{faqId}", faq, cancellationToken);
    }

    public async Task DeleteFaqAsync(int faqId, CancellationToken cancellationToken = default)
    {
        await _api.DeleteAsync($"api/admin/support/faq/{faqId}", cancellationToken);
    }
}
