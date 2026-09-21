using HospitalAdminPanel.Factories;
using HospitalAdminPanel.Helpers;
using HospitalAdminPanel.Middleware;
using HospitalAdminPanel.Models.ViewModels;
using HospitalAdminPanel.Services;
using Microsoft.AspNetCore.Mvc;

namespace HospitalAdminPanel.Controllers;

[AdminAuthorize]
public class DashboardController : Controller
{
    private readonly IApiFactory _apiFactory;

    public DashboardController(IApiFactory apiFactory) => _apiFactory = apiFactory;

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var vm = new DashboardViewModel();

        try
        {
            vm.ApiHealthy = await _apiFactory.Dashboard.IsApiHealthyAsync(cancellationToken);
            var counts = await _apiFactory.Dashboard.GetCountsAsync(cancellationToken);
            if (counts != null)
            {
                vm.OpenTickets = counts.OpenTickets;
                vm.PendingRefills = counts.PendingRefills;
                vm.MessageThreads = counts.MessageThreads;
            }

            var users = await _apiFactory.Dashboard.GetUserAnalyticsAsync(cancellationToken);
            if (users != null)
            {
                vm.TotalUsers = users.TotalUsers;
                vm.ActiveUsers = users.ActiveUsers;
                vm.NewUsers = users.NewUsers;
                vm.ReturningUsers = users.ReturningUsers;
                if (users.Period != null)
                {
                    vm.DateRangeLabel =
                        $"{users.Period.From:MMM d} – {users.Period.To:MMM d, yyyy}";
                }
            }
            else
            {
                vm.MissingApis.Add(MissingApiCatalog.DashboardAnalytics[0]);
            }

            var visits = await _apiFactory.Dashboard.GetVisitAnalyticsAsync(cancellationToken);
            if (visits != null)
            {
                vm.TotalVisits = visits.TotalVisits;
                vm.DailyVisits = visits.Daily;
                vm.WeeklyVisits = visits.Weekly;
            }
            else
            {
                vm.MissingApis.Add(MissingApiCatalog.DashboardAnalytics[1]);
            }

            var engagement = await _apiFactory.Dashboard.GetEngagementAnalyticsAsync(cancellationToken);
            if (engagement != null)
            {
                vm.AvgSessionMinutes = engagement.AvgSessionDurationSeconds / 60;
            }
            else
            {
                vm.MissingApis.Add(MissingApiCatalog.DashboardAnalytics[2]);
            }

            vm.RecentThreads = (await _apiFactory.Messages.GetThreadsAsync(cancellationToken)).Take(5).ToList();
            vm.RecentTickets = (await _apiFactory.SupportTickets.GetOpenAsync(cancellationToken)).Take(5).ToList();
            vm.RecentRefills = (await _apiFactory.Refills.GetPendingAsync(cancellationToken)).Take(5).ToList();
        }
        catch (ApiException ex) when (ex.StatusCode == 401)
        {
            return RedirectToAction("Login", "Account");
        }
        catch (ApiException)
        {
            TempData["Error"] = "Some dashboard data could not be loaded. The API may be unavailable.";
        }

        return View(vm);
    }
}
