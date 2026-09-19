namespace RealTimePortal.Application.Requests;

public class DashboardFilterRequest
{
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}