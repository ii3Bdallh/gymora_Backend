using System.Net;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.DependencyInjection;

public static class RateLimitExtensions
{
    public static IServiceCollection AddRateLimiting(this IServiceCollection services)
    {
        
        services.AddRateLimiter(options =>
              {
                  options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

                  options.AddPolicy("login", context =>
                      RateLimitPartition.GetFixedWindowLimiter(
                          partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                          factory: _ => new FixedWindowRateLimiterOptions
                          {
                              PermitLimit = 5,
                              Window = TimeSpan.FromMinutes(1),
                              QueueLimit = 0
                          }));

                  options.AddPolicy("otp-sensitive", context =>
                      RateLimitPartition.GetFixedWindowLimiter(
                          partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                          factory: _ => new FixedWindowRateLimiterOptions
                          {
                              PermitLimit = 3,
                              Window = TimeSpan.FromMinutes(5),
                              QueueLimit = 0
                          }));

                  options.AddPolicy("otp-verify", context =>
                      RateLimitPartition.GetFixedWindowLimiter(
                          partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                          factory: _ => new FixedWindowRateLimiterOptions
                          {
                              PermitLimit = 10,
                              Window = TimeSpan.FromMinutes(1),
                              QueueLimit = 0
                          }));

                  options.OnRejected = async (context, cancellationToken) =>
                  {
                      context.HttpContext.Response.ContentType = "application/json";
                      await context.HttpContext.Response.WriteAsync(
                          """{"success":false,"error":{"code":"RATE_LIMIT_EXCEEDED","message":"Too many requests. Please try again later."}}""",
                          cancellationToken);
                  };
              });

        return services;
    }
}