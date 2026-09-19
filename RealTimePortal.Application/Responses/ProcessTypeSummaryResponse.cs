namespace RealTimePortal.Application.Responses;

public class ProcessTypeSummaryResponse
{
    public string ProcessType { get; set; } = string.Empty;
    public int Total { get; set; }
    public int Pending { get; set; }
    public int Running { get; set; }
    public int Completed { get; set; }
    public int Failed { get; set; }
    public int Cancelled { get; set; }
}