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
