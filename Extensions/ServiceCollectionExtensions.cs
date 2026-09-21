using HospitalAdminPanel.Configuration;
using HospitalAdminPanel.Factories;
using HospitalAdminPanel.Repositories.Implementations;
using HospitalAdminPanel.Repositories.Interfaces;
using HospitalAdminPanel.Services;

namespace HospitalAdminPanel.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAdminPanelServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ApiSettings>(configuration.GetSection(ApiSettings.SectionName));
        services.Configure<AdminPanelSettings>(configuration.GetSection(AdminPanelSettings.SectionName));

        services.AddHttpContextAccessor();
        services.AddDistributedMemoryCache();
        services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromHours(8);
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
            options.Cookie.Name = ".BTIH.Admin.Session";
        });

        services.AddHttpClient(nameof(ApiClient));
        services.AddScoped<IApiClient, ApiClient>();

        services.AddScoped<ITokenSessionService, TokenSessionService>();
        services.AddScoped<IAuthRepository, AuthRepository>();
        services.AddScoped<IDashboardRepository, DashboardRepository>();
        services.AddScoped<IMessageRepository, MessageRepository>();
        services.AddScoped<IAppointmentRepository, AppointmentRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IReportRepository, ReportRepository>();
        services.AddScoped<IRefillRepository, RefillRepository>();
        services.AddScoped<ISupportTicketRepository, SupportTicketRepository>();
        services.AddScoped<IAuditRepository, AuditRepository>();
        services.AddScoped<IApiFactory, ApiFactory>();

        return services;
    }
}
