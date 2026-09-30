using HospitalAdminPanel.Factories;
using HospitalAdminPanel.Helpers;
using HospitalAdminPanel.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;

namespace HospitalAdminPanel.Middleware;

/// <summary>
/// Loads open support-ticket count into ViewData for nav badge / staff notification.
/// Cached briefly to avoid an API call on every request. Skipped for roles without Tickets access.
/// </summary>
public class TicketNavBadgeFilter : IAsyncActionFilter
{
    public const string CacheKey = "admin_open_ticket_count";
    public const string ViewDataKey = "TicketCount";

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var executed = await next();

        if (context.Controller is not Controller controller)
        {
            return;
        }

        if (executed.Result is not ViewResult)
        {
            return;
        }

        var tokenSession = context.HttpContext.RequestServices.GetService<ITokenSessionService>();
        if (tokenSession == null || !tokenSession.IsAuthenticated())
        {
            controller.ViewData[ViewDataKey] = 0;
            return;
        }

        var role = tokenSession.GetUser()?.Role;
        if (!AdminModulePermissions.CanAccess(role, AdminModules.Tickets))
        {
            controller.ViewData[ViewDataKey] = 0;
            return;
        }

        var cache = context.HttpContext.RequestServices.GetRequiredService<IMemoryCache>();
        if (!cache.TryGetValue(CacheKey, out int count))
        {
            try
            {
                var api = context.HttpContext.RequestServices.GetRequiredService<IApiFactory>();
                var tickets = await api.SupportTickets.GetOpenAsync(context.HttpContext.RequestAborted);
                count = tickets.Count;
            }
            catch
            {
                count = 0;
            }

            cache.Set(CacheKey, count, TimeSpan.FromSeconds(20));
        }

        controller.ViewData[ViewDataKey] = count;
    }

    public static void Invalidate(IMemoryCache cache) => cache.Remove(CacheKey);
}
