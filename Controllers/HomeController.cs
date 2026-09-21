using HospitalAdminPanel.Models;
using HospitalAdminPanel.Services;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace HospitalAdminPanel.Controllers;

public class HomeController : Controller
{
    private readonly ITokenSessionService _tokenSession;
    private readonly IWebHostEnvironment _environment;

    public HomeController(ITokenSessionService tokenSession, IWebHostEnvironment environment)
    {
        _tokenSession = tokenSession;
        _environment = environment;
    }

    public IActionResult Index()
    {
        return _tokenSession.IsAuthenticated()
            ? RedirectToAction("Index", "Dashboard")
            : RedirectToAction("Login", "Account");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        var exceptionFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
        var exception = exceptionFeature?.Error;
        var model = BuildErrorViewModel(exception, exceptionFeature?.Path);

        Response.StatusCode = model.StatusCode;
        return View(model);
    }

    private ErrorViewModel BuildErrorViewModel(Exception? exception, string? path)
    {
        var model = new ErrorViewModel
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
        };

        if (exception is ApiException apiEx)
        {
            model.StatusCode = apiEx.StatusCode is >= 400 and <= 599 ? apiEx.StatusCode : 502;
            model.ErrorCode = $"API-{model.StatusCode}";

            model.Title = model.StatusCode switch
            {
                401 => "Session expired",
                403 => "Access denied",
                404 => "Not found",
                408 or 504 => "Request timed out",
                >= 500 => "Service unavailable",
                _ => "Request failed",
            };

            model.Message = apiEx.Message;
        }
        else if (exception != null)
        {
            model.StatusCode = 500;
            model.ErrorCode = exception.GetType().Name;

            model.Title = exception switch
            {
                InvalidOperationException => "Unable to complete this action",
                UnauthorizedAccessException => "Access denied",
                TimeoutException => "Request timed out",
                _ => "Something went wrong",
            };

            model.Message = _environment.IsDevelopment()
                ? GetFriendlyMessage(exception)
                : "We couldn't complete your request right now. Please try again, or return to the dashboard and retry from there.";
        }

        if (_environment.IsDevelopment() && exception != null)
        {
            model.ShowTechnicalDetails = true;
            model.TechnicalDetails = exception.Message;
            if (!string.IsNullOrWhiteSpace(path))
            {
                model.TechnicalDetails += $"\nPath: {path}";
            }
        }

        if (_tokenSession.IsAuthenticated())
        {
            model.ReturnUrl = Url.Action("Index", "Dashboard");
            model.ReturnLabel = "Go to Dashboard";
        }
        else
        {
            model.ReturnUrl = Url.Action("Login", "Account");
            model.ReturnLabel = "Back to Sign In";
        }

        return model;
    }

    private static string GetFriendlyMessage(Exception exception) =>
        exception switch
        {
            InvalidOperationException ioe when ioe.Message.Contains("JSON property name", StringComparison.OrdinalIgnoreCase)
                => "A data mapping error occurred while loading information from the server. The page could not be displayed.",
            InvalidOperationException => exception.Message,
            _ => "An unexpected error occurred while processing your request.",
        };
}
