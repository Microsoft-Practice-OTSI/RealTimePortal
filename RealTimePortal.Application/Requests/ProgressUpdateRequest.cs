namespace RealTimePortal.Application.Requests;

public class ProgressUpdateRequest
{
    public string ExecutionId { get; set; } = string.Empty;

    public string Step { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public int Percentage { get; set; }

    public string? Message { get; set; }
}