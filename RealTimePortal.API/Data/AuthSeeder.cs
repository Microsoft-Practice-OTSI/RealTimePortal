using Microsoft.AspNetCore.Identity;
using RealTimePortal.API.Models;

namespace RealTimePortal.API.Data;

public static class AuthSeeder
{
    public static async Task SeedAsync(
        IServiceProvider services)
    {
        var userManager =
            services.GetRequiredService<
                UserManager<ApplicationUser>>();

        var roleManager =
            services.GetRequiredService<
                RoleManager<IdentityRole<long>>>();

        const string adminRoleName = "Admin";
        const string userRoleName = "User";

        const string adminUserName = "admin";
        const string adminPassword = "Admin@123";

        // =========================================================
        // Create Admin Role
        // =========================================================

        if (!await roleManager.RoleExistsAsync(adminRoleName))
        {
            var adminRole = new IdentityRole<long>
            {
                Name = adminRoleName
            };

            var roleResult =
                await roleManager.CreateAsync(adminRole);

            if (!roleResult.Succeeded)
            {
                throw new Exception(
                    string.Join(
                        "; ",
                        roleResult.Errors
                            .Select(x => x.Description)));
            }
        }

        // =========================================================
        // Create User Role
        // =========================================================

        if (!await roleManager.RoleExistsAsync(userRoleName))
        {
            var userRole = new IdentityRole<long>
            {
                Name = userRoleName
            };

            var roleResult =
                await roleManager.CreateAsync(userRole);

            if (!roleResult.Succeeded)
            {
                throw new Exception(
                    string.Join(
                        "; ",
                        roleResult.Errors
                            .Select(x => x.Description)));
            }
        }

        // =========================================================
        // Create Admin User
        // =========================================================

        var adminUser =
            await userManager.FindByNameAsync(adminUserName);

        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminUserName,
                Email = "admin@realtimeportal.com",
                FirstName = "System",
                LastName = "Administrator",
                EmailConfirmed = true
            };

            var userResult =
                await userManager.CreateAsync(
                    adminUser,
                    adminPassword);

            if (!userResult.Succeeded)
            {
                throw new Exception(
                    string.Join(
                        "; ",
                        userResult.Errors
                            .Select(x => x.Description)));
            }
        }

        // =========================================================
        // Assign Admin Role
        // =========================================================

        if (!await userManager.IsInRoleAsync(
                adminUser,
                adminRoleName))
        {
            var roleResult =
                await userManager.AddToRoleAsync(
                    adminUser,
                    adminRoleName);

            if (!roleResult.Succeeded)
            {
                throw new Exception(
                    string.Join(
                        "; ",
                        roleResult.Errors
                            .Select(x => x.Description)));
            }
        }
    }
}