using HospitalAdminPanel.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace HospitalAdminPanel.Middleware;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AdminAuthorizeAttribute : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var tokenService = context.HttpContext.RequestServices.GetRequiredService<ITokenSessionService>();
        if (tokenService.IsAuthenticated())
        {
            return;
        }

        context.Result = new RedirectToActionResult("Login", "Account", new { returnUrl = context.HttpContext.Request.Path });
    }
}

public class AdminRoleAttribute : Attribute, IAuthorizationFilter
{
    private readonly string _role;

    public AdminRoleAttribute(string role) => _role = role;

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var tokenService = context.HttpContext.RequestServices.GetRequiredService<ITokenSessionService>();
        var user = tokenService.GetUser();
        if (user == null)
        {
            context.Result = new RedirectToActionResult("Login", "Account", null);
            return;
        }

        if (!string.Equals(user.Role, _role, StringComparison.OrdinalIgnoreCase))
        {
            context.Result = new ForbidResult();
        }
    }
}
