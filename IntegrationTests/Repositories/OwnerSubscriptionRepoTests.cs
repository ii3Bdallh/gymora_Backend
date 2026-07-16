using Domain.Enum;
using Domain.Model;
using FluentAssertions;
using Infrastructure.Repo;
using IntegrationTests.Helpers;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace IntegrationTests.Repositories;

public class OwnerSubscriptionRepoTests : IDisposable
{
    private readonly Infrastructure.Persistence.ApplicationDbContext _context;
    private readonly OwnerSubscriptionRepo _sut;
    private readonly string _dbName;

    public OwnerSubscriptionRepoTests()
    {
        _dbName = Guid.NewGuid().ToString();
        _context = InMemoryDbContextFactory.Create(_dbName);
        var currentUser = InMemoryDbContextFactory.SuperAdminCurrentUser();
        _sut = new OwnerSubscriptionRepo(
            _context,
            InMemoryDbContextFactory.Logger<Infrastructure.Repo.OwnerSubscriptionRepo>().Object,
            new Infrastructure.Cache.QueryCache(),
            currentUser);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    private async Task<(PlanPrice planPrice, SubscriptionPlan plan)> SeedPlanAsync()
    {
        var plan = new SubscriptionPlan
        {
            Name = "Basic", IsFree = false,
            MaxOwnedGyms = 1, MaxCoachesGym = 5, MaxMembersGym = 50,
            IsActive = true, CreatedOn = DateTime.UtcNow
        };
        _context.SubscriptionPlan.Add(plan);
        await _context.SaveChangesAsync();

        var planPrice = new PlanPrice
        {
            PlanId = plan.Id, CountryCode = "US", CurrencyCode = "USD",
            DurationMonths = 1, Amount = 50m, CreatedOn = DateTime.UtcNow,
            IsActive = true
        };
        _context.PlanPrice.Add(planPrice);
        await _context.SaveChangesAsync();

        return (planPrice, plan);
    }

    #region AddAsync

    [Fact]
    public async Task AddAsync_ShouldPersistEntity_WhenDataIsValid()
    {
        var (planPrice, plan) = await SeedPlanAsync();

        var subscription = new OwnerSubscription
        {
            PlanId = plan.Id,
            PlanPriceId = planPrice.Id,
            AmountPaid = 50m,
            CurrencyCode = "USD",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddMonths(1),
            GraceEndDate = DateTime.UtcNow.AddMonths(1).AddDays(7),
            CreatedById = 1,
            CreatedOn = DateTime.UtcNow,
            IsActive = true,
            RowVersion = [1, 2, 3]
        };

        var result = await _sut.AddAsync(subscription);
        await _context.SaveChangesAsync();

        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
    }

    #endregion

    #region HasActiveSubscriptionAsync

    [Fact]
    public async Task HasActiveSubscriptionAsync_ShouldReturnTrue_WhenActiveSubscriptionExists()
    {
        var (planPrice, plan) = await SeedPlanAsync();

        var subscription = new OwnerSubscription
        {
            PlanId = plan.Id,
            PlanPriceId = planPrice.Id,
            AmountPaid = 50m,
            CurrencyCode = "USD",
            StartDate = DateTime.UtcNow.AddDays(-1),
            EndDate = DateTime.UtcNow.AddMonths(1),
            GraceEndDate = DateTime.UtcNow.AddMonths(1).AddDays(7),
            CreatedById = 1,
            CreatedOn = DateTime.UtcNow,
            IsActive = true,
            RowVersion = [1, 2, 3]
        };
        _context.OwnerSubscription.Add(subscription);
        await _context.SaveChangesAsync();

        var result = await _sut.HasActiveSubscriptionAsync(1);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasActiveSubscriptionAsync_ShouldReturnFalse_WhenNoActiveSubscription()
    {
        var (planPrice, plan) = await SeedPlanAsync();

        var subscription = new OwnerSubscription
        {
            PlanId = plan.Id,
            PlanPriceId = planPrice.Id,
            AmountPaid = 50m,
            CurrencyCode = "USD",
            StartDate = DateTime.UtcNow.AddMonths(-2),
            EndDate = DateTime.UtcNow.AddMonths(-1),
            GraceEndDate = DateTime.UtcNow.AddMonths(-1).AddDays(7),
            CreatedById = 1,
            CreatedOn = DateTime.UtcNow,
            IsActive = true,
            RowVersion = [1, 2, 3]
        };
        _context.OwnerSubscription.Add(subscription);
        await _context.SaveChangesAsync();

        var result = await _sut.HasActiveSubscriptionAsync(1);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task HasActiveSubscriptionAsync_ShouldReturnFalse_WhenSubscriptionBelongsToDifferentUser()
    {
        var (planPrice, plan) = await SeedPlanAsync();

        var subscription = new OwnerSubscription
        {
            PlanId = plan.Id,
            PlanPriceId = planPrice.Id,
            AmountPaid = 50m,
            CurrencyCode = "USD",
            StartDate = DateTime.UtcNow.AddDays(-1),
            EndDate = DateTime.UtcNow.AddMonths(1),
            GraceEndDate = DateTime.UtcNow.AddMonths(1).AddDays(7),
            CreatedById = 999,
            CreatedOn = DateTime.UtcNow,
            IsActive = true,
            RowVersion = [1, 2, 3]
        };
        _context.OwnerSubscription.Add(subscription);
        await _context.SaveChangesAsync();

        var result = await _sut.HasActiveSubscriptionAsync(1);

        result.Should().BeFalse();
    }

    #endregion

    #region HasGraceSubscriptionAsync

    [Fact]
    public async Task HasGraceSubscriptionAsync_ShouldReturnTrue_WhenInGracePeriod()
    {
        var (planPrice, plan) = await SeedPlanAsync();

        var subscription = new OwnerSubscription
        {
            PlanId = plan.Id,
            PlanPriceId = planPrice.Id,
            AmountPaid = 50m,
            CurrencyCode = "USD",
            StartDate = DateTime.UtcNow.AddMonths(-2),
            EndDate = DateTime.UtcNow.AddDays(-1),
            GraceEndDate = DateTime.UtcNow.AddDays(5),
            CreatedById = 1,
            CreatedOn = DateTime.UtcNow,
            IsActive = true,
            RowVersion = [1, 2, 3]
        };
        _context.OwnerSubscription.Add(subscription);
        await _context.SaveChangesAsync();

        var result = await _sut.HasGraceSubscriptionAsync(1);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasGraceSubscriptionAsync_ShouldReturnFalse_WhenNotInGracePeriod()
    {
        var (planPrice, plan) = await SeedPlanAsync();

        var subscription = new OwnerSubscription
        {
            PlanId = plan.Id,
            PlanPriceId = planPrice.Id,
            AmountPaid = 50m,
            CurrencyCode = "USD",
            StartDate = DateTime.UtcNow.AddMonths(-3),
            EndDate = DateTime.UtcNow.AddMonths(-2),
            GraceEndDate = DateTime.UtcNow.AddMonths(-2).AddDays(7),
            CreatedById = 1,
            CreatedOn = DateTime.UtcNow,
            IsActive = true,
            RowVersion = [1, 2, 3]
        };
        _context.OwnerSubscription.Add(subscription);
        await _context.SaveChangesAsync();

        var result = await _sut.HasGraceSubscriptionAsync(1);

        result.Should().BeFalse();
    }

    #endregion

    #region GetCurrentSubscriptionAsync

    [Fact]
    public async Task GetCurrentSubscriptionAsync_ShouldReturnSubscription_WhenActive()
    {
        var (planPrice, plan) = await SeedPlanAsync();

        var subscription = new OwnerSubscription
        {
            PlanId = plan.Id,
            PlanPriceId = planPrice.Id,
            AmountPaid = 50m,
            CurrencyCode = "USD",
            StartDate = DateTime.UtcNow.AddDays(-1),
            EndDate = DateTime.UtcNow.AddMonths(1),
            GraceEndDate = DateTime.UtcNow.AddMonths(1).AddDays(7),
            CreatedById = 1,
            CreatedOn = DateTime.UtcNow,
            IsActive = true,
            RowVersion = [1, 2, 3]
        };
        _context.OwnerSubscription.Add(subscription);
        await _context.SaveChangesAsync();

        var result = await _sut.GetCurrentSubscriptionAsync(1);

        result.Should().NotBeNull();
        result!.PlanId.Should().Be(plan.Id);
    }

    [Fact]
    public async Task GetCurrentSubscriptionAsync_ShouldReturnNull_WhenNoActiveSubscription()
    {
        var result = await _sut.GetCurrentSubscriptionAsync(999);

        result.Should().BeNull();
    }

    #endregion

    #region GetByIdAsync

    [Fact]
    public async Task GetByIdAsync_ShouldReturnEntity_WhenExists()
    {
        var (planPrice, plan) = await SeedPlanAsync();

        var subscription = new OwnerSubscription
        {
            PlanId = plan.Id,
            PlanPriceId = planPrice.Id,
            AmountPaid = 50m,
            CurrencyCode = "USD",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddMonths(1),
            GraceEndDate = DateTime.UtcNow.AddMonths(1).AddDays(7),
            CreatedById = 1,
            CreatedOn = DateTime.UtcNow,
            IsActive = true,
            RowVersion = [1, 2, 3]
        };
        _context.OwnerSubscription.Add(subscription);
        await _context.SaveChangesAsync();

        var result = await _sut.GetByIdAsync(subscription.Id);

        result.Should().NotBeNull();
        result!.CurrencyCode.Should().Be("USD");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenEntityDoesNotExist()
    {
        var result = await _sut.GetByIdAsync(999);

        result.Should().BeNull();
    }

    #endregion

    #region DeleteAsync (Soft Delete)

    [Fact]
    public async Task DeleteAsync_ShouldSoftDelete()
    {
        var (planPrice, plan) = await SeedPlanAsync();

        var subscription = new OwnerSubscription
        {
            PlanId = plan.Id,
            PlanPriceId = planPrice.Id,
            AmountPaid = 50m,
            CurrencyCode = "USD",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddMonths(1),
            GraceEndDate = DateTime.UtcNow.AddMonths(1).AddDays(7),
            CreatedById = 1,
            CreatedOn = DateTime.UtcNow,
            IsActive = true,
            RowVersion = [1, 2, 3]
        };
        _context.OwnerSubscription.Add(subscription);
        await _context.SaveChangesAsync();

        var result = await _sut.DeleteAsync(subscription);
        await _context.SaveChangesAsync();

        result.IsActive.Should().BeFalse();
    }

    #endregion
}
