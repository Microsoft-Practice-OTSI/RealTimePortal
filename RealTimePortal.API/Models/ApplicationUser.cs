using Microsoft.AspNetCore.Identity;

namespace RealTimePortal.API.Models;

public class ApplicationUser : IdentityUser<long>
{
    public string? FirstName { get; set; }

    public string? LastName { get; set; }
}