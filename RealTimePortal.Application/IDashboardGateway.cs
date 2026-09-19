namespace RealTimePortal.Application;

public interface IDashboardGateway
{
    Task ProcessStatusChangedAsync(
        long processId,
        string processType,
        string status,
        CancellationToken cancellationToken = default);
}