namespace RealTimePortal.Application.Requests;

public class ProgressCompletedRequest
{
    public string ExecutionId { get; set; } = string.Empty;
    public string? Message { get; set; }
    public long? ResultId { get; set; }
}