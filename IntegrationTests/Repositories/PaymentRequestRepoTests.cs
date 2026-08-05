using Application.DTO.Pagintion;
using Domain.Enum;
using Domain.Model;
using FluentAssertions;
using Infrastructure.Repo;
using IntegrationTests.Helpers;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace IntegrationTests.Repositories;

public class PaymentRequestRepoTests : IDisposable
{
    private readonly Infrastructure.Persistence.ApplicationDbContext _context;
    private readonly PaymentRequestRepo _sut;
    private readonly string _dbName;

    public PaymentRequestRepoTests()
    {
        _dbName = Guid.NewGuid().ToString();
        _context = InMemoryDbContextFactory.Create(_dbName);
        var currentUser = InMemoryDbContextFactory.SuperAdminCurrentUser();
        _sut = new PaymentRequestRepo(
            _context,
            InMemoryDbContextFactory.Logger<Infrastructure.Repo.PaymentRequestRepo>().Object,
            new Infrastructure.Cache.QueryCache(),
            currentUser);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    private async Task<SubscriptionPlan> SeedPlanAsync()
    {
        var plan = new SubscriptionPlan
        {
            Name = "Basic",
            IsFree = false,
            MaxOwnedGyms = 1,
            MaxCoaches = 5,
            MaxMembers = 50,
            CreatedOn = DateTime.UtcNow
        };
        _context.SubscriptionPlan.Add(plan);
        await _context.SaveChangesAsync();
        return plan;
    }

    #region AddAsync

    [Fact]
    public async Task AddAsync_ShouldPersistEntity_WhenDataIsValid()
    {
        var plan = await SeedPlanAsync();

        var paymentRequest = new PaymentRequest
        {
            PlanId = plan.Id,
            PlanPriceId = 0,
            OriginalAmount = 100m,
            FinalAmount = 100m,
            CurrencyCode = "USD",
            Status = PaymentRequestStatus.Pending,
            CreatedOn = DateTime.UtcNow,
            CreatedById = 1,
            StoredFilePath = string.Empty,
            RowVersion = [1, 2, 3]
        };

        var result = await _sut.AddAsync(paymentRequest);
        await _context.SaveChangesAsync();

        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);

        var fromDb = await _context.PaymentRequest.FindAsync(result.Id);
        fromDb.Should().NotBeNull();
        fromDb!.Status.Should().Be(PaymentRequestStatus.Pending);
    }

    #endregion

    #region HasPendingRequestAsync

