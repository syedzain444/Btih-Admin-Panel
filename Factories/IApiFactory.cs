using HospitalAdminPanel.Repositories.Interfaces;

namespace HospitalAdminPanel.Factories;

public interface IApiFactory
{
    IAuthRepository Auth { get; }
    IDashboardRepository Dashboard { get; }
    IMessageRepository Messages { get; }
    IAppointmentRepository Appointments { get; }
    IUserRepository Users { get; }
    IReportRepository Reports { get; }
    IRefillRepository Refills { get; }
    ISupportTicketRepository SupportTickets { get; }
    IAuditRepository Audit { get; }
    IPromotionRepository Promotions { get; }
}
