using Application.Cache;
using Application.Model;
using Domain.Enum;
using Infrastructure.Cache;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace IntegrationTests.Helpers;

public static class InMemoryDbContextFactory
{
    public static ApplicationDbContext Create(string dbName)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;

        var context = new ApplicationDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    public static Mock<ILogger<T>> Logger<T>() => new();

    public static CurrentUser DefaultCurrentUser(int userId = 1, string? platformRole = null)
    {
        return new CurrentUser
        {
            UserId = userId,
            PlatformRole = platformRole,
            IsAuthenticated = true
        };
    }

    public static CurrentUser SuperAdminCurrentUser(int userId = 1)
    {
        return new CurrentUser
        {
            UserId = userId,
            PlatformRole = AppRole.SuperAdmin,
            IsAuthenticated = true
        };
    }
}
