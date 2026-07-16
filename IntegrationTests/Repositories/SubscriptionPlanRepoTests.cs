using Application.DTO.Pagintion;
using Domain.Enum;
using Domain.Model;
using FluentAssertions;
using Infrastructure.Repo.Entity;
using IntegrationTests.Helpers;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace IntegrationTests.Repositories;

public class SubscriptionPlanRepoTests : IDisposable
{
    private readonly Infrastructure.Persistence.ApplicationDbContext _context;
    private readonly SubscriptionPlanRepo _sut;
    private readonly string _dbName;

    public SubscriptionPlanRepoTests()
    {
        _dbName = Guid.NewGuid().ToString();
        _context = InMemoryDbContextFactory.Create(_dbName);
        var currentUser = InMemoryDbContextFactory.SuperAdminCurrentUser();
        _sut = new SubscriptionPlanRepo(
            _context,
            InMemoryDbContextFactory.Logger<SubscriptionPlanRepo>().Object,
            new Infrastructure.Cache.QueryCache(),
            currentUser);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    #region AddAsync

    [Fact]
    public async Task AddAsync_ShouldPersistEntity_WhenDataIsValid()
    {
        var plan = new SubscriptionPlan
        {
            Name = "Premium",
            Description = "Premium plan",
            IsFree = false,
            MaxOwnedGyms = 5,
            MaxCoachesPerGym = 20,
            MaxMembersPerGym = 200,
            IsActive = true,
            CreatedOn = DateTime.UtcNow
        };

        var result = await _sut.AddAsync(plan);
        await _context.SaveChangesAsync();

        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);

        var fromDb = await _context.SubscriptionPlan.FindAsync(result.Id);
        fromDb.Should().NotBeNull();
        fromDb!.Name.Should().Be("Premium");
    }

    #endregion

    #region GetByIdAsync

