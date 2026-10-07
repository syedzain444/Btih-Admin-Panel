using HospitalAdminPanel.Helpers;
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

        context.Result = new RedirectToActionResult(
            "Login",
            "Account",
            new { returnUrl = context.HttpContext.Request.Path });
    }
}

/// <summary>
/// Exact single-role gate (legacy). Prefer <see cref="AdminPermissionAttribute"/> for modules.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class AdminRoleAttribute : Attribute, IAuthorizationFilter
{
    private readonly HashSet<string> _roles;

    public AdminRoleAttribute(params string[] roles)
    {
        _roles = new HashSet<string>(
            roles.Where(r => !string.IsNullOrWhiteSpace(r)).Select(r => r.Trim()),
            StringComparer.OrdinalIgnoreCase);
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var tokenService = context.HttpContext.RequestServices.GetRequiredService<ITokenSessionService>();
        var user = tokenService.GetUser();
        if (user == null)
        {
            context.Result = new RedirectToActionResult("Login", "Account", null);
            return;
        }

        if (_roles.Count > 0 &&
            !_roles.Contains(AdminRoles.Normalize(user.Role)))
        {
            context.Result = new RedirectToActionResult("Forbidden", "Account", null);
        }
    }
}

/// <summary>Module-based RBAC using <see cref="AdminModulePermissions"/>.</summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class AdminPermissionAttribute : Attribute, IAuthorizationFilter
{
    private readonly string _module;

    public AdminPermissionAttribute(string module) => _module = module;

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var tokenService = context.HttpContext.RequestServices.GetRequiredService<ITokenSessionService>();
        var user = tokenService.GetUser();
        if (user == null)
        {
            context.Result = new RedirectToActionResult("Login", "Account", null);
            return;
        }

        if (!AdminModulePermissions.CanAccess(user.Role, _module, user.Permissions))
        {
            context.Result = new RedirectToActionResult("Forbidden", "Account", new { module = _module });
        }
    }
}
