using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Application.DependencyInjection;

public static class ApplicationDependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services , IConfiguration configuration)
    {
        services.AddApplicationServices(configuration);

        return services;
    }
}
    