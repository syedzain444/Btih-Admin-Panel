using HospitalAdminPanel.Factories;
using HospitalAdminPanel.Helpers;
using HospitalAdminPanel.Middleware;
using HospitalAdminPanel.Models.ViewModels;
using HospitalAdminPanel.Services;
using Microsoft.AspNetCore.Mvc;

namespace HospitalAdminPanel.Controllers;

[AdminAuthorize]
[AdminPermission(AdminModules.AccessControl)]
public class AccessControlController : Controller
{
    private readonly IApiFactory _apiFactory;

    public AccessControlController(IApiFactory apiFactory) => _apiFactory = apiFactory;

    [HttpGet]
    public async Task<IActionResult> Users(CancellationToken cancellationToken)
    {
        try
        {
            var users = await _apiFactory.Rbac.GetUsersAsync(cancellationToken);
            var roles = await _apiFactory.Rbac.GetRolesAsync(true, cancellationToken);
            return View(new AccessUsersViewModel { Users = users, Roles = roles });
        }
        catch (ApiException ex)
        {
            TempData["Error"] = ex.Message;
            return View(new AccessUsersViewModel());
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Could not load admin users: {ex.Message}";
            return View(new AccessUsersViewModel());
        }
    }

    [HttpGet]
    public async Task<IActionResult> CreateUser(CancellationToken cancellationToken)
    {
        var roles = await SafeRoles(cancellationToken);
        return View("UserForm", new AccessUserFormViewModel
        {
            IsActive = true,
            Role = roles.FirstOrDefault(r => r.Code == "Staff")?.Code ?? roles.FirstOrDefault()?.Code ?? "Staff",
            Roles = roles,
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateUser(AccessUserFormViewModel model, CancellationToken cancellationToken)
    {
        model.Roles = await SafeRoles(cancellationToken);
        if (string.IsNullOrWhiteSpace(model.Username) || string.IsNullOrWhiteSpace(model.Password))
        {
            ModelState.AddModelError(string.Empty, "Username and password are required.");
        }

        if (!ModelState.IsValid)
        {
            return View("UserForm", model);
        }

        try
        {
            await _apiFactory.Rbac.CreateUserAsync(new
            {
                username = model.Username.Trim(),
                displayName = model.DisplayName,
                role = model.Role,
                password = model.Password,
                isActive = model.IsActive,
            }, cancellationToken);
            TempData["Success"] = "Admin user created.";
            return RedirectToAction(nameof(Users));
        }
        catch (ApiException ex)
        {
            TempData["Error"] = ex.Message;
            return View("UserForm", model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> EditUser(int id, CancellationToken cancellationToken)
    {
        try
        {
            var users = await _apiFactory.Rbac.GetUsersAsync(cancellationToken);
            var item = users.FirstOrDefault(u => u.AdminId == id);
            if (item == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction(nameof(Users));
            }

            return View("UserForm", new AccessUserFormViewModel
            {
                AdminId = item.AdminId,
                Username = item.Username,
                DisplayName = item.DisplayName,
                Role = item.Role,
                IsActive = item.IsActive,
                Roles = await SafeRoles(cancellationToken),
            });
        }
        catch (ApiException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Users));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditUser(int id, AccessUserFormViewModel model, CancellationToken cancellationToken)
    {
        model.AdminId = id;
        model.Roles = await SafeRoles(cancellationToken);
        try
        {
            await _apiFactory.Rbac.UpdateUserAsync(id, new
            {
                displayName = model.DisplayName,
                role = model.Role,
                isActive = model.IsActive,
            }, cancellationToken);

            if (!string.IsNullOrWhiteSpace(model.Password))
            {
                await _apiFactory.Rbac.ResetPasswordAsync(id, model.Password, cancellationToken);
            }

            TempData["Success"] = "Admin user updated.";
            return RedirectToAction(nameof(Users));
        }
        catch (ApiException ex)
        {
            TempData["Error"] = ex.Message;
            return View("UserForm", model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Roles(CancellationToken cancellationToken)
    {
        try
        {
            var roles = await _apiFactory.Rbac.GetRolesAsync(true, cancellationToken);
            var permissions = await _apiFactory.Rbac.GetPermissionsAsync(cancellationToken);
            return View(new AccessRolesViewModel { Roles = roles, Permissions = permissions });
        }
        catch (ApiException ex)
        {
            TempData["Error"] = ex.Message;
            return View(new AccessRolesViewModel());
        }
    }

    [HttpGet]
    public async Task<IActionResult> CreateRole(CancellationToken cancellationToken)
    {
        return View("RoleForm", new AccessRoleFormViewModel
        {
            IsActive = true,
            AllPermissions = await SafePermissions(cancellationToken),
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateRole(AccessRoleFormViewModel model, CancellationToken cancellationToken)
    {
        model.AllPermissions = await SafePermissions(cancellationToken);
        model.SelectedPermissions ??= new List<string>();
        if (string.IsNullOrWhiteSpace(model.Code) || string.IsNullOrWhiteSpace(model.Name))
        {
            ModelState.AddModelError(string.Empty, "Code and name are required.");
        }

        if (!ModelState.IsValid)
        {
            return View("RoleForm", model);
        }

        try
        {
            await _apiFactory.Rbac.CreateRoleAsync(new
            {
                code = model.Code.Trim(),
                name = model.Name.Trim(),
                description = model.Description,
                isActive = model.IsActive,
                permissions = model.SelectedPermissions,
            }, cancellationToken);
            TempData["Success"] = "Role created.";
            return RedirectToAction(nameof(Roles));
        }
        catch (ApiException ex)
        {
            TempData["Error"] = ex.Message;
            return View("RoleForm", model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> EditRole(int id, CancellationToken cancellationToken)
    {
        try
        {
            var role = await _apiFactory.Rbac.GetRoleAsync(id, cancellationToken);
            if (role == null)
            {
                TempData["Error"] = "Role not found.";
                return RedirectToAction(nameof(Roles));
            }

            return View("RoleForm", new AccessRoleFormViewModel
            {
                RoleId = role.RoleId,
                Code = role.Code,
                Name = role.Name,
                Description = role.Description,
                IsActive = role.IsActive,
                IsSystem = role.IsSystem,
                SelectedPermissions = role.Permissions ?? new List<string>(),
                AllPermissions = await SafePermissions(cancellationToken),
            });
        }
        catch (ApiException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Roles));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditRole(int id, AccessRoleFormViewModel model, CancellationToken cancellationToken)
    {
        model.RoleId = id;
        model.AllPermissions = await SafePermissions(cancellationToken);
        model.SelectedPermissions ??= new List<string>();
        try
        {
            await _apiFactory.Rbac.UpdateRoleAsync(id, new
            {
                name = model.Name.Trim(),
                description = model.Description,
                isActive = model.IsActive,
            }, cancellationToken);
            await _apiFactory.Rbac.SetRolePermissionsAsync(id, model.SelectedPermissions, cancellationToken);
            TempData["Success"] = "Role and permissions updated.";
            return RedirectToAction(nameof(Roles));
        }
        catch (ApiException ex)
        {
            TempData["Error"] = ex.Message;
            return View("RoleForm", model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteRole(int id, CancellationToken cancellationToken)
    {
        try
        {
            await _apiFactory.Rbac.DeleteRoleAsync(id, cancellationToken);
            TempData["Success"] = "Role deleted.";
        }
        catch (ApiException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Roles));
    }

    private async Task<List<Models.Api.AdminRoleApiModel>> SafeRoles(CancellationToken cancellationToken)
    {
        try { return await _apiFactory.Rbac.GetRolesAsync(false, cancellationToken); }
        catch { return new List<Models.Api.AdminRoleApiModel>(); }
    }

    private async Task<List<Models.Api.AdminPermissionApiModel>> SafePermissions(CancellationToken cancellationToken)
    {
        try { return await _apiFactory.Rbac.GetPermissionsAsync(cancellationToken); }
        catch { return new List<Models.Api.AdminPermissionApiModel>(); }
    }
}
