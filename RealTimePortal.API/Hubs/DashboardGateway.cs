using Microsoft.AspNetCore.SignalR;
using RealTimePortal.Application;

namespace RealTimePortal.API.Hubs;

public class DashboardGateway : IDashboardGateway
{
    private readonly IHubContext<RealTimeHub> _hubContext;

    public DashboardGateway(IHubContext<RealTimeHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public Task ProcessStatusChangedAsync(
        long processId,
        string processType,
        string status,
        CancellationToken cancellationToken = default)
    {
        return _hubContext.Clients
            .Group("dashboard")
            .SendAsync(
                "dashboardProcessStatusChanged",
                new
                {
                    ProcessId = processId,
                    ProcessType = processType,
                    Status = status
                },
                cancellationToken);
    }
}