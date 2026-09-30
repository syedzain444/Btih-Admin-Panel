namespace HospitalAdminPanel.Helpers;

/// <summary>
/// Admin panel RBAC matrix (REQ-2026-025 / TC-025).
/// Keep in sync with API policies in HospitalMobileAPPApi.Helpers.AuthorizationPolicies.
/// </summary>
public static class AdminRoles
{
    public const string Admin = "Admin";
    public const string Staff = "Staff";
    public const string Reception = "Reception";

    public static readonly string[] All = [Admin, Staff, Reception];

    public static string Normalize(string? role)
    {
        if (string.IsNullOrWhiteSpace(role)) return Admin;
        foreach (var known in All)
        {
            if (string.Equals(known, role.Trim(), StringComparison.OrdinalIgnoreCase))
                return known;
        }

        return Admin;
    }
}

public static class AdminModules
{
    public const string Dashboard = "Dashboard";
    public const string Messages = "Messages";
    public const string Appointments = "Appointments";
    public const string Users = "Users";
    public const string Reports = "Reports";
    public const string Analytics = "Analytics";
    public const string Refills = "Refills";
    public const string Tickets = "Tickets";
    public const string Promotions = "Promotions";
    public const string SupportContent = "SupportContent";
    public const string Audit = "Audit";
}

public static class AdminModulePermissions
{
    private static readonly IReadOnlyDictionary<string, string[]> Matrix =
        new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            // Front desk + operations overview
            [AdminModules.Dashboard] = [AdminRoles.Admin, AdminRoles.Staff, AdminRoles.Reception],
            [AdminModules.Messages] = [AdminRoles.Admin, AdminRoles.Staff, AdminRoles.Reception],
            [AdminModules.Appointments] = [AdminRoles.Admin, AdminRoles.Staff, AdminRoles.Reception],

            // Restricted (TC-025): Reception must not access Users / Reports
            [AdminModules.Users] = [AdminRoles.Admin],
            [AdminModules.Reports] = [AdminRoles.Admin],
            [AdminModules.Analytics] = [AdminRoles.Admin],
            [AdminModules.Audit] = [AdminRoles.Admin],

            // Operations — Staff + Admin
            [AdminModules.Refills] = [AdminRoles.Admin, AdminRoles.Staff],
            [AdminModules.Tickets] = [AdminRoles.Admin, AdminRoles.Staff],
            [AdminModules.Promotions] = [AdminRoles.Admin, AdminRoles.Staff],
            [AdminModules.SupportContent] = [AdminRoles.Admin, AdminRoles.Staff],
        };

    public static bool CanAccess(string? role, string module)
    {
        if (string.IsNullOrWhiteSpace(module)) return false;
        if (!Matrix.TryGetValue(module, out var allowed)) return false;
        var normalized = AdminRoles.Normalize(role);
        return allowed.Any(r => string.Equals(r, normalized, StringComparison.OrdinalIgnoreCase));
    }

    public static IReadOnlyList<string> ModulesFor(string? role)
    {
        var normalized = AdminRoles.Normalize(role);
        return Matrix
            .Where(kv => kv.Value.Any(r => string.Equals(r, normalized, StringComparison.OrdinalIgnoreCase)))
            .Select(kv => kv.Key)
            .ToList();
    }
}
