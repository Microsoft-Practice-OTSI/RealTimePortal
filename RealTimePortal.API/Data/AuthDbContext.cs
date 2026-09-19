using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RealTimePortal.API.Models;

namespace RealTimePortal.API.Data;

public class AuthDbContext
    : IdentityDbContext<ApplicationUser, IdentityRole<long>, long>
{
    public AuthDbContext(
        DbContextOptions<AuthDbContext> options)
        : base(options)
    {
    }
}