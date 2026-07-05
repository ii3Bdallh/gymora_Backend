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

                  options.AddPolicy("Ip_5Limit_1Min", context => // Login
                      RateLimitPartition.GetFixedWindowLimiter(
                          partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                          factory: _ => new FixedWindowRateLimiterOptions
                          {
                              PermitLimit = 5,
                              Window = TimeSpan.FromMinutes(1),
                              QueueLimit = 0
                          }));

                  options.AddPolicy("Ip_3Limit_5Min", context => // OTP sensitive endpoints
                      RateLimitPartition.GetFixedWindowLimiter(
                          partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                          factory: _ => new FixedWindowRateLimiterOptions
                          {
                              PermitLimit = 3,
                              Window = TimeSpan.FromMinutes(5),
                              QueueLimit = 0
                          }));

                  options.AddPolicy("Ip_10Limit_1Min", context => // OTP verify endpoints
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