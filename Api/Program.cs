using Api.Extensions;
using Api.Middalewares;
using Api.Middlewares;
using Application.DependencyInjection;
using Domain.Model.Json;
using Hangfire;
using Infrastructure.DependencyInjection;
using Infrastructure.Hangfire;
using Infrastructure.Seed;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

#region Services

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration)
    .AddApi();

Domain.Model.Json.JwtOptions jwtOptions = builder.Configuration
    .GetSection("JWT").Get<Domain.Model.Json.JwtOptions>() ?? new Domain.Model.Json.JwtOptions();

if (string.IsNullOrWhiteSpace(jwtOptions?.SecretKey) || jwtOptions.SecretKey.Length < 32)
    throw new InvalidOperationException("JWT SecretKey is not configured or too short (min 32 characters). Set it in appsettings.json or environment variables.");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

}).AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuer = true,
        ValidIssuer = jwtOptions.issuer,
        ValidateAudience = true,
        ValidAudience = jwtOptions.audience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(jwtOptions.SecretKey ?? ""))
    };
});

#endregion

var app = builder.Build();

#region Seeder

using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<IdentitySeeder>();
    await seeder.InitializeAsync();
}

#endregion

#region Middleware Pipeline

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHangfireDashboard("/hangfire");
RecurringJobs.Register();

app.UseCors("FrontendClient");
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseStaticFiles();
app.UseRouting();
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseMiddleware<CurrentUserMiddleware>();
app.UseAuthorization();
app.UseRateLimiter();

app.MapControllers();
app.MapHealthChecks("/healthz");

app.Run();

#endregion
