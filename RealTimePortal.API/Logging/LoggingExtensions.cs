using Serilog;
using Serilog.Events;

namespace RealTimePortal.API.Logging;

public static class LoggingExtensions
{
    public static IHostBuilder UseRealTimePortalLogging(
        this IHostBuilder hostBuilder,
        IConfiguration configuration)
    {
        hostBuilder.UseSerilog((context, services, loggerConfiguration) =>
        {
            loggerConfiguration
                .MinimumLevel.Information()

                .MinimumLevel.Override(
                    "Microsoft",
                    LogEventLevel.Warning)

                .MinimumLevel.Override(
                    "Microsoft.AspNetCore",
                    LogEventLevel.Warning)

                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithThreadId()

                .WriteTo.Console()

                .WriteTo.File(
                    path: "Logs/realtimeportal-.log",
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 30,
                    fileSizeLimitBytes: 10_000_000,
                    rollOnFileSizeLimit: true,
                    shared: true,
                    outputTemplate:
                        "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} " +
                        "[{Level:u3}] " +
                        "{SourceContext} " +
                        "{Message:lj}" +
                        "{NewLine}{Exception}");
        });

        return hostBuilder;
    }
}