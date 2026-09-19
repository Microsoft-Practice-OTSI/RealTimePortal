using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using RealTimePortal.Application;

namespace RealTimePortal.API.Hubs;

[Authorize]
public class RealTimeHub : Hub
{
    private readonly ChatService _chatService;

    public RealTimeHub(ChatService chatService)
    {
        _chatService = chatService;
    }

    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
    }

    public async Task JoinProcess(long processId)
    {
        await Groups.AddToGroupAsync(
            Context.ConnectionId,
            $"process-{processId}");
    }

    public async Task LeaveProcess(long processId)
    {
        await Groups.RemoveFromGroupAsync(
            Context.ConnectionId,
            $"process-{processId}");
    }
    public async Task JoinDashboard()
    {
        await Groups.AddToGroupAsync(
            Context.ConnectionId,
            "dashboard");
    }

    public async Task LeaveDashboard()
    {
        await Groups.RemoveFromGroupAsync(
            Context.ConnectionId,
            "dashboard");
    }

    // NEW

    public async Task JoinExecutionGroup(string executionId)
    {
        if (string.IsNullOrWhiteSpace(executionId))
            throw new HubException("ExecutionId is required.");

        await Groups.AddToGroupAsync(
            Context.ConnectionId,
            executionId);

        Console.WriteLine(
            $"Connection {Context.ConnectionId} joined execution group {executionId}");
    }

    public async Task LeaveExecutionGroup(
        string executionId)
    {
        if (string.IsNullOrWhiteSpace(executionId))
        {
            return;
        }

        await Groups.RemoveFromGroupAsync(
            Context.ConnectionId,
            executionId);
    }

    public async Task JoinConversation(long conversationId)
    {
        var userId = GetUserId();

        await _chatService.EnsureParticipantAsync(
            conversationId,
            userId);

        await Groups.AddToGroupAsync(
            Context.ConnectionId,
            $"conversation-{conversationId}");
    }

    public async Task LeaveConversation(long conversationId)
    {
        await Groups.RemoveFromGroupAsync(
            Context.ConnectionId,
            $"conversation-{conversationId}");
    }

    private long GetUserId()
    {
        var claim = Context.User?.FindFirst(
            System.Security.Claims.ClaimTypes.NameIdentifier);

        if (claim == null ||
            !long.TryParse(claim.Value, out var userId))
        {
            throw new HubException("Invalid user.");
        }

        return userId;
    }
}