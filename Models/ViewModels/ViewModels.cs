using HospitalAdminPanel.Models.Api;

namespace HospitalAdminPanel.Models.ViewModels;

public class LoginViewModel
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
    public bool IsLoading { get; set; }
}

public class DashboardViewModel
{
    public int OpenTickets { get; set; }
    public int PendingRefills { get; set; }
    public int MessageThreads { get; set; }
    public bool ApiHealthy { get; set; }
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int NewUsers { get; set; }
    public int ReturningUsers { get; set; }
    public int TotalVisits { get; set; }
    public int AvgSessionMinutes { get; set; }
    public List<VisitBucketApiModel> DailyVisits { get; set; } = new();
    public List<VisitBucketApiModel> WeeklyVisits { get; set; } = new();
    public List<MessageThreadApiModel> RecentThreads { get; set; } = new();
    public List<SupportTicketApiModel> RecentTickets { get; set; } = new();
    public List<RefillRequestApiModel> RecentRefills { get; set; } = new();
    public List<MissingApiNotice> MissingApis { get; set; } = new();
    public string DateRangeLabel { get; set; } = "Last 30 days";
}

public class MissingApiNotice
{
    public string Feature { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class MessageListViewModel
{
    public List<MessageThreadApiModel> Threads { get; set; } = new();
    public string? Search { get; set; }
    public string? StatusFilter { get; set; }
    public bool MissingThreadMessagesApi { get; set; } = true;
    public int AllCount { get; set; }
    public int UnreadCount { get; set; }
}

public class ChatSidebarViewModel
{
    public List<MessageThreadApiModel> Threads { get; set; } = new();
    public int? ActiveThreadId { get; set; }
    public string? Search { get; set; }
    public string? StatusFilter { get; set; }
    public int AllCount { get; set; }
    public int UnreadCount { get; set; }
}

public class MessageReplyViewModel
{
    public MessageThreadApiModel Thread { get; set; } = new();
    public List<MessageThreadApiModel> Threads { get; set; } = new();
    public List<MessageItemApiModel> Messages { get; set; } = new();
    public PatientProfileApiModel? PatientProfile { get; set; }
    public string Body { get; set; } = string.Empty;
    public string? StaffName { get; set; }
    public string? SuccessMessage { get; set; }
    public string? ErrorMessage { get; set; }
    public int AllCount { get; set; }
    public int UnreadCount { get; set; }
}

public class AppointmentListViewModel
{
    public List<AppointmentApiModel> Appointments { get; set; } = new();
    public string? MrNo { get; set; }
    public string? StatusFilter { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int TotalPages { get; set; }
    public int TotalRecords { get; set; }
    public bool RequiresMrLookup { get; set; }
    public bool MissingListAllApi { get; set; }
    public bool MissingApproveRejectApi { get; set; }
    public string? InfoMessage { get; set; }
}

public class UserListViewModel
{
    public List<AdminPortalUserApiModel> Users { get; set; } = new();
    public string? Search { get; set; }
    public string? MrNo { get; set; }
    public int Page { get; set; } = 1;
    public int TotalPages { get; set; }
    public int TotalRecords { get; set; }
    public bool MissingListAllApi { get; set; }
    public string? InfoMessage { get; set; }
}

public class UserDetailsViewModel
{
    public PatientProfileApiModel? Profile { get; set; }
    public List<AppointmentApiModel> Appointments { get; set; } = new();
    public List<MessageThreadApiModel> MessageThreads { get; set; } = new();
    public string MrNo { get; set; } = string.Empty;
    public bool MissingAnalyticsApi { get; set; } = true;
}

public class ReportsViewModel
{
    public DashboardCountsApiModel? Summary { get; set; }
    public RegistrationReportApiModel? RegistrationReport { get; set; }
    public AppointmentReportApiModel? AppointmentReport { get; set; }
    public List<AuditLogApiModel> AuditLogs { get; set; } = new();
    public string DateFrom { get; set; } = DateTime.Today.AddDays(-30).ToString("yyyy-MM-dd");
    public string DateTo { get; set; } = DateTime.Today.ToString("yyyy-MM-dd");
    public List<MissingApiNotice> MissingApis { get; set; } = new();
}

public class RefillListViewModel
{
    public List<RefillRequestApiModel> Refills { get; set; } = new();
}

public class TicketListViewModel
{
    public List<SupportTicketApiModel> Tickets { get; set; } = new();
}

public class PromotionListViewModel
{
    public List<PromotionApiModel> Promotions { get; set; } = new();
    public string ApiBaseUrl { get; set; } = string.Empty;
}

public class PromotionFormViewModel
{
    public int? PromotionId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public int DurationSeconds { get; set; } = 5;
    public bool IsActive { get; set; } = true;
    public DateTime? StartAt { get; set; }
    public DateTime? EndAt { get; set; }
    public string? ExistingImageUrl { get; set; }
    public string ApiBaseUrl { get; set; } = string.Empty;
}

public class PageHeaderViewModel
{
    public string Title { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    public string? Breadcrumb { get; set; }
}
