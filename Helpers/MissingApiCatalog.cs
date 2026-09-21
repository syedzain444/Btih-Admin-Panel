using HospitalAdminPanel.Models.ViewModels;

namespace HospitalAdminPanel.Helpers;

public static class MissingApiCatalog
{
    public static List<MissingApiNotice> DashboardAnalytics => new()
    {
        new() { Feature = "User statistics", Description = "GET /api/admin/analytics/users — total/active/new/returning users" },
        new() { Feature = "Visit analytics", Description = "GET /api/admin/analytics/visits — daily/weekly/monthly app visits" },
        new() { Feature = "Engagement metrics", Description = "GET /api/admin/analytics/engagement — session duration, time spent" },
    };

    public static List<MissingApiNotice> Reports => new()
    {
        new() { Feature = "Registration report", Description = "GET /api/admin/reports/registrations?from=&to=" },
        new() { Feature = "Appointment report", Description = "GET /api/admin/reports/appointments?status=&from=&to=" },
        new() { Feature = "Engagement export", Description = "GET /api/admin/reports/engagement/export" },
    };

    public static MissingApiNotice AppointmentListAll => new()
    {
        Feature = "List all appointments",
        Description = "GET /api/admin/appointments — paginated list with status filters and approve/reject actions",
    };

    public static MissingApiNotice UserListAll => new()
    {
        Feature = "List all portal users",
        Description = "GET /api/admin/users — search, filter, pagination for registered mobile app users",
    };

    public static MissingApiNotice MessageThreadHistory => new()
    {
        Feature = "Read message thread history",
        Description = "GET /api/admin/messages/threads/{threadId}/messages — full conversation for admin inbox",
    };
}
