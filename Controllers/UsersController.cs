using HospitalAdminPanel.Factories;

using HospitalAdminPanel.Helpers;

using HospitalAdminPanel.Middleware;

using HospitalAdminPanel.Models.ViewModels;

using HospitalAdminPanel.Services;

using Microsoft.AspNetCore.Mvc;



namespace HospitalAdminPanel.Controllers;



[AdminAuthorize]

public class UsersController : Controller

{

    private readonly IApiFactory _apiFactory;



    public UsersController(IApiFactory apiFactory) => _apiFactory = apiFactory;



    public async Task<IActionResult> Index(string? search, int page = 1, CancellationToken cancellationToken = default)

    {

        var vm = new UserListViewModel

        {

            Search = search,

            Page = page,

        };



        try

        {

            var result = await _apiFactory.Users.GetAllAsync(search, page, cancellationToken: cancellationToken);

            vm.Users = result.Data;

            vm.Page = result.PageNumber;

            vm.TotalPages = result.TotalPages;

            vm.TotalRecords = result.TotalRecords;

        }

        catch (ApiException ex)

        {

            vm.MissingListAllApi = true;

            vm.InfoMessage = ex.Message;

        }



        return View(vm);

    }



    public async Task<IActionResult> Details(string mrNo, CancellationToken cancellationToken)

    {

        if (string.IsNullOrWhiteSpace(mrNo))

        {

            return RedirectToAction(nameof(Index));

        }



        var vm = new UserDetailsViewModel

        {

            MrNo = mrNo.Trim(),

            MissingAnalyticsApi = false,

        };



        try

        {

            vm.Profile = await _apiFactory.Users.GetByMrNoAsync(mrNo.Trim(), cancellationToken);

            vm.Appointments = await _apiFactory.Appointments.GetByMrNoAsync(mrNo.Trim(), cancellationToken);

            var threads = await _apiFactory.Messages.GetThreadsAsync(cancellationToken);

            vm.MessageThreads = threads.Where(t => t.MrNo.Equals(mrNo.Trim(), StringComparison.OrdinalIgnoreCase)).ToList();

        }

        catch (ApiException ex)

        {

            TempData["Error"] = ex.Message;

        }



        return View(vm);

    }

}


