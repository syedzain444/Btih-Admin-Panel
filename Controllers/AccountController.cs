using HospitalAdminPanel.Configuration;
using HospitalAdminPanel.Factories;
using HospitalAdminPanel.Models.Api;
using HospitalAdminPanel.Models.ViewModels;
using HospitalAdminPanel.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace HospitalAdminPanel.Controllers;

public class AccountController : Controller
{
    private readonly IApiFactory _apiFactory;
    private readonly ITokenSessionService _tokenSession;
    private readonly ApiSettings _apiSettings;
    private readonly IWebHostEnvironment _environment;

    public AccountController(
        IApiFactory apiFactory,
        ITokenSessionService tokenSession,
        IOptions<ApiSettings> apiSettings,
        IWebHostEnvironment environment)
    {
        _apiFactory = apiFactory;
        _tokenSession = tokenSession;
        _apiSettings = apiSettings.Value;
        _environment = environment;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (_tokenSession.IsAuthenticated())
        {
            return RedirectToAction("Index", "Dashboard");
        }

        ViewData["ReturnUrl"] = returnUrl;
        ViewData["ApiBaseUrl"] = _apiSettings.BaseUrl;
        ViewData["ShowDevHints"] = _environment.IsDevelopment();
        return View(new LoginViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(model.Username) || string.IsNullOrWhiteSpace(model.Password))
        {
            model.ErrorMessage = "Username and password are required.";
            return View(model);
        }

        try
        {
            var response = await _apiFactory.Auth.LoginAsync(new AdminLoginRequest
            {
                Username = model.Username.Trim(),
                Password = model.Password,
            }, cancellationToken);

            if (response?.Success != true || string.IsNullOrWhiteSpace(response.Token) || response.User == null)
            {
                model.ErrorMessage = response?.Message ?? "Invalid username or password.";
                return View(model);
            }

            var expiresAt = response.ExpiresAt ?? DateTime.UtcNow.AddHours(8);
            _tokenSession.SetSession(response.Token, response.User, expiresAt);

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Dashboard");
        }
        catch (ApiException ex)
        {
            model.ErrorMessage = ex.StatusCode switch
            {
                401 => ex.Message.Contains("Invalid", StringComparison.OrdinalIgnoreCase)
                    ? "Invalid username or password. If this is your first login, ask IT to run admin bootstrap on the API."
                    : ex.Message,
                >= 500 => "Unable to reach the hospital API. Please try again later.",
                _ => ex.Message,
            };
            ViewData["ApiBaseUrl"] = _apiSettings.BaseUrl;
            ViewData["ShowDevHints"] = _environment.IsDevelopment();
            return View(model);
        }
        catch (HttpRequestException)
        {
            model.ErrorMessage = $"Cannot connect to the API at {_apiSettings.BaseUrl}. Start HospitalMobileAPPApi2 first (http://localhost:8080).";
            ViewData["ApiBaseUrl"] = _apiSettings.BaseUrl;
            ViewData["ShowDevHints"] = _environment.IsDevelopment();
            return View(model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        _tokenSession.Clear();
        return RedirectToAction(nameof(Login));
    }
}
