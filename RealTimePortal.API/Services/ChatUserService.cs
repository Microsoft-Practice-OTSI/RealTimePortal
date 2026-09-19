using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RealTimePortal.API.Models;

namespace RealTimePortal.API.Services;

public class ChatUserService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public ChatUserService(
        UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<List<ChatUserResponse>> GetUsersAsync(
        long currentUserId,
        CancellationToken cancellationToken = default)
    {
        return await _userManager.Users
            .AsNoTracking()
            .Where(x => x.Id != currentUserId)
            .OrderBy(x => x.UserName)
            .Select(x => new ChatUserResponse
            {
                Id = x.Id,
                UserName = x.UserName!,
                DisplayName =
                    string.IsNullOrWhiteSpace(x.FirstName) &&
                    string.IsNullOrWhiteSpace(x.LastName)
                        ? x.UserName
                        : (x.FirstName + " " + x.LastName).Trim()
            })
            .ToListAsync(cancellationToken);
    }
}