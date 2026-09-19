using Microsoft.AspNetCore.SignalR;
using RealTimePortal.Application;
using RealTimePortal.Application.Responses;

namespace RealTimePortal.API.Hubs;

public class ChatGateway : IChatGateway
{
    private readonly IHubContext<RealTimeHub> _hubContext;

    public ChatGateway(IHubContext<RealTimeHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public Task MessageReceivedAsync(
        long conversationId,
        ChatMessageResponse message,
        CancellationToken cancellationToken = default)
    {
        return _hubContext.Clients
            .Group($"conversation-{conversationId}")
            .SendAsync(
                "chatMessageReceived",
                message,
                cancellationToken);
    }


    public async Task UnreadMessageReceivedAsync(
    long userId,
    ChatMessageResponse message,
    CancellationToken cancellationToken = default)
    {
        await _hubContext.Clients
            .Group($"user-{userId}")
            .SendAsync(
                "chatUnreadMessageReceived",
                message,
                cancellationToken);
    }
}