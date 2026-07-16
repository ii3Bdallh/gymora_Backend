using Application.DTO.Pagintion;
using Application.Model;
using Domain.Enum;
using Domain.Model;
using FluentAssertions;
using Infrastructure.Cache;
using Infrastructure.Repo;
using IntegrationTests.Helpers;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace IntegrationTests.Repositories;

public class CouponRepoTests : IDisposable
{
    private readonly Infrastructure.Persistence.ApplicationDbContext _context;
    private readonly CouponRepo _sut;
    private readonly string _dbName;

    public CouponRepoTests()
    {
        _dbName = Guid.NewGuid().ToString();
        _context = InMemoryDbContextFactory.Create(_dbName);
        var currentUser = InMemoryDbContextFactory.SuperAdminCurrentUser();
        _sut = new CouponRepo(
            _context,
            InMemoryDbContextFactory.Logger<Infrastructure.Repo.CouponRepo>().Object,
            new QueryCache(),
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
        var coupon = new Coupon
        {
            Code = "SAVE10",
            Name = "Save 10%",
            DiscountType = DiscountType.Percentage,
            DiscountValue = 10m,
            ValidFrom = DateTime.UtcNow.AddDays(-1),
            ValidTo = DateTime.UtcNow.AddDays(30),
            UsageLimit = 100,
            UsedCount = 0,
            IsActive = true,
            RowVersion = [1, 2, 3]
        };

        var result = await _sut.AddAsync(coupon);

        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);

        var fromDb = await _context.Coupon.FindAsync(result.Id);
        fromDb.Should().NotBeNull();
        fromDb!.Code.Should().Be("SAVE10");
    }

    #endregion

    #region GetByIdAsync

    [Fact]
    public async Task GetByIdAsync_ShouldReturnEntity_WhenExists()
    {
        var coupon = new Coupon
        {
            Code = "TEST10",
            Name = "Test",
            DiscountType = DiscountType.FixedAmount,
            DiscountValue = 10m,
            ValidFrom = DateTime.UtcNow.AddDays(-1),
            ValidTo = DateTime.UtcNow.AddDays(30),
            IsActive = true,
            RowVersion = [1, 2, 3]
        };
        _context.Coupon.Add(coupon);
        await _context.SaveChangesAsync();

        var result = await _sut.GetByIdAsync(coupon.Id);

        result.Should().NotBeNull();
        result!.Code.Should().Be("TEST10");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenEntityDoesNotExist()
    {
        var result = await _sut.GetByIdAsync(999);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenEntityIsSoftDeleted()
    {
        var coupon = new Coupon
        {
            Code = "DELETED",
            Name = "Deleted Coupon",
            DiscountType = DiscountType.FixedAmount,
            DiscountValue = 5m,
            ValidFrom = DateTime.UtcNow.AddDays(-1),
            ValidTo = DateTime.UtcNow.AddDays(30),
            IsActive = false,
            RowVersion = [1, 2, 3]
        };
        _context.Coupon.Add(coupon);
        await _context.SaveChangesAsync();

        var result = await _sut.GetByIdAsync(coupon.Id, isActive: true);

        result.Should().BeNull();
    }

    #endregion

    #region UpdateAsync

    [Fact]
    public async Task UpdateAsync_ShouldPersistChanges()
    {
        var coupon = new Coupon
        {
            Code = "UPDATE10",
            Name = "Original",
            DiscountType = DiscountType.FixedAmount,
            DiscountValue = 10m,
            ValidFrom = DateTime.UtcNow.AddDays(-1),
            ValidTo = DateTime.UtcNow.AddDays(30),
            IsActive = true,
            RowVersion = [1, 2, 3]
        };
        _context.Coupon.Add(coupon);
        await _context.SaveChangesAsync();

        _context.Entry(coupon).State = EntityState.Detached;

        var tracked = await _context.Coupon.FindAsync(coupon.Id);
        tracked!.Name = "Updated";

        var result = await _sut.UpdateAsync(tracked);
        await _context.SaveChangesAsync();

        var fromDb = await _context.Coupon.FindAsync(coupon.Id);
        fromDb!.Name.Should().Be("Updated");
    }

    #endregion

    #region DeleteAsync (Soft Delete)

    [Fact]
    public async Task DeleteAsync_ShouldSoftDelete_WhenEntityExists()
    {
        var coupon = new Coupon
        {
            Code = "SOFTDEL",
            Name = "Soft Delete Test",
            DiscountType = DiscountType.FixedAmount,
            DiscountValue = 5m,
            ValidFrom = DateTime.UtcNow.AddDays(-1),
            ValidTo = DateTime.UtcNow.AddDays(30),
            IsActive = true,
            RowVersion = [1, 2, 3]
        };
        _context.Coupon.Add(coupon);
        await _context.SaveChangesAsync();

        var result = await _sut.DeleteAsync(coupon);
        await _context.SaveChangesAsync();

        result.IsActive.Should().BeFalse();

        var fromDb = await _context.Coupon.FindAsync(coupon.Id);
        fromDb!.IsActive.Should().BeFalse();
    }

    #endregion

    #region GetAllAsync

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllActiveEntities()
    {
        _context.Coupon.AddRange(
            new Coupon
            {
                Code = "ACTIVE1", Name = "Active 1",
                DiscountType = DiscountType.FixedAmount,
                DiscountValue = 10m,
                ValidFrom = DateTime.UtcNow.AddDays(-1),
                ValidTo = DateTime.UtcNow.AddDays(30),
                IsActive = true,
                RowVersion = [1, 2, 3]
            },
            new Coupon
            {
                Code = "ACTIVE2", Name = "Active 2",
                DiscountType = DiscountType.FixedAmount,
                DiscountValue = 20m,
                ValidFrom = DateTime.UtcNow.AddDays(-1),
                ValidTo = DateTime.UtcNow.AddDays(30),
                IsActive = true,
                RowVersion = [1, 2, 3]
            },
            new Coupon
            {
                Code = "INACTIVE", Name = "Inactive",
                DiscountType = DiscountType.FixedAmount,
                DiscountValue = 5m,
                ValidFrom = DateTime.UtcNow.AddDays(-1),
                ValidTo = DateTime.UtcNow.AddDays(30),
                IsActive = false,
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
        for (int i = 1; i <= 15; i++)
        {
            _context.Coupon.Add(new Coupon
            {
                Code = $"COUPON{i:D2}",
                Name = $"Coupon {i}",
                DiscountType = DiscountType.FixedAmount,
                DiscountValue = i * 10m,
                ValidFrom = DateTime.UtcNow.AddDays(-1),
                ValidTo = DateTime.UtcNow.AddDays(30),
                IsActive = true,
                RowVersion = [1, 2, 3]
            });
        }
        await _context.SaveChangesAsync();

        var searchReq = new PaginatedSearchReq { PageNumber = 1, PageSize = 10 };

        var result = await _sut.GetPageAsync(searchReq);

        result.Should().NotBeNull();
        result.Items.Should().HaveCount(10);
        result.TotalCount.Should().Be(15);
        result.PageNumber.Should().Be(1);
    }

    [Fact]
    public async Task GetPageAsync_ShouldReturnSecondPage()
    {
        for (int i = 1; i <= 15; i++)
        {
            _context.Coupon.Add(new Coupon
            {
                Code = $"COUPON{i:D2}",
                Name = $"Coupon {i}",
                DiscountType = DiscountType.FixedAmount,
                DiscountValue = i * 10m,
                ValidFrom = DateTime.UtcNow.AddDays(-1),
                ValidTo = DateTime.UtcNow.AddDays(30),
                IsActive = true,
                RowVersion = [1, 2, 3]
            });
        }
        await _context.SaveChangesAsync();

        var searchReq = new PaginatedSearchReq { PageNumber = 2, PageSize = 10 };

        var result = await _sut.GetPageAsync(searchReq);

        result.Items.Should().HaveCount(5);
        result.PageNumber.Should().Be(2);
    }

    #endregion

    #region GetByCodeAsync

    [Fact]
    public async Task GetByCodeAsync_ShouldReturnCoupon_WhenCodeExists()
    {
        var coupon = new Coupon
        {
            Code = "UNIQUE10",
            Name = "Unique Coupon",
            DiscountType = DiscountType.FixedAmount,
            DiscountValue = 10m,
            ValidFrom = DateTime.UtcNow.AddDays(-1),
            ValidTo = DateTime.UtcNow.AddDays(30),
            IsActive = true,
            RowVersion = [1, 2, 3]
        };
        _context.Coupon.Add(coupon);
        await _context.SaveChangesAsync();

        var result = await _sut.GetByCodeAsync("UNIQUE10");

        result.Should().NotBeNull();
        result!.Code.Should().Be("UNIQUE10");
    }

    [Fact]
    public async Task GetByCodeAsync_ShouldBeCaseInsensitive()
    {
        var coupon = new Coupon
        {
            Code = "CASE10",
            Name = "Case Test",
            DiscountType = DiscountType.FixedAmount,
            DiscountValue = 10m,
            ValidFrom = DateTime.UtcNow.AddDays(-1),
            ValidTo = DateTime.UtcNow.AddDays(30),
            IsActive = true,
            RowVersion = [1, 2, 3]
        };
        _context.Coupon.Add(coupon);
        await _context.SaveChangesAsync();

        var result = await _sut.GetByCodeAsync("case10");

        result.Should().NotBeNull();
        result!.Code.Should().Be("CASE10");
    }

    [Fact]
    public async Task GetByCodeAsync_ShouldReturnNull_WhenCodeDoesNotExist()
    {
        var result = await _sut.GetByCodeAsync("NONEXISTENT");

        result.Should().BeNull();
    }

    #endregion

    #region HardDeleteAsync

    [Fact]
    public async Task HardDeleteAsync_ShouldRemoveEntityFromDatabase()
    {
        var coupon = new Coupon
        {
            Code = "HARDDEL",
            Name = "Hard Delete",
            DiscountType = DiscountType.FixedAmount,
            DiscountValue = 5m,
            ValidFrom = DateTime.UtcNow.AddDays(-1),
            ValidTo = DateTime.UtcNow.AddDays(30),
            IsActive = true,
            RowVersion = [1, 2, 3]
        };
        _context.Coupon.Add(coupon);
        await _context.SaveChangesAsync();

        await _sut.HardDeleteAsync(coupon);
        await _context.SaveChangesAsync();

        var fromDb = await _context.Coupon.FindAsync(coupon.Id);
        fromDb.Should().BeNull();
    }

    #endregion
}
