using System.Text.Json.Serialization;

namespace HospitalAdminPanel.Models.Api;

public class ApiSuccessResponse<T>
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }
}

public class PagedApiResponse<T>
{
    public bool Success { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalRecords { get; set; }
    public int TotalPages { get; set; }
    public List<T> Data { get; set; } = new();
}

public class AdminLoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class AdminLoginResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? Token { get; set; }
    public string? TokenType { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public int ExpiresInSeconds { get; set; }
    public AdminUserApiModel? User { get; set; }
}

public class AdminUserApiModel
{
    public int AdminId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public class DashboardCountsApiModel
{
    public int OpenTickets { get; set; }
    public int PendingRefills { get; set; }
    public int MessageThreads { get; set; }
}

public class AnalyticsPeriodApiModel
{
    public DateTime From { get; set; }
    public DateTime To { get; set; }
}

public class UserAnalyticsApiModel
{
    public AnalyticsPeriodApiModel? Period { get; set; }
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int NewUsers { get; set; }
    public int ReturningUsers { get; set; }
    public int MobileRegistrations { get; set; }
    public int HmisPortalUsers { get; set; }
}

public class VisitBucketApiModel
{
    public string Label { get; set; } = string.Empty;
    public DateTime PeriodStart { get; set; }
    public int Visits { get; set; }
    public int UniqueUsers { get; set; }
}

public class VisitAnalyticsApiModel
{
    public AnalyticsPeriodApiModel? Period { get; set; }
    public int TotalVisits { get; set; }
    public int UniqueUsers { get; set; }
    public List<VisitBucketApiModel> Daily { get; set; } = new();
    public List<VisitBucketApiModel> Weekly { get; set; } = new();
    public List<VisitBucketApiModel> Monthly { get; set; } = new();
}

public class EngagementAnalyticsApiModel
{
    public AnalyticsPeriodApiModel? Period { get; set; }
    public int TotalSessions { get; set; }
    public int CompletedSessions { get; set; }
    public int AvgSessionDurationSeconds { get; set; }
    public int MedianSessionDurationSeconds { get; set; }
    public long TotalTimeSpentSeconds { get; set; }
    public bool SessionTrackingAvailable { get; set; }
}

public class MessageThreadApiModel
{
    public int ThreadId { get; set; }
    public string MrNo { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? LastMessagePreview { get; set; }
    public int UnreadCount { get; set; }
}

public class MessageItemApiModel
{
    public int MessageId { get; set; }
    public int ThreadId { get; set; }
    public string SenderType { get; set; } = string.Empty;
    public string? SenderName { get; set; }
    public string? Body { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<MessageAttachmentApiModel> Attachments { get; set; } = new();
}

public class MessageAttachmentApiModel
{
    public int AttachmentId { get; set; }
    public int MessageId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public string? ContentType { get; set; }
    public long? FileSize { get; set; }
}

public class AdminPortalUserApiModel
{
    public string MrNo { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? ContactNo { get; set; }
    public string? Email { get; set; }
    public string Source { get; set; } = string.Empty;
    public bool ProfileSetupComplete { get; set; }
    public DateTime? RegisteredAt { get; set; }

    [JsonIgnore]
    public string? PatientName => string.Join(" ", new[] { FirstName, LastName }.Where(s => !string.IsNullOrWhiteSpace(s)));
}

public class AdminAppointmentActionRequest
{
    public string? Notes { get; set; }
}

public class ReportBucketApiModel
{
    public string Label { get; set; } = string.Empty;
    public DateTime PeriodStart { get; set; }
    public int Count { get; set; }
}

public class StatusCountApiModel
{
    public string Status { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class RegistrationReportApiModel
{
    public AnalyticsPeriodApiModel? Period { get; set; }
    public int TotalRegistrations { get; set; }
    public int MobileRegistrations { get; set; }
    public int HmisPortalUsers { get; set; }
    public List<ReportBucketApiModel> Daily { get; set; } = new();
}

public class AppointmentReportApiModel
{
    public AnalyticsPeriodApiModel? Period { get; set; }
    public string? StatusFilter { get; set; }
    public int TotalAppointments { get; set; }
    public List<StatusCountApiModel> ByStatus { get; set; } = new();
    public List<ReportBucketApiModel> Daily { get; set; } = new();
}

public class AdminReplyRequest
{
    public int ThreadId { get; set; }
    public string Body { get; set; } = string.Empty;
    public string? StaffName { get; set; }
}

public class RefillRequestApiModel
{
    public int RefillId { get; set; }
    public string MrNo { get; set; } = string.Empty;
    public int? MedicationId { get; set; }
    public string? MedicationName { get; set; }
    public int? Quantity { get; set; }
    public string? Notes { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? StatusMessage { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class UpdateRefillRequest
{
    public int RefillId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? StatusMessage { get; set; }
}

public class SupportTicketApiModel
{
    public int TicketId { get; set; }
    public string? MrNo { get; set; }
    public string ContactName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? AdminNotes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class UpdateTicketRequest
{
    public int TicketId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? AdminNotes { get; set; }
}

public class AuditLogApiModel
{
    public int AuditId { get; set; }
    public string? ActorId { get; set; }
    public string? ActorRole { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? EntityType { get; set; }
    public string? MrNo { get; set; }
    public string? Details { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class AppointmentApiModel
{
    public string? AppointmentId { get; set; }
    public string? Name { get; set; }
    public string? PhoneNo { get; set; }
    public string? MRNo { get; set; }
    public string? Email { get; set; }
    public int? WeekId { get; set; }
    public string? AppointmentTime { get; set; }
    public string? Status { get; set; }
    public string? DoctorName { get; set; }
    public int? DoctorId { get; set; }
    public int? DepartmentId { get; set; }
    public string? Purpose { get; set; }
    public DateTime? CreatedAt { get; set; }
}

public class PatientProfileWrapper
{
    public PatientProfileApiModel? Profile { get; set; }
}

public class PatientProfileApiModel
{
    public string? MrNo { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? ContactNo { get; set; }
    public string? EmailAddress { get; set; }
    public string? Gender { get; set; }
    public DateTime? DateOfBirth { get; set; }

    [JsonIgnore]
    public string? PatientName => string.Join(" ", new[] { FirstName, LastName }.Where(s => !string.IsNullOrWhiteSpace(s)));

    [JsonIgnore]
    public string? MRNo => MrNo;

    [JsonIgnore]
    public string? Email => EmailAddress;
}

public class HealthStatusApiModel
{
    public string? Status { get; set; }
    public bool Success { get; set; }
}
