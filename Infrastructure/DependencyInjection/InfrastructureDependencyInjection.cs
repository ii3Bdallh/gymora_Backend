using Application.DTO;
using Application.Interface.Service;
using Application.Interface.Service.Shared;
using Application.Service.Shared;
using Domain.Model.Auth;
using Domain.Options;
using Infrastructure.Cache;
using Infrastructure.Hangfire;
using Infrastructure.Persistence;
using Infrastructure.Seed;
using Infrastructure.Service;
using Infrastructure.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
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
        services.Configure<JwtOptions>(
            configuration.GetSection(JwtOptions.SectionName));
        services.AddSingleton(sp =>
sp.GetRequiredService<IOptions<JwtOptions>>().Value);

        services.Configure<RedisOptions>(
            configuration.GetSection(RedisOptions.SectionName));
        services.AddSingleton(sp =>
sp.GetRequiredService<IOptions<RedisOptions>>().Value);

        services.Configure<StorageOptions>(
            configuration.GetSection(StorageOptions.SectionName));
        services.AddSingleton(sp =>
sp.GetRequiredService<IOptions<StorageOptions>>().Value);

        services.Configure<BunnyOptions>(
            configuration.GetSection(BunnyOptions.SectionName));
        services.AddSingleton(sp =>
sp.GetRequiredService<IOptions<BunnyOptions>>().Value);

        services.Configure<GoogleDriveOptions>(
            configuration.GetSection(GoogleDriveOptions.SectionName));
        services.AddSingleton(sp =>
sp.GetRequiredService<IOptions<GoogleDriveOptions>>().Value);

        services.Configure<MailOptions>(
            configuration.GetSection(MailOptions.SectionName));
        services.AddSingleton(sp =>
sp.GetRequiredService<IOptions<MailOptions>>().Value);

        services.Configure<FirebaseOptions>(
            configuration.GetSection(FirebaseOptions.SectionName));
        services.AddSingleton(sp =>
sp.GetRequiredService<IOptions<FirebaseOptions>>().Value);

        services.Configure<HangfireOptions>(
            configuration.GetSection(HangfireOptions.SectionName));
        services.AddSingleton(sp =>
sp.GetRequiredService<IOptions<HangfireOptions>>().Value);






        var jwtOptions = configuration
            .GetSection(JwtOptions.SectionName)
            .Get<JwtOptions>()
            ?? throw new InvalidOperationException("Jwt configuration is missing.");


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
        // services.AddHttpClient<IPaymobService, PaymobService>()
        //     .AddPolicyHandler(retryPolicy);

        services.AddSingleton<QueryCache>();

        services.AddScoped<JwtProvider>();

        services.AddScoped<TokenCleanupJob>();

        services.AddAutoMapper(fg => { }, Assembly.GetAssembly(typeof(MapperConfig)));

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sql =>
                {
                    sql.MigrationsAssembly(
                        typeof(ApplicationDbContext).Assembly.FullName);

                    sql.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null);
                });
        });

        services.AddInfrastructureRepositories();

        services
            .AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                options.User.RequireUniqueEmail = true;

                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = false;

                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);

                options.SignIn.RequireConfirmedEmail = false;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();


        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

        }).AddJwtBearer(options =>
        {
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters()
            {
                ValidateIssuer = true,
                ValidIssuer = jwtOptions.Issuer,
                ValidateAudience = true,
                ValidAudience = jwtOptions.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(jwtOptions.SecretKey ?? ""))
            };
        });

        services.AddScoped<IdentitySeeder>();

        services.AddHttpContextAccessor();

        services.AddScoped<ICurrentUserService, CurrentUserService>();

        services.AddScoped<ICurrentGymService, CurrentGymService>();


        return services;
    }
}
