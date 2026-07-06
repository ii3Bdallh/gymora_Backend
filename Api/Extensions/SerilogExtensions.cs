using Serilog;

namespace Api.Extensions;

public static class SerilogExtensions
{
    public static WebApplicationBuilder AddSerilogLogging(
        this WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog((context, services, configuration) =>
        {
            configuration
                .ReadFrom.Configuration(context.Configuration)
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithThreadId()
                .Enrich.WithEnvironmentName();
        });

        return builder;
    }
}
