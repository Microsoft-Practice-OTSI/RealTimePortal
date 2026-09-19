using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealTimePortal.API.Services;
using RealTimePortal.Application;
using RealTimePortal.Application.Requests;
using System.Security.Claims;

namespace RealTimePortal.API.Controllers;



[ApiController]
[Authorize]
[Route("api/chat")]
public class ChatController : ControllerBase
{
    private readonly ChatService _chatService;
    private readonly ChatUserService _chatUserService;

    public ChatController(
     ChatService chatService,
     ChatUserService chatUserService)
    {
        _chatService = chatService;
        _chatUserService = chatUserService;
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetUsers(
    CancellationToken cancellationToken)
    {
        var userId = GetUserId();

        var users = await _chatUserService.GetUsersAsync(
            userId,
            cancellationToken);

        return Ok(users);
    }

    [HttpPost("conversations")]
    public async Task<IActionResult> CreateConversation(
        [FromBody] CreateConversationRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();

        var result = await _chatService.CreateConversationAsync(
            userId,
            request,
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("conversations")]
    public async Task<IActionResult> GetConversations(
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();

        var result = await _chatService.GetConversationsAsync(
            userId,
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("conversations/{conversationId:long}/messages")]
    public async Task<IActionResult> GetMessages(
        long conversationId,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();

        var result = await _chatService.GetMessagesAsync(
            conversationId,
            userId,
            cancellationToken);

        return Ok(result);
    }

    [HttpPost("conversations/{conversationId:long}/messages")]
    public async Task<IActionResult> SendMessage(
        long conversationId,
        [FromBody] SendChatMessageRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();

        var result = await _chatService.SendMessageAsync(
            conversationId,
            userId,
            request,
            cancellationToken);

        return Ok(result);
    }

    [HttpPost("conversations/{conversationId:long}/read")]
    public async Task<IActionResult> MarkAsRead(
        long conversationId,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();

        await _chatService.MarkAsReadAsync(
            conversationId,
            userId,
            cancellationToken);

        return Ok();
    }

    private long GetUserId()
    {
        var claim = User.FindFirst(
            ClaimTypes.NameIdentifier);

        if (claim == null ||
            !long.TryParse(claim.Value, out var userId))
        {
            throw new UnauthorizedAccessException(
                "Invalid user.");
        }

        return userId;
    }
}