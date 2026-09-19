using RealTimePortal.Application.Responses;
using RealTimePortal.Domain;

namespace RealTimePortal.Application;

public class DashboardService
{
    private readonly IProcessRepository _processRepository;

    public DashboardService(IProcessRepository processRepository)
    {
        _processRepository = processRepository;
    }


    public async Task<DashboardSummaryResponse> GetSummaryAsync(
   DashboardPeriod period,
   CancellationToken cancellationToken = default)
    {
        var (fromDate, toDate) = GetDateRange(period);

        return await _processRepository.GetDashboardSummaryAsync(
            fromDate,
            toDate,
            cancellationToken);
    }

    public async Task<List<ProcessTypeSummaryResponse>> GetProcessTypeSummaryAsync(
    DashboardPeriod period,
    CancellationToken cancellationToken = default)
    {
        var (fromDate, toDate) = GetDateRange(period);

        return await _processRepository.GetProcessTypeSummaryAsync(
            fromDate,
            toDate,
            cancellationToken);
    }

    public async Task<List<DashboardProcessResponse>> GetRecentProcessesAsync(
    DashboardPeriod period,
    int count,
    CancellationToken cancellationToken = default)
    {
        count = NormalizeCount(count);

        var (fromDate, toDate) = GetDateRange(period);

        var processes = await _processRepository.GetRecentAsync(
            fromDate,
            toDate,
            count,
            cancellationToken);

        return processes.Select(MapProcess).ToList();
    }

    public async Task<List<DashboardProcessResponse>> GetRunningProcessesAsync(
     DashboardPeriod period,
     CancellationToken cancellationToken = default)
    {
        var (fromDate, toDate) = GetDateRange(period);

        var processes = await _processRepository.GetRunningAsync(
            fromDate,
            toDate,
            cancellationToken);

        return processes.Select(MapProcess).ToList();
    }

    public async Task<List<DashboardProcessResponse>> GetFailedProcessesAsync(
     DashboardPeriod period,
     int count,
     CancellationToken cancellationToken = default)
    {
        count = NormalizeCount(count);

        var (fromDate, toDate) = GetDateRange(period);

        var processes = await _processRepository.GetFailedAsync(
            fromDate,
            toDate,
            count,
            cancellationToken);

        return processes.Select(MapProcess).ToList();
    }

 
    private static DashboardProcessResponse MapProcess(Process process)
    {
        return new DashboardProcessResponse
        {
            Id = process.Id,
            ClientId = process.ClientId,
            ApplicationId = process.ApplicationId,
            ProcessType = process.ProcessType,
            ReferenceNumber = process.ReferenceNumber,
            Status = process.Status.ToString(),
            StartedAt = process.StartedAt,
            CompletedAt = process.CompletedAt,
            ErrorMessage = process.ErrorMessage,
            CreatedAt = process.CreatedAt
        };
    }

    private static int NormalizeCount(int count)
    {
        if (count <= 0)
            return 10;

        return Math.Min(count, 100);
    }

    private static (DateTime? FromDate, DateTime? ToDate) GetDateRange(
    DashboardPeriod period)
    {
        var today = DateTime.UtcNow.Date;

        return period switch
        {
            DashboardPeriod.Today =>
                (today, today.AddDays(1)),

            DashboardPeriod.Last7Days =>
                (today.AddDays(-6), today.AddDays(1)),

            DashboardPeriod.Last30Days =>
                (today.AddDays(-29), today.AddDays(1)),

            _ =>
                (null, null)
        };
    }

    public async Task<List<DashboardTrendResponse>> GetTrendAsync(
     DashboardPeriod period,
     CancellationToken cancellationToken = default)
    {
        var (fromDate, toDate) = GetDateRange(period);

        if (!fromDate.HasValue || !toDate.HasValue)
        {
            var today = DateTime.UtcNow.Date;

            fromDate = today.AddDays(-29);
            toDate = today.AddDays(1);
        }

        var trend = await _processRepository.GetDashboardTrendAsync(
            fromDate.Value,
            toDate.Value,
            cancellationToken);

        if (trend.Count == 0)
        {
            return trend;
        }

        var processTypes = trend
            .Select(x => x.ProcessType)
            .Distinct()
            .OrderBy(x => x)
            .ToList();

        var result = new List<DashboardTrendResponse>();

        for (var date = fromDate.Value.Date;
             date < toDate.Value.Date;
             date = date.AddDays(1))
        {
            foreach (var processType in processTypes)
            {
                var existing = trend.FirstOrDefault(x =>
                    x.Date.Date == date &&
                    x.ProcessType == processType);

                result.Add(existing ?? new DashboardTrendResponse
                {
                    Date = date,
                    ProcessType = processType,
                    Created = 0,
                    Completed = 0,
                    Failed = 0
                });
            }
        }

        return result;
    }
}