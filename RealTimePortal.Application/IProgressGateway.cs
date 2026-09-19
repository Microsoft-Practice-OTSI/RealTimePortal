using RealTimePortal.Application.Requests;

namespace RealTimePortal.Application;

public interface IProgressGateway
{
    Task SendProgressUpdateAsync(
        ProgressUpdateRequest request,
        CancellationToken cancellationToken = default);

    Task SendProgressCompletedAsync(
        ProgressCompletedRequest request,
        CancellationToken cancellationToken = default);
}