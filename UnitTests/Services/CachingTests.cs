using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Cache;
using Application.DTO.Pagintion;
using Application.EventConsumer;
using Application.Interface.Service.Shared;
using Domain.Events;
using Domain.Interface;
using Domain.Options;
using FluentAssertions;
using Infrastructure.Cache;
using MassTransit;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace UnitTests.Services;

public class CachingTests
{
    #region Mock Entities for Testing Cache Keys

    private class MockGymEntity : IBaseGymEntity
    {
        public int GymId { get; set; }
    }

    private class MockUserEntity : IOnlyMeCanSee
    {
        public int CreatedById { get; set; }
    }

    private class MockGymAndUserEntity : IBaseGymEntity, IOnlyMeCanSee
    {
        public int GymId { get; set; }
        public int CreatedById { get; set; }
    }

    private class MockGlobalEntity
    {
    }

    private class MockOnlyMeCanSeeAtGymEntity : IOnlyMeCanSeeAtGym
    {
        public int CreatedByPersonId { get; set; }
    }

    private class MockGymAndOnlyMeCanSeeAtGymEntity : IBaseGymEntity, IOnlyMeCanSeeAtGym
    {
        public int GymId { get; set; }
        public int CreatedByPersonId { get; set; }
    }

    #endregion

    #region CacheKeyGenerator Tests

    [Fact]
    public void ById_ShouldScopeByGymIdOnly_WhenEntityIsGymEntity()
    {
        // Act
        var key = CacheKeyGenerator.ById<MockGymEntity>(entityId: 5, gymId: 10, userId: 20);

        // Assert
        // Expecting gymora:gym:10:mockgymentity:id:5
        key.Should().Be("gymora:gym:10:mockgymentity:id:5");
    }

    [Fact]
    public void ById_ShouldScopeByUserIdOnly_WhenEntityIsUserEntity()
    {
        // Act
        var key = CacheKeyGenerator.ById<MockUserEntity>(entityId: 5, gymId: 10, userId: 20);

        // Assert
        // Expecting gymora:global:user:20:mockuserentity:id:5
        key.Should().Be("gymora:global:user:20:mockuserentity:id:5");
    }

    [Fact]
    public void ById_ShouldScopeByBothGymAndUser_WhenEntityIsGymAndUserEntity()
    {
        // Act
        var key = CacheKeyGenerator.ById<MockGymAndUserEntity>(entityId: 5, gymId: 10, userId: 20);

        // Assert
        // Expecting gymora:gym:10:user:20:mockgymanduserentity:id:5
        key.Should().Be("gymora:gym:10:user:20:mockgymanduserentity:id:5");
    }

    [Fact]
    public void ById_ShouldScopeGlobally_WhenEntityIsNeitherGymNorUserEntity()
    {
        // Act
        var key = CacheKeyGenerator.ById<MockGlobalEntity>(entityId: 5, gymId: 10, userId: 20);

        // Assert
        // Expecting gymora:global:mockglobalentity:id:5
        key.Should().Be("gymora:global:mockglobalentity:id:5");
    }

    [Fact]
    public void ById_ShouldScopeByUserIdOnly_WhenEntityIsOnlyMeCanSeeAtGymEntity()
    {
        // Act
        var key = CacheKeyGenerator.ById<MockOnlyMeCanSeeAtGymEntity>(entityId: 5, gymId: 10, userId: 20);

        // Assert
        // Expecting gymora:global:user:20:mockonlymecanseeatgymentity:id:5
        key.Should().Be("gymora:global:user:20:mockonlymecanseeatgymentity:id:5");
    }

    [Fact]
    public void ById_ShouldScopeByBothGymAndUser_WhenEntityIsGymAndOnlyMeCanSeeAtGymEntity()
    {
        // Act
        var key = CacheKeyGenerator.ById<MockGymAndOnlyMeCanSeeAtGymEntity>(entityId: 5, gymId: 10, userId: 20);

        // Assert
        // Expecting gymora:gym:10:user:20:mockgymandonlymecanseeatgymentity:id:5
        key.Should().Be("gymora:gym:10:user:20:mockgymandonlymecanseeatgymentity:id:5");
    }

    #endregion


    [Fact]
    public async Task CacheService_ShouldStoreAndRetrieveAndRemoveValuesCorrectly()
    {
        // Arrange
        using var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var options = new CacheOptions
        {
            DefaultAbsoluteExpirationMinutes = 10,
            DefaultSlidingExpirationMinutes = 5
        };
        var optionsMock = new Mock<IOptions<CacheOptions>>();
        optionsMock.Setup(o => o.Value).Returns(options);
        var loggerMock = new Mock<ILogger<CacheService>>();

        var cacheService = new CacheService(memoryCache, optionsMock.Object, loggerMock.Object);
        var key = "test-key";
        var value = "cached-value";

        // Act & Assert - Set
        await cacheService.SetAsync(key, value);
        var retrieved = await cacheService.GetAsync<string>(key);
        retrieved.Should().Be(value);

        // Act & Assert - Remove
        await cacheService.RemoveAsync(key);
        var afterRemoval = await cacheService.GetAsync<string>(key);
        afterRemoval.Should().BeNull();
    }

