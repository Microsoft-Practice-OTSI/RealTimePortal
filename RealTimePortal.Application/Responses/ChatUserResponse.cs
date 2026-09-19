namespace RealTimePortal.Application.Responses;

public class ChatUserResponse
{
    public long Id { get; set; }

    public string UserName { get; set; } = string.Empty;

    public string? DisplayName { get; set; }
}