using RealTimePortal.Application.Responses;
using RealTimePortal.Domain;

namespace RealTimePortal.Application;

public interface IProcessRepository
{
    Task AddAsync(
        Process process,
        CancellationToken cancellationToken = default);

    Task<Process?> GetByIdAsync(
        long processId,
        CancellationToken cancellationToken = default);

    Task<Process?> GetByExecutionIdAsync(
        string executionId,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        Process process,
        CancellationToken cancellationToken = default);

    Task<List<Process>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<DashboardSummaryResponse> GetDashboardSummaryAsync(
        DateTime? fromDate,
        DateTime? toDate,
        CancellationToken cancellationToken = default);

    Task<List<ProcessTypeSummaryResponse>> GetProcessTypeSummaryAsync(
        DateTime? fromDate,
        DateTime? toDate,
        CancellationToken cancellationToken = default);

    Task<List<Process>> GetRecentAsync(
        DateTime? fromDate,
        DateTime? toDate,
        int count,
        CancellationToken cancellationToken = default);

    Task<List<Process>> GetRunningAsync(
        DateTime? fromDate,
        DateTime? toDate,
        CancellationToken cancellationToken = default);

    Task<List<Process>> GetFailedAsync(
        DateTime? fromDate,
        DateTime? toDate,
        int count,
        CancellationToken cancellationToken = default);

    Task<List<DashboardTrendResponse>> GetDashboardTrendAsync(
        DateTime fromDate,
        DateTime toDate,
        CancellationToken cancellationToken = default);
}