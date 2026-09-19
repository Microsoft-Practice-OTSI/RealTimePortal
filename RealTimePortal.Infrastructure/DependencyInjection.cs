using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RealTimePortal.Application;

namespace RealTimePortal.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<RealTimePortalDbContext>(options =>
        {
            options.UseSqlServer(
                configuration.GetConnectionString(
                    "RealTimePortalDatabase"));
        });
        services.AddScoped<IProcessRepository, ProcessRepository>();    
        return services;
    }
}