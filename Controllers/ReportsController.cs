using HospitalAdminPanel.Factories;

using HospitalAdminPanel.Helpers;

using HospitalAdminPanel.Middleware;

using HospitalAdminPanel.Models.ViewModels;

using HospitalAdminPanel.Services;

using Microsoft.AspNetCore.Mvc;



namespace HospitalAdminPanel.Controllers;



[AdminAuthorize]

public class ReportsController : Controller

{

    private readonly IApiFactory _apiFactory;



    public ReportsController(IApiFactory apiFactory) => _apiFactory = apiFactory;



    public async Task<IActionResult> Index(string? dateFrom, string? dateTo, CancellationToken cancellationToken)

    {

        var vm = new ReportsViewModel

        {

            DateFrom = dateFrom ?? DateTime.Today.AddDays(-30).ToString("yyyy-MM-dd"),

            DateTo = dateTo ?? DateTime.Today.ToString("yyyy-MM-dd"),

        };



        try

        {

            vm.Summary = await _apiFactory.Reports.GetSummaryAsync(cancellationToken);

            vm.AuditLogs = await _apiFactory.Reports.GetRecentAuditAsync(100, cancellationToken);

            vm.RegistrationReport = await _apiFactory.Reports.GetRegistrationsAsync(vm.DateFrom, vm.DateTo, cancellationToken);

            vm.AppointmentReport = await _apiFactory.Reports.GetAppointmentsAsync(null, vm.DateFrom, vm.DateTo, cancellationToken);

        }

        catch (ApiException ex) when (ex.StatusCode == 401)

        {

            return RedirectToAction("Login", "Account");

        }

        catch (ApiException ex)

        {

            TempData["Error"] = ex.Message;

            vm.MissingApis = MissingApiCatalog.Reports;

        }



        return View(vm);

    }



    public async Task<IActionResult> ExportEngagement(string? dateFrom, string? dateTo, CancellationToken cancellationToken)

    {

        var from = dateFrom ?? DateTime.Today.AddDays(-30).ToString("yyyy-MM-dd");

        var to = dateTo ?? DateTime.Today.ToString("yyyy-MM-dd");



        try

        {

            var bytes = await _apiFactory.Reports.ExportEngagementAsync(from, to, cancellationToken);

            return File(bytes, "text/csv", $"engagement-{from}-to-{to}.csv");

        }

        catch (ApiException ex) when (ex.StatusCode == 401)

        {

            return RedirectToAction("Login", "Account");

        }

        catch (ApiException ex)

        {

            TempData["Error"] = ex.Message;

            return RedirectToAction(nameof(Index), new { dateFrom = from, dateTo = to });

        }

    }

}