    [Fact]
    public async Task GetByIdAsync_ShouldReturnEntity_WhenExists()
    {
        var plan = new SubscriptionPlan
        {
            Name = "Basic",
            IsFree = false,
            MaxOwnedGyms = 1,
            MaxCoachesPerGym = 5,
            MaxMembersPerGym = 50,
            IsActive = true,
            CreatedOn = DateTime.UtcNow
        };
        _context.SubscriptionPlan.Add(plan);
        await _context.SaveChangesAsync();

        var result = await _sut.GetByIdAsync(plan.Id);

        result.Should().NotBeNull();
        result!.Name.Should().Be("Basic");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenEntityDoesNotExist()
    {
        var result = await _sut.GetByIdAsync(999);

        result.Should().BeNull();
    }

    #endregion

    #region GetPageAsync

    [Fact]
    public async Task GetPageAsync_ShouldIncludePrices_WhenRequested()
    {
        var plan = new SubscriptionPlan
        {
            Name = "Basic",
            IsFree = false,
            MaxOwnedGyms = 1,
            MaxCoachesPerGym = 5,
            MaxMembersPerGym = 50,
            IsActive = true,
            CreatedOn = DateTime.UtcNow
        };
        _context.SubscriptionPlan.Add(plan);
        await _context.SaveChangesAsync();

        _context.PlanPrice.AddRange(
            new PlanPrice
            {
                PlanId = plan.Id, CountryCode = "US", CurrencyCode = "USD",
                DurationMonths = 1, Amount = 50m, CreatedOn = DateTime.UtcNow,
                IsActive = true
            },
            new PlanPrice
            {
                PlanId = plan.Id, CountryCode = "US", CurrencyCode = "USD",
                DurationMonths = 12, Amount = 500m, CreatedOn = DateTime.UtcNow,
                IsActive = true
            }
        );
        await _context.SaveChangesAsync();

        var searchReq = new PaginatedSearchReq { PageNumber = 1, PageSize = 10 };

        var result = await _sut.GetPageAsync(searchReq);

        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetPageAsync_ShouldReturnPaginatedResults()
    {
        for (int i = 1; i <= 8; i++)
        {
            _context.SubscriptionPlan.Add(new SubscriptionPlan
            {
                Name = $"Plan {i}",
                IsFree = i == 1,
                MaxOwnedGyms = i,
                MaxCoachesPerGym = i * 5,
                MaxMembersPerGym = i * 50,
                IsActive = true,
                CreatedOn = DateTime.UtcNow
            });
        }
        await _context.SaveChangesAsync();

        var searchReq = new PaginatedSearchReq { PageNumber = 1, PageSize = 5 };

        var result = await _sut.GetPageAsync(searchReq);

        result.Items.Should().HaveCount(5);
        result.TotalCount.Should().Be(8);
    }

    #endregion

    #region AddPlanPriceAsync

    [Fact]
    public async Task AddPlanPriceAsync_ShouldPersistPlanPrice()
    {
        var plan = new SubscriptionPlan
        {
            Name = "Basic",
            IsFree = false,
            MaxOwnedGyms = 1,
            MaxCoachesPerGym = 5,
            MaxMembersPerGym = 50,
            IsActive = true,
            CreatedOn = DateTime.UtcNow
        };
        _context.SubscriptionPlan.Add(plan);
        await _context.SaveChangesAsync();

        var planPrice = new PlanPrice
        {
            PlanId = plan.Id,
            CountryCode = "US",
            CurrencyCode = "USD",
            DurationMonths = 1,
            Amount = 50m,
            CreatedOn = DateTime.UtcNow,
            IsActive = true
        };

        var result = await _sut.AddPlanPriceAsync(planPrice);
        await _context.SaveChangesAsync();

        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);

        var fromDb = await _context.PlanPrice.FindAsync(result.Id);
        fromDb.Should().NotBeNull();
        fromDb!.Amount.Should().Be(50m);
    }

    #endregion

    #region DeletePlanPriceAsync

    [Fact]
    public async Task DeletePlanPriceAsync_ShouldRemovePlanPrice()
    {
        var plan = new SubscriptionPlan
        {
            Name = "Basic",
            IsFree = false,
            MaxOwnedGyms = 1,
            MaxCoachesPerGym = 5,
            MaxMembersPerGym = 50,
            IsActive = true,
            CreatedOn = DateTime.UtcNow
        };
        _context.SubscriptionPlan.Add(plan);
        await _context.SaveChangesAsync();

        var planPrice = new PlanPrice
        {
            PlanId = plan.Id,
            CountryCode = "US",
            CurrencyCode = "USD",
            DurationMonths = 1,
            Amount = 50m,
            CreatedOn = DateTime.UtcNow,
            IsActive = true
        };
        _context.PlanPrice.Add(planPrice);
        await _context.SaveChangesAsync();

        var result = await _sut.DeletePlanPriceAsync(planPrice);
        await _context.SaveChangesAsync();

        result.Should().NotBeNull();

        var fromDb = await _context.PlanPrice.FindAsync(planPrice.Id);
        fromDb.Should().BeNull();
    }

    #endregion

    #region GetPlanPriceByIdAsync

    [Fact]
    public async Task GetPlanPriceByIdAsync_ShouldReturnPrice_WhenExists()
    {
        var plan = new SubscriptionPlan
        {
            Name = "Basic",
            IsFree = false,
            MaxOwnedGyms = 1,
            MaxCoachesPerGym = 5,
            MaxMembersPerGym = 50,
            IsActive = true,
            CreatedOn = DateTime.UtcNow
        };
        _context.SubscriptionPlan.Add(plan);
        await _context.SaveChangesAsync();

        var planPrice = new PlanPrice
        {
            PlanId = plan.Id,
            CountryCode = "US",
            CurrencyCode = "USD",
            DurationMonths = 1,
            Amount = 50m,
            CreatedOn = DateTime.UtcNow,
            IsActive = true
        };
        _context.PlanPrice.Add(planPrice);
        await _context.SaveChangesAsync();

        var result = await _sut.GetPlanPriceByIdAsync(planPrice.Id, true, false);

        result.Should().NotBeNull();
        result!.Amount.Should().Be(50m);
    }

    [Fact]
    public async Task GetPlanPriceByIdAsync_ShouldReturnNull_WhenNotFound()
    {
        var result = await _sut.GetPlanPriceByIdAsync(999, true, false);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetPlanPriceByIdAsync_ShouldReturnNull_WhenInactive()
    {
        var plan = new SubscriptionPlan
        {
            Name = "Basic",
            IsFree = false,
            MaxOwnedGyms = 1,
            MaxCoachesPerGym = 5,
            MaxMembersPerGym = 50,
            IsActive = true,
            CreatedOn = DateTime.UtcNow
        };
        _context.SubscriptionPlan.Add(plan);
        await _context.SaveChangesAsync();

        var planPrice = new PlanPrice
        {
            PlanId = plan.Id,
            CountryCode = "US",
            CurrencyCode = "USD",
            DurationMonths = 1,
            Amount = 50m,
            CreatedOn = DateTime.UtcNow,
            IsActive = false
        };
        _context.PlanPrice.Add(planPrice);
        await _context.SaveChangesAsync();

        var result = await _sut.GetPlanPriceByIdAsync(planPrice.Id, isActive: true);

        result.Should().BeNull();
    }

    #endregion

    #region GetFreePlanAsync

    [Fact]
    public async Task GetFreePlanAsync_ShouldReturnFreePlan_WhenExists()
    {
        var freePlan = new SubscriptionPlan
        {
            Name = "Free",
            IsFree = true,
            MaxOwnedGyms = 1,
            MaxCoachesPerGym = 2,
            MaxMembersPerGym = 10,
            IsActive = true,
            CreatedOn = DateTime.UtcNow
        };
        _context.SubscriptionPlan.Add(freePlan);
        await _context.SaveChangesAsync();

        var result = await _sut.GetFreePlanAsync();

        result.Should().NotBeNull();
        result!.IsFree.Should().BeTrue();
    }

    [Fact]
    public async Task GetFreePlanAsync_ShouldReturnNull_WhenNoFreePlanExists()
    {
        var result = await _sut.GetFreePlanAsync();

        result.Should().BeNull();
    }

    #endregion

    #region UpdateAsync

    [Fact]
    public async Task UpdateAsync_ShouldPersistChanges()
    {
        var plan = new SubscriptionPlan
        {
            Name = "Basic",
            IsFree = false,
            MaxOwnedGyms = 1,
            MaxCoachesPerGym = 5,
            MaxMembersPerGym = 50,
            IsActive = true,
            CreatedOn = DateTime.UtcNow
        };
        _context.SubscriptionPlan.Add(plan);
        await _context.SaveChangesAsync();

        _context.Entry(plan).State = EntityState.Detached;

        var tracked = await _context.SubscriptionPlan.FindAsync(plan.Id);
        tracked!.Name = "Updated Basic";

        var result = await _sut.UpdateAsync(tracked);
        await _context.SaveChangesAsync();

        var fromDb = await _context.SubscriptionPlan.FindAsync(plan.Id);
        fromDb!.Name.Should().Be("Updated Basic");
    }

    #endregion

    #region DeleteAsync (Soft Delete)

    [Fact]
    public async Task DeleteAsync_ShouldSoftDelete()
    {
        var plan = new SubscriptionPlan
        {
            Name = "Delete Me",
            IsFree = false,
            MaxOwnedGyms = 1,
            MaxCoachesPerGym = 5,
            MaxMembersPerGym = 50,
            IsActive = true,
            CreatedOn = DateTime.UtcNow
        };
        _context.SubscriptionPlan.Add(plan);
        await _context.SaveChangesAsync();

        var result = await _sut.DeleteAsync(plan);
        await _context.SaveChangesAsync();

        result.IsActive.Should().BeFalse();

        var fromDb = await _context.SubscriptionPlan.FindAsync(plan.Id);
        fromDb!.IsActive.Should().BeFalse();
    }

    #endregion

    #region HardDeleteAsync

    [Fact]
    public async Task HardDeleteAsync_ShouldRemoveEntityFromDatabase()
    {
        var plan = new SubscriptionPlan
        {
            Name = "Hard Delete",
            IsFree = false,
            MaxOwnedGyms = 1,
            MaxCoachesPerGym = 5,
            MaxMembersPerGym = 50,
            IsActive = true,
            CreatedOn = DateTime.UtcNow
        };
        _context.SubscriptionPlan.Add(plan);
        await _context.SaveChangesAsync();

        await _sut.HardDeleteAsync(plan);
        await _context.SaveChangesAsync();

        var fromDb = await _context.SubscriptionPlan.FindAsync(plan.Id);
        fromDb.Should().BeNull();
    }

    #endregion
}
