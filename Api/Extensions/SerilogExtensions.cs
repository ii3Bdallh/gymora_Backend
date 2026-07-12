using Serilog;

namespace Api.Extensions;

public static class SerilogExtensions
{
    public static WebApplicationBuilder AddSerilogLogging(
        this WebApplicationBuilder builder)
    {
        // builder.Host.UseSerilog((context, services, configuration) =>
        // {
        //     configuration
        //         .ReadFrom.Configuration(context.Configuration)
        //         .Enrich.FromLogContext()
        //         .Enrich.WithMachineName()
        //         .Enrich.WithThreadId()
        //         .Enrich.WithEnvironmentName();
        // });

        builder.Host.UseSerilog((ctx, lc) => lc
            .ReadFrom.Configuration(ctx.Configuration)
            .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
            .WriteTo.File("logs/log-.txt",
                rollingInterval: RollingInterval.Day,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
            .Filter.ByExcluding(log =>
                log.Properties.TryGetValue("RequestPath", out var path) &&
                path.ToString().Contains("/health", StringComparison.OrdinalIgnoreCase))
        );

        return builder;
    }
}
