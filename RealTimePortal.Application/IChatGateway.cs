using RealTimePortal.Application.Responses;

namespace RealTimePortal.Application;

public interface IChatGateway
{
    Task MessageReceivedAsync(
        long conversationId,
        ChatMessageResponse message,
        CancellationToken cancellationToken = default);

    Task UnreadMessageReceivedAsync(
        long userId,
        ChatMessageResponse message,
        CancellationToken cancellationToken = default);
}