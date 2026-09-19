namespace RealTimePortal.Application.Responses;

public class DashboardTrendResponse
{
    public DateTime Date { get; set; }

    public string ProcessType { get; set; } = string.Empty;

    public int Created { get; set; }

    public int Completed { get; set; }

    public int Failed { get; set; }
}