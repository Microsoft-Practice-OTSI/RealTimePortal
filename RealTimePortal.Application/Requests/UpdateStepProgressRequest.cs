namespace RealTimePortal.Application.Requests;

public class UpdateStepProgressRequest
{
    public int ProgressPercentage { get; set; }

    public string? Message { get; set; }
}