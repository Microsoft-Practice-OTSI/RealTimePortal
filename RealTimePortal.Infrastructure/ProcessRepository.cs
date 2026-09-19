using Microsoft.EntityFrameworkCore;
using RealTimePortal.Application;
using RealTimePortal.Application.Responses;
using RealTimePortal.Domain;

namespace RealTimePortal.Infrastructure;

public class ProcessRepository : IProcessRepository
{
    private readonly RealTimePortalDbContext _dbContext;

    public ProcessRepository(
        RealTimePortalDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(
        Process process,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.Processes.AddAsync(
            process,
            cancellationToken);

        await _dbContext.SaveChangesAsync(
            cancellationToken);
    }

    public async Task<Process?> GetByIdAsync(
        long processId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Processes
            .Include(x => x.Steps)
            .FirstOrDefaultAsync(
                x => x.Id == processId,
                cancellationToken);
    }

    public async Task UpdateAsync(
        Process process,
        CancellationToken cancellationToken = default)
    {
        _dbContext.Processes.Update(process);

        await _dbContext.SaveChangesAsync(
            cancellationToken);
    }

    public async Task<List<Process>> GetAllAsync(
    CancellationToken cancellationToken = default)
    {
        return await _dbContext.Processes
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Process>> GetRecentAsync(
        DateTime? fromDate,
        DateTime? toDate,
        int count,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Processes
            .AsNoTracking()
            .AsQueryable();

        if (fromDate.HasValue)
            query = query.Where(x => x.CreatedAt >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(x => x.CreatedAt < toDate.Value);

        return await query
            .OrderByDescending(x => x.CreatedAt)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Process>> GetRunningAsync(
      DateTime? fromDate,
      DateTime? toDate,
      CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Processes
            .AsNoTracking()
            .Where(x => x.Status == ProcessStatus.Running);

        if (fromDate.HasValue)
            query = query.Where(x => x.CreatedAt >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(x => x.CreatedAt < toDate.Value);

        return await query
            .OrderByDescending(x => x.StartedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Process>> GetFailedAsync(
     DateTime? fromDate,
     DateTime? toDate,
     int count,
     CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Processes
            .AsNoTracking()
            .Where(x => x.Status == ProcessStatus.Failed);

        if (fromDate.HasValue)
            query = query.Where(x => x.CreatedAt >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(x => x.CreatedAt < toDate.Value);

        return await query
            .OrderByDescending(x => x.CompletedAt)
            .Take(count)
            .ToListAsync(cancellationToken);
    }
    public async Task<Process?> GetByExecutionIdAsync(
     string executionId,
     CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(executionId))
        {
            return null;
        }

        return await _dbContext.Processes
            .Include(x => x.Steps)
            .FirstOrDefaultAsync(
                x => x.ExecutionId == executionId,
                cancellationToken);
    }
    public async Task<DashboardSummaryResponse> GetDashboardSummaryAsync(
     DateTime? fromDate,
     DateTime? toDate,
     CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Processes
            .AsNoTracking()
            .AsQueryable();

        if (fromDate.HasValue)
            query = query.Where(x => x.CreatedAt >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(x => x.CreatedAt < toDate.Value);

        return new DashboardSummaryResponse
        {
            Total = await query.CountAsync(cancellationToken),

            Pending = await query.CountAsync(
                x => x.Status == ProcessStatus.Pending,
                cancellationToken),

            Running = await query.CountAsync(
                x => x.Status == ProcessStatus.Running,
                cancellationToken),

            Completed = await query.CountAsync(
                x => x.Status == ProcessStatus.Completed,
                cancellationToken),

            Failed = await query.CountAsync(
                x => x.Status == ProcessStatus.Failed,
                cancellationToken),

            Cancelled = await query.CountAsync(
                x => x.Status == ProcessStatus.Cancelled,
                cancellationToken)
        };
    }

    public async Task<List<ProcessTypeSummaryResponse>> GetProcessTypeSummaryAsync(
    DateTime? fromDate,
    DateTime? toDate,
    CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Processes
            .AsNoTracking()
            .AsQueryable();

        if (fromDate.HasValue)
            query = query.Where(x => x.CreatedAt >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(x => x.CreatedAt < toDate.Value);

        return await query
            .GroupBy(x => x.ProcessType)
            .Select(group => new ProcessTypeSummaryResponse
            {
                ProcessType = group.Key,
                Total = group.Count(),

                Pending = group.Count(
                    x => x.Status == ProcessStatus.Pending),

                Running = group.Count(
                    x => x.Status == ProcessStatus.Running),

                Completed = group.Count(
                    x => x.Status == ProcessStatus.Completed),

                Failed = group.Count(
                    x => x.Status == ProcessStatus.Failed),

                Cancelled = group.Count(
                    x => x.Status == ProcessStatus.Cancelled)
            })
            .OrderBy(x => x.ProcessType)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<DashboardTrendResponse>> GetDashboardTrendAsync(
     DateTime fromDate,
     DateTime toDate,
     CancellationToken cancellationToken = default)
    {
        return await _dbContext.Processes
            .AsNoTracking()
            .Where(x =>
                x.CreatedAt >= fromDate &&
                x.CreatedAt < toDate)
            .GroupBy(x => new
            {
                Date = x.CreatedAt.Date,
                x.ProcessType
            })
            .Select(group => new DashboardTrendResponse
            {
                Date = group.Key.Date,
                ProcessType = group.Key.ProcessType,
                Created = group.Count(),
                Completed = group.Count(
                    x => x.Status == ProcessStatus.Completed),
                Failed = group.Count(
                    x => x.Status == ProcessStatus.Failed)
            })
            .OrderBy(x => x.Date)
            .ThenBy(x => x.ProcessType)
            .ToListAsync(cancellationToken);
    }
}