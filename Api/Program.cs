using Api.Extensions;
using Api.Middalewares;
using Application.DependencyInjection;
using Application.EventHandlers;
using Domain.Events;
using Domain.Options;
using Hangfire;
using Infrastructure.DependencyInjection;
using Infrastructure.Hangfire;
using Infrastructure.Seed;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// builder.AddSerilogLogging();


#region Services

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration)
    .AddApi();

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(
        typeof(NotificationHandler).Assembly);
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
// app.UseMiddleware<CurrentUserMiddleware>();
app.UseAuthorization();
app.UseRateLimiter();

app.MapControllers();
app.MapHealthChecks("/healthz");

app.Run();

#endregion
