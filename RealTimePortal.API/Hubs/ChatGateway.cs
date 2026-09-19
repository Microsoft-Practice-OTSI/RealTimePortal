using Microsoft.AspNetCore.SignalR;
using RealTimePortal.Application;

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
        object message,
        CancellationToken cancellationToken = default)
    {
        return _hubContext.Clients
            .Group($"conversation-{conversationId}")
            .SendAsync(
                "chatMessageReceived",
                message,
                cancellationToken);
    }
}