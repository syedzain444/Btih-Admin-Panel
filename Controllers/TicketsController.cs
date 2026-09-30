using HospitalAdminPanel.Factories;
using HospitalAdminPanel.Helpers;
using HospitalAdminPanel.Middleware;
using HospitalAdminPanel.Models.Api;
using HospitalAdminPanel.Models.ViewModels;
using HospitalAdminPanel.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace HospitalAdminPanel.Controllers;

[AdminAuthorize]
[AdminPermission(AdminModules.Tickets)]
public class TicketsController : Controller
{
    private readonly IApiFactory _apiFactory;
    private readonly IMemoryCache _cache;

    public TicketsController(IApiFactory apiFactory, IMemoryCache cache)
    {
        _apiFactory = apiFactory;
        _cache = cache;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var tickets = await _apiFactory.SupportTickets.GetOpenAsync(cancellationToken);
        ViewData["TicketCount"] = tickets.Count;
        TicketNavBadgeFilter.Invalidate(_cache);
        _cache.Set(TicketNavBadgeFilter.CacheKey, tickets.Count, TimeSpan.FromSeconds(20));
        return View(new TicketListViewModel { Tickets = tickets });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(int ticketId, string status, string? adminNotes, CancellationToken cancellationToken)
    {
        try
        {
            await _apiFactory.SupportTickets.UpdateAsync(new UpdateTicketRequest
            {
                TicketId = ticketId,
                Status = status,
                AdminNotes = adminNotes,
            }, cancellationToken);

            TicketNavBadgeFilter.Invalidate(_cache);
            TempData["Success"] = "Ticket updated. The patient has been notified if they have the app installed.";
        }
        catch (ApiException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}
