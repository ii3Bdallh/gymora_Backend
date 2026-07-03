using Application.DTO;
using Application.Interface.Service;
using Application.Interface.Service.Shared;
using Application.Service.Shared;
using Domain.Model.Json;
using Infrastructure.Cache;
using Infrastructure.Data;
using Infrastructure.Hangfire;
using Infrastructure.Options;
using Infrastructure.Service;
using Infrastructure.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;
using System.Reflection;

namespace Infrastructure.DependencyInjection;

public static class InfrastructureDependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<Infrastructure.Options.JwtOptions>(
            configuration.GetSection(Infrastructure.Options.JwtOptions.SectionName));

        services.Configure<RedisOptions>(
            configuration.GetSection(RedisOptions.SectionName));

        services.Configure<StorageOptions>(
            configuration.GetSection(StorageOptions.SectionName));

        services.Configure<BunnyOptions>(
            configuration.GetSection(BunnyOptions.SectionName));

        services.Configure<GoogleDriveOptions>(
            configuration.GetSection(GoogleDriveOptions.SectionName));

        services.Configure<MailOptions>(
            configuration.GetSection(MailOptions.SectionName));

        services.Configure<FirebaseOptions>(
            configuration.GetSection(FirebaseOptions.SectionName));

        services.Configure<HangfireOptions>(
            configuration.GetSection(HangfireOptions.SectionName));

        BunnyConfig legacyBunnyOptions = configuration.GetSection("BunnyConfig").Get<BunnyConfig>() ?? new BunnyConfig();
        BunnyDefultVideos legacyBunnyDefultVideos = configuration.GetSection("BunnyDefultVideos").Get<BunnyDefultVideos>() ?? new BunnyDefultVideos();
        Domain.Model.Json.JwtOptions legacyJwtOptions = configuration.GetSection("JWT").Get<Domain.Model.Json.JwtOptions>() ?? new Domain.Model.Json.JwtOptions();
        EmailOptions legacyEmailOptions = configuration.GetSection("Email").Get<EmailOptions>() ?? new EmailOptions();
        PaymobConfig legacyPaymobConfig = configuration.GetSection("Paymob").Get<PaymobConfig>() ?? new PaymobConfig();

        services.AddSingleton(legacyBunnyOptions);
        services.AddSingleton(legacyBunnyDefultVideos);
        services.AddSingleton(legacyJwtOptions);
        services.AddSingleton(legacyEmailOptions);
        services.AddSingleton(legacyPaymobConfig);

        services.AddHangfireConfiguration(configuration);

        var retryPolicy = HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

        services.AddHttpClient<IBunnyStorageService, BunnyStorageService>()
            .AddPolicyHandler(retryPolicy);
        services.AddHttpClient<IBunnyCollectionService, BunnyCollectionService>()
            .AddPolicyHandler(retryPolicy);
        services.AddHttpClient<IBunnyStreamService, BunnyStreamService>()
            .AddPolicyHandler(retryPolicy);
        services.AddHttpClient<IPaymobService, PaymobService>()
            .AddPolicyHandler(retryPolicy);

        services.AddSingleton<QueryCache>();

        services.AddScoped<JwtProvider>();

        services.AddScoped<TokenCleanupJob>();

        services.AddAutoMapper(fg => { }, Assembly.GetAssembly(typeof(MapperConfig)));

        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
        });

        services.AddInfrastructureRepositories();

        return services;
    }
}