    [Fact]
    public async Task CacheService_ShouldRemoveByPrefixCorrectly()
    {
        // Arrange
        using var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var options = new CacheOptions
        {
            DefaultAbsoluteExpirationMinutes = 10,
            DefaultSlidingExpirationMinutes = 5
        };
        var optionsMock = new Mock<IOptions<CacheOptions>>();
        optionsMock.Setup(o => o.Value).Returns(options);
        var loggerMock = new Mock<ILogger<CacheService>>();

        var cacheService = new CacheService(memoryCache, optionsMock.Object, loggerMock.Object);
        
        await cacheService.SetAsync("gymora:gym:1:sub:page:1", "page1");
        await cacheService.SetAsync("gymora:gym:1:sub:page:2", "page2");
        await cacheService.SetAsync("gymora:gym:1:sub:id:5", "id5");

        // Act
        await cacheService.RemoveByPrefixAsync("gymora:gym:1:sub:page:");

        // Assert
        (await cacheService.GetAsync<string>("gymora:gym:1:sub:page:1")).Should().BeNull();
        (await cacheService.GetAsync<string>("gymora:gym:1:sub:page:2")).Should().BeNull();
        (await cacheService.GetAsync<string>("gymora:gym:1:sub:id:5")).Should().Be("id5");
    }

    [Fact]
    public void ForPage_ShouldGenerateDifferentKeys_ForDifferentQueryRequests()
    {
        // Arrange
        var req1 = new PaginatedSearchReq { PageNumber = 1, PageSize = 10, SearchTerm = "test" };
        var req2 = new PaginatedSearchReq { PageNumber = 2, PageSize = 10, SearchTerm = "test" };
        var req3 = new PaginatedSearchReq { PageNumber = 1, PageSize = 10, SearchTerm = "test", OrderBy = "Name" };

        // Act
        var key1 = CacheKeyGenerator.ForPage<MockGymEntity>(req1, gymId: 1);
        var key2 = CacheKeyGenerator.ForPage<MockGymEntity>(req2, gymId: 1);
        var key3 = CacheKeyGenerator.ForPage<MockGymEntity>(req3, gymId: 1);

        // Assert
        key1.Should().NotBe(key2);
        key1.Should().NotBe(key3);
        key1.Should().StartWith("gymora:gym:1:mockgymentity:page:");
    }

    [Fact]
    public async Task CacheInvalidationConsumer_ShouldCallRemoveAndRemoveByPrefix_WhenEntityChangedEventReceived()
    {
        // Arrange
        var cacheServiceMock = new Mock<ICacheService>();
        var loggerMock = new Mock<ILogger<CacheInvalidationConsumer>>();
        var consumer = new CacheInvalidationConsumer(cacheServiceMock.Object, loggerMock.Object);

        var contextMock = new Mock<ConsumeContext<EntityChangedEvent>>();
        contextMock.Setup(c => c.Message).Returns(new EntityChangedEvent("subscription_plan", 5, 10, 20));

        // Act
        await consumer.Consume(contextMock.Object);

        // Assert
        var expectedIdKey = "gymora:global:subscription_plan:id:5";
        var expectedPagesPrefix = "gymora:global:subscription_plan:page:";

        cacheServiceMock.Verify(c => c.RemoveAsync(expectedIdKey), Times.Once);
        cacheServiceMock.Verify(c => c.RemoveByPrefixAsync(expectedPagesPrefix), Times.Once);
    }

    private class DerivedTestSearchReq : PaginatedSearchReq
    {
        public int CoachId { get; set; }
    }

    [Fact]
    public void ForPage_ShouldGenerateDifferentKeys_ForDerivedQueryRequestsWithDifferentSubclassProperties()
    {
        // Arrange
        var req1 = new DerivedTestSearchReq { PageNumber = 1, PageSize = 10, CoachId = 5 };
        var req2 = new DerivedTestSearchReq { PageNumber = 1, PageSize = 10, CoachId = 10 };

        // Act
        var key1 = CacheKeyGenerator.ForPage<MockGymEntity>(req1, gymId: 1);
        var key2 = CacheKeyGenerator.ForPage<MockGymEntity>(req2, gymId: 1);

        // Assert
        key1.Should().NotBe(key2);
    }
}
