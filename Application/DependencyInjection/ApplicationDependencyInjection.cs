using Microsoft.Extensions.DependencyInjection;

namespace Application.DependencyInjection;

public static class ApplicationDependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        services.AddApplicationServices();

        return services;
    }
}
