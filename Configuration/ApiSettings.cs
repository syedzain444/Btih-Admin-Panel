namespace HospitalAdminPanel.Configuration;

public class ApiSettings
{
    public const string SectionName = "ApiSettings";
    public string BaseUrl { get; set; } = "http://localhost:8080";
    public int TimeoutSeconds { get; set; } = 60;
}

public class AdminPanelSettings
{
    public const string SectionName = "AdminPanel";
    public string ApplicationName { get; set; } = "BTIH Admin Portal";
    public string HospitalName { get; set; } = "Bahria Town International Hospital";
}
