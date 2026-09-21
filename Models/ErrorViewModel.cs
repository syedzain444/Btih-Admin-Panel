namespace HospitalAdminPanel.Models;

public class ErrorViewModel
{
    public string? RequestId { get; set; }
    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

    public string Title { get; set; } = "Something went wrong";
    public string Message { get; set; } = "An unexpected error occurred while processing your request. Please try again or contact support if the problem persists.";

    public int StatusCode { get; set; } = 500;
    public string? ErrorCode { get; set; }

    public bool ShowTechnicalDetails { get; set; }
    public string? TechnicalDetails { get; set; }

    public string? ReturnUrl { get; set; }
    public string ReturnLabel { get; set; } = "Go to Dashboard";
}
