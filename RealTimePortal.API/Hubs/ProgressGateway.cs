using Microsoft.AspNetCore.SignalR;
using RealTimePortal.Application;
using RealTimePortal.Application.Requests;

namespace RealTimePortal.API.Hubs;

public class ProgressGateway : IProgressGateway
{
    private readonly IHubContext<RealTimeHub> _hubContext;
    private readonly ILogger<ProgressGateway> _logger;

    public ProgressGateway(
        IHubContext<RealTimeHub> hubContext,
        ILogger<ProgressGateway> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task SendProgressUpdateAsync(
        ProgressUpdateRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Sending progress update. ExecutionId: {ExecutionId}, Step: {Step}, Status: {Status}, Percentage: {Percentage}",
            request.ExecutionId,
            request.Step,
            request.Status,
            request.Percentage);

        await _hubContext.Clients
            .Group(request.ExecutionId)
            .SendAsync(
                "ReceiveProgressUpdate",
                request,
                cancellationToken);

        _logger.LogInformation(
            "Progress update sent to SignalR group: {ExecutionId}",
            request.ExecutionId);
    }

    public async Task SendProgressCompletedAsync(
        ProgressCompletedRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Sending progress completed. ExecutionId: {ExecutionId}",
            request.ExecutionId);

        await _hubContext.Clients
            .Group(request.ExecutionId)
            .SendAsync(
                "ReceiveProgressCompleted",
                request,
                cancellationToken);

        _logger.LogInformation(
            "Progress completed sent to SignalR group: {ExecutionId}",
            request.ExecutionId);
    }
}