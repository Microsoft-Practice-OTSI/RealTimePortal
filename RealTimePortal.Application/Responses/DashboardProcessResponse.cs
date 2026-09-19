namespace RealTimePortal.Application.Responses;

public class DashboardProcessResponse
{
    public long Id { get; set; }
    public string ClientId { get; set; } = string.Empty;
    public string ApplicationId { get; set; } = string.Empty;
    public string ProcessType { get; set; } = string.Empty;
    public string ReferenceNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; }
}