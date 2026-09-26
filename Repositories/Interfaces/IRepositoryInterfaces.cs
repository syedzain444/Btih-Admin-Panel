using HospitalAdminPanel.Models.Api;

namespace HospitalAdminPanel.Repositories.Interfaces;

public interface IAuthRepository
{
    Task<AdminLoginResponse?> LoginAsync(AdminLoginRequest request, CancellationToken cancellationToken = default);
}

public interface IDashboardRepository
{
    Task<DashboardCountsApiModel?> GetCountsAsync(CancellationToken cancellationToken = default);
    Task<bool> IsApiHealthyAsync(CancellationToken cancellationToken = default);
    Task<UserAnalyticsApiModel?> GetUserAnalyticsAsync(CancellationToken cancellationToken = default);
    Task<VisitAnalyticsApiModel?> GetVisitAnalyticsAsync(CancellationToken cancellationToken = default);
    Task<EngagementAnalyticsApiModel?> GetEngagementAnalyticsAsync(CancellationToken cancellationToken = default);
}

public interface IMessageRepository
{
    Task<List<MessageThreadApiModel>> GetThreadsAsync(CancellationToken cancellationToken = default);
    Task<List<MessageItemApiModel>> GetThreadMessagesAsync(int threadId, CancellationToken cancellationToken = default);
    Task<MessageItemApiModel?> ReplyAsync(AdminReplyRequest request, CancellationToken cancellationToken = default);
}

public interface IAppointmentRepository
{
    Task<List<AppointmentApiModel>> GetByMrNoAsync(string mrNo, CancellationToken cancellationToken = default);
    Task<PagedApiResponse<AppointmentApiModel>> GetAllAsync(
        string? status,
        string? from,
        string? to,
        string? search,
        int page = 1,
        int pageSize = 500,
        CancellationToken cancellationToken = default);
    Task<bool> ApproveAsync(string appointmentId, string? notes, CancellationToken cancellationToken = default);
    Task<bool> RejectAsync(string appointmentId, string? notes, CancellationToken cancellationToken = default);
}

public interface IUserRepository
{
    Task<PatientProfileApiModel?> GetByMrNoAsync(string mrNo, CancellationToken cancellationToken = default);
    Task<PagedApiResponse<AdminPortalUserApiModel>> GetAllAsync(
        string? search,
        int page = 1,
        int pageSize = 500,
        CancellationToken cancellationToken = default);
}

public interface IReportRepository
{
    Task<DashboardCountsApiModel?> GetSummaryAsync(CancellationToken cancellationToken = default);
    Task<List<AuditLogApiModel>> GetRecentAuditAsync(int take = 50, CancellationToken cancellationToken = default);
    Task<RegistrationReportApiModel?> GetRegistrationsAsync(string? from, string? to, CancellationToken cancellationToken = default);
    Task<AppointmentReportApiModel?> GetAppointmentsAsync(string? status, string? from, string? to, CancellationToken cancellationToken = default);
    Task<byte[]> ExportEngagementAsync(string? from, string? to, CancellationToken cancellationToken = default);
}

public interface IRefillRepository
{
    Task<List<RefillRequestApiModel>> GetPendingAsync(CancellationToken cancellationToken = default);
    Task<bool> UpdateStatusAsync(UpdateRefillRequest request, CancellationToken cancellationToken = default);
}

public interface ISupportTicketRepository
{
    Task<List<SupportTicketApiModel>> GetOpenAsync(CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(UpdateTicketRequest request, CancellationToken cancellationToken = default);
}

public interface IAuditRepository
{
    Task<List<AuditLogApiModel>> GetRecentAsync(int take = 100, string? mrNo = null, CancellationToken cancellationToken = default);
}

public interface IPromotionRepository
{
    Task<List<PromotionApiModel>> GetAllAsync(CancellationToken cancellationToken = default);
    Task CreateAsync(MultipartFormDataContent content, CancellationToken cancellationToken = default);
    Task UpdateAsync(int promotionId, MultipartFormDataContent content, CancellationToken cancellationToken = default);
    Task DeleteAsync(int promotionId, CancellationToken cancellationToken = default);
}
