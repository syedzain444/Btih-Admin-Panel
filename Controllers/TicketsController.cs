using HospitalAdminPanel.Factories;
using HospitalAdminPanel.Middleware;
using HospitalAdminPanel.Models.Api;
using HospitalAdminPanel.Models.ViewModels;
using HospitalAdminPanel.Services;
using Microsoft.AspNetCore.Mvc;

namespace HospitalAdminPanel.Controllers;

[AdminAuthorize]
public class TicketsController : Controller
{
    private readonly IApiFactory _apiFactory;

    public TicketsController(IApiFactory apiFactory) => _apiFactory = apiFactory;

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var tickets = await _apiFactory.SupportTickets.GetOpenAsync(cancellationToken);
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

            TempData["Success"] = "Support ticket updated.";
        }
        catch (ApiException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}
