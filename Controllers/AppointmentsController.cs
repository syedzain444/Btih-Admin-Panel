using HospitalAdminPanel.Factories;

using HospitalAdminPanel.Helpers;

using HospitalAdminPanel.Middleware;

using HospitalAdminPanel.Models.ViewModels;

using HospitalAdminPanel.Services;

using Microsoft.AspNetCore.Mvc;



namespace HospitalAdminPanel.Controllers;



[AdminAuthorize]

public class AppointmentsController : Controller

{

    private readonly IApiFactory _apiFactory;



    public AppointmentsController(IApiFactory apiFactory) => _apiFactory = apiFactory;



    public async Task<IActionResult> Index(

        string? mrNo,

        string? status,

        string? search,

        int page = 1,

        CancellationToken cancellationToken = default)

    {

        var vm = new AppointmentListViewModel

        {

            MrNo = mrNo,

            StatusFilter = status,

            Search = search,

            Page = page,

        };



        try

        {

            if (!string.IsNullOrWhiteSpace(mrNo))

            {

                var appointments = await _apiFactory.Appointments.GetByMrNoAsync(mrNo.Trim(), cancellationToken);

                if (!string.IsNullOrWhiteSpace(status))

                {

                    appointments = appointments

                        .Where(a => (a.Status ?? "").Contains(status, StringComparison.OrdinalIgnoreCase))

                        .ToList();

                }



                if (!string.IsNullOrWhiteSpace(search))

                {

                    appointments = appointments.Where(a =>

                        (a.DoctorName?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) ||

                        (a.Purpose?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) ||

                        (a.AppointmentId?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false)).ToList();

                }



                vm.Appointments = appointments;

                vm.RequiresMrLookup = true;

                vm.TotalRecords = appointments.Count;

            }

            else

            {

                var result = await _apiFactory.Appointments.GetAllAsync(status, null, null, search, page, cancellationToken: cancellationToken);

                vm.Appointments = result.Data;

                vm.Page = result.PageNumber;

                vm.TotalPages = result.TotalPages;

                vm.TotalRecords = result.TotalRecords;

            }

        }

        catch (ApiException ex)

        {

            vm.InfoMessage = ex.Message;

        }



        return View(vm);

    }



    [HttpPost]

    [ValidateAntiForgeryToken]

    public async Task<IActionResult> Approve(string appointmentId, string? notes, CancellationToken cancellationToken)

    {

        try

        {

            await _apiFactory.Appointments.ApproveAsync(appointmentId, notes, cancellationToken);

            TempData["Success"] = "Appointment approved.";

        }

        catch (ApiException ex)

        {

            TempData["Error"] = ex.Message;

        }



        return RedirectToAction(nameof(Index));

    }



    [HttpPost]

    [ValidateAntiForgeryToken]

    public async Task<IActionResult> Reject(string appointmentId, string? notes, CancellationToken cancellationToken)

    {

        try

        {

            await _apiFactory.Appointments.RejectAsync(appointmentId, notes, cancellationToken);

            TempData["Success"] = "Appointment rejected.";

        }

        catch (ApiException ex)

        {

            TempData["Error"] = ex.Message;

        }



        return RedirectToAction(nameof(Index));

    }

}


