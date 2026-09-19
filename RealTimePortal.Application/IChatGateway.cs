namespace RealTimePortal.Application;

public interface IChatGateway
{
    Task MessageReceivedAsync(
        long conversationId,
        object message,
        CancellationToken cancellationToken = default);
}