    [Fact]
    public async Task HasPendingRequestAsync_ShouldReturnTrue_WhenPendingRequestExists()
    {
        var plan = await SeedPlanAsync();

        var paymentRequest = new PaymentRequest
        {
            PlanId = plan.Id,
            PlanPriceId = 0,
            OriginalAmount = 100m,
            FinalAmount = 100m,
            CurrencyCode = "USD",
            Status = PaymentRequestStatus.Pending,
            CreatedOn = DateTime.UtcNow,
            CreatedById = 1,
            StoredFilePath = string.Empty,
            RowVersion = [1, 2, 3]
        };
        _context.PaymentRequest.Add(paymentRequest);
        await _context.SaveChangesAsync();

        var result = await _sut.HasPendingRequestAsync(1);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasPendingRequestAsync_ShouldReturnFalse_WhenNoPendingRequest()
    {
        var plan = await SeedPlanAsync();

        var paymentRequest = new PaymentRequest
        {
            PlanId = plan.Id,
            PlanPriceId = 0,
            OriginalAmount = 100m,
            FinalAmount = 100m,
            CurrencyCode = "USD",
            Status = PaymentRequestStatus.Approved,
            CreatedOn = DateTime.UtcNow,
            CreatedById = 1,
            StoredFilePath = string.Empty,
            RowVersion = [1, 2, 3]
        };
        _context.PaymentRequest.Add(paymentRequest);
        await _context.SaveChangesAsync();

        var result = await _sut.HasPendingRequestAsync(1);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task HasPendingRequestAsync_ShouldReturnFalse_WhenPendingRequestBelongsToDifferentUser()
    {
        var plan = await SeedPlanAsync();

        var paymentRequest = new PaymentRequest
        {
            PlanId = plan.Id,
            PlanPriceId = 0,
            OriginalAmount = 100m,
            FinalAmount = 100m,
            CurrencyCode = "USD",
            Status = PaymentRequestStatus.Pending,
            CreatedOn = DateTime.UtcNow,
            CreatedById = 999,
            StoredFilePath = string.Empty,
            RowVersion = [1, 2, 3]
        };
        _context.PaymentRequest.Add(paymentRequest);
        await _context.SaveChangesAsync();

        var result = await _sut.HasPendingRequestAsync(1);

        result.Should().BeFalse();
    }

    #endregion

    #region HasUsedThisCouponBeforeAsync

    [Fact]
    public async Task HasUsedThisCouponBeforeAsync_ShouldReturnTrue_WhenCouponUsedAndApproved()
    {
        var plan = await SeedPlanAsync();

        var paymentRequest = new PaymentRequest
        {
            PlanId = plan.Id,
            PlanPriceId = 0,
            CouponId = 1,
            OriginalAmount = 100m,
            DiscountAmount = 10m,
            FinalAmount = 90m,
            CurrencyCode = "USD",
            Status = PaymentRequestStatus.Approved,
            CreatedOn = DateTime.UtcNow,
            CreatedById = 1,
            StoredFilePath = string.Empty,
            RowVersion = [1, 2, 3]
        };
        _context.PaymentRequest.Add(paymentRequest);
        await _context.SaveChangesAsync();

        var result = await _sut.HasUsedThisCouponBeforeAsync(1, 1);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasUsedThisCouponBeforeAsync_ShouldReturnFalse_WhenCouponNeverUsed()
    {
        var result = await _sut.HasUsedThisCouponBeforeAsync(1, 999);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task HasUsedThisCouponBeforeAsync_ShouldReturnFalse_WhenCouponUsedButNotApproved()
    {
        var plan = await SeedPlanAsync();

        var paymentRequest = new PaymentRequest
        {
            PlanId = plan.Id,
            PlanPriceId = 0,
            CouponId = 1,
            OriginalAmount = 100m,
            FinalAmount = 100m,
            CurrencyCode = "USD",
            Status = PaymentRequestStatus.Pending,
            CreatedOn = DateTime.UtcNow,
            CreatedById = 1,
            StoredFilePath = string.Empty,
            RowVersion = [1, 2, 3]
        };
        _context.PaymentRequest.Add(paymentRequest);
        await _context.SaveChangesAsync();

        var result = await _sut.HasUsedThisCouponBeforeAsync(1, 1);

        result.Should().BeFalse();
    }

    #endregion

    #region GetByIdAsync

    [Fact]
    public async Task GetByIdAsync_ShouldReturnEntity_WhenExists()
    {
        var plan = await SeedPlanAsync();

        var paymentRequest = new PaymentRequest
        {
            PlanId = plan.Id,
            PlanPriceId = 0,
            OriginalAmount = 100m,
            FinalAmount = 100m,
            CurrencyCode = "USD",
            Status = PaymentRequestStatus.Pending,
            CreatedOn = DateTime.UtcNow,
            CreatedById = 1,
            StoredFilePath = string.Empty,
            RowVersion = [1, 2, 3]
        };
        _context.PaymentRequest.Add(paymentRequest);
        await _context.SaveChangesAsync();

        var result = await _sut.GetByIdAsync(paymentRequest.Id);

        result.Should().NotBeNull();
        result!.OriginalAmount.Should().Be(100m);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenEntityDoesNotExist()
    {
        var result = await _sut.GetByIdAsync(999);

        result.Should().BeNull();
    }

    #endregion

    #region GetAllAsync

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllActiveEntities()
    {
        var plan = await SeedPlanAsync();

        _context.PaymentRequest.AddRange(
            new PaymentRequest
            {
                PlanId = plan.Id,
                PlanPriceId = 0,
                OriginalAmount = 100m,
                FinalAmount = 100m,
                CurrencyCode = "USD",
                Status = PaymentRequestStatus.Pending,
                CreatedOn = DateTime.UtcNow,
                CreatedById = 1,
                StoredFilePath = string.Empty,
                RowVersion = [1, 2, 3]
            },
            new PaymentRequest
            {
                PlanId = plan.Id,
                PlanPriceId = 0,
                OriginalAmount = 200m,
                FinalAmount = 200m,
                CurrencyCode = "USD",
                Status = PaymentRequestStatus.Approved,
                CreatedOn = DateTime.UtcNow,
                CreatedById = 1,
                StoredFilePath = string.Empty,
                RowVersion = [1, 2, 3]
            }
        );
        await _context.SaveChangesAsync();

        var result = await _sut.GetAllAsync();

        result.Should().HaveCount(2);
    }

    #endregion

    #region GetPageAsync

    [Fact]
    public async Task GetPageAsync_ShouldReturnPaginatedResults()
    {
        var plan = await SeedPlanAsync();

        for (int i = 1; i <= 12; i++)
        {
            _context.PaymentRequest.Add(new PaymentRequest
            {
                PlanId = plan.Id,
                PlanPriceId = 0,
                OriginalAmount = i * 10m,
                FinalAmount = i * 10m,
                CurrencyCode = "USD",
                Status = PaymentRequestStatus.Pending,
                CreatedOn = DateTime.UtcNow,
                CreatedById = 1,
                StoredFilePath = string.Empty,
                RowVersion = [1, 2, 3]
            });
        }
        await _context.SaveChangesAsync();

        var searchReq = new PaginatedSearchReq { PageNumber = 1, PageSize = 10 };

        var result = await _sut.GetPageAsync(searchReq);

        result.Should().NotBeNull();
        result.Items.Should().HaveCount(10);
        result.TotalCount.Should().Be(12);
    }

    #endregion

    #region DeleteAsync (Soft Delete)

    [Fact]
    public async Task DeleteAsync_ShouldSoftDelete()
    {
        var plan = await SeedPlanAsync();

        var paymentRequest = new PaymentRequest
        {
            PlanId = plan.Id,
            PlanPriceId = 0,
            OriginalAmount = 100m,
            FinalAmount = 100m,
            CurrencyCode = "USD",
            Status = PaymentRequestStatus.Pending,
            CreatedOn = DateTime.UtcNow,
            CreatedById = 1,
            StoredFilePath = string.Empty,
            RowVersion = [1, 2, 3]
        };
        _context.PaymentRequest.Add(paymentRequest);
        await _context.SaveChangesAsync();

        var result = await _sut.DeleteAsync(paymentRequest);
        await _context.SaveChangesAsync();

        
    }

    #endregion
}