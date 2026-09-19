using RealTimePortal.Application.Responses;

namespace RealTimePortal.Application;

public interface IUserRepository
{
    Task<List<ChatUserResponse>> GetChatUsersAsync(
        long currentUserId,
        CancellationToken cancellationToken = default);
}