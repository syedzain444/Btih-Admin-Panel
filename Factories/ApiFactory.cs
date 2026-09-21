using HospitalAdminPanel.Repositories.Interfaces;

namespace HospitalAdminPanel.Factories;

public class ApiFactory : IApiFactory
{
    public ApiFactory(
        IAuthRepository auth,
        IDashboardRepository dashboard,
        IMessageRepository messages,
        IAppointmentRepository appointments,
        IUserRepository users,
        IReportRepository reports,
        IRefillRepository refills,
        ISupportTicketRepository supportTickets,
        IAuditRepository audit)
    {
        Auth = auth;
        Dashboard = dashboard;
        Messages = messages;
        Appointments = appointments;
        Users = users;
        Reports = reports;
        Refills = refills;
        SupportTickets = supportTickets;
        Audit = audit;
    }

    public IAuthRepository Auth { get; }
    public IDashboardRepository Dashboard { get; }
    public IMessageRepository Messages { get; }
    public IAppointmentRepository Appointments { get; }
    public IUserRepository Users { get; }
    public IReportRepository Reports { get; }
    public IRefillRepository Refills { get; }
    public ISupportTicketRepository SupportTickets { get; }
    public IAuditRepository Audit { get; }
}
