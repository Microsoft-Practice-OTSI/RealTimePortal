using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RealTimePortal.API.Data;
using RealTimePortal.API.Models;
using RealTimePortal.API.Services;
using Serilog;
using Serilog.Events;

namespace RealTimePortal.API.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddApiServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString =
            configuration.GetConnectionString(
                "RealTimePortalDatabase")
            ?? throw new InvalidOperationException(
                "RealTimePortalDatabase connection string is not configured.");

        // --------------------------------------------------
        // Authentication database
        // --------------------------------------------------

        services.AddDbContext<AuthDbContext>(options =>
        {
            options.UseSqlServer(
                connectionString,
                sql =>
                {
                    sql.MigrationsHistoryTable(
                        "__AuthMigrationsHistory");
                });
        });

        // --------------------------------------------------
        // ASP.NET Core Identity
        // --------------------------------------------------

        services
            .AddIdentityCore<ApplicationUser>(options =>
            {
                options.User.RequireUniqueEmail = true;

                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = true;

                options.Lockout.MaxFailedAccessAttempts = 5;

                options.Lockout.DefaultLockoutTimeSpan =
                    TimeSpan.FromMinutes(15);
            })
            .AddRoles<IdentityRole<long>>()
            .AddEntityFrameworkStores<AuthDbContext>()
            .AddSignInManager();

        // --------------------------------------------------
        // JWT Authentication
        // --------------------------------------------------

        var jwtSecretKey =
            configuration["Jwt:SecretKey"]
            ?? throw new InvalidOperationException(
                "JWT SecretKey is not configured.");

        var jwtIssuer =
            configuration["Jwt:Issuer"]
            ?? throw new InvalidOperationException(
                "JWT Issuer is not configured.");

        var jwtAudience =
            configuration["Jwt:Audience"]
            ?? throw new InvalidOperationException(
                "JWT Audience is not configured.");

        services
            .AddAuthentication(
                JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters =
                    new TokenValidationParameters
                    {
                        ValidateIssuer = true,

                        ValidateAudience = true,

                        ValidateLifetime = true,

                        ValidateIssuerSigningKey = true,

                        ValidIssuer = jwtIssuer,

                        ValidAudience = jwtAudience,

                        IssuerSigningKey =
                            new SymmetricSecurityKey(
                                Encoding.UTF8.GetBytes(
                                    jwtSecretKey)),

                        ClockSkew =
                            TimeSpan.FromMinutes(1)
                    };

                // SignalR JWT support
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken =
                            context.Request.Query[
                                "access_token"];

                        var path =
                            context.HttpContext.Request.Path;

                        if (!string.IsNullOrEmpty(accessToken) &&
                            path.StartsWithSegments(
                                "/hubs/realtime"))
                        {
                            context.Token = accessToken;
                        }

                        return Task.CompletedTask;
                    }
                };
            });

        services.AddAuthorization();

        // --------------------------------------------------
        // JWT service
        // --------------------------------------------------

        services.AddScoped<JwtService>();

        // --------------------------------------------------
        // CORS
        // --------------------------------------------------

        services.AddCors(options =>
        {
            options.AddPolicy("DefaultPolicy", policy =>
            {
                policy
                    .WithOrigins("http://localhost:4200")
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });

        return services;
    }

    public static IHostBuilder AddApiLogging(
        this IHostBuilder hostBuilder)
    {
        hostBuilder.UseSerilog(
            (context, services, logger) =>
            {
                logger
                    .MinimumLevel.Information()
                    .MinimumLevel.Override(
                        "Microsoft",
                        LogEventLevel.Warning)
                    .MinimumLevel.Override(
                        "Microsoft.AspNetCore",
                        LogEventLevel.Warning)
                    .Enrich.FromLogContext()
                    .Enrich.WithMachineName()
                    .WriteTo.Console()
                    .WriteTo.File(
                        "Logs/realtimeportal-.log",
                        rollingInterval:
                            RollingInterval.Day,
                        retainedFileCountLimit: 30,
                        rollOnFileSizeLimit: true);
            });

        return hostBuilder;
    }
}