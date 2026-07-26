using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Cache;
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


}
