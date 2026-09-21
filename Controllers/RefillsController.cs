using HospitalAdminPanel.Factories;
using HospitalAdminPanel.Middleware;
using HospitalAdminPanel.Models.Api;
using HospitalAdminPanel.Models.ViewModels;
using HospitalAdminPanel.Services;
using Microsoft.AspNetCore.Mvc;

namespace HospitalAdminPanel.Controllers;

[AdminAuthorize]
public class RefillsController : Controller
{
    private readonly IApiFactory _apiFactory;

    public RefillsController(IApiFactory apiFactory) => _apiFactory = apiFactory;

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var refills = await _apiFactory.Refills.GetPendingAsync(cancellationToken);
        return View(new RefillListViewModel { Refills = refills });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int refillId, string status, string? statusMessage, CancellationToken cancellationToken)
    {
        try
        {
            await _apiFactory.Refills.UpdateStatusAsync(new UpdateRefillRequest
            {
                RefillId = refillId,
                Status = status,
                StatusMessage = statusMessage,
            }, cancellationToken);

            TempData["Success"] = $"Refill marked as {status}.";
        }
        catch (ApiException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}
