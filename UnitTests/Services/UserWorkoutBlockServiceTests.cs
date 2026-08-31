using Application.DTO.Exceptions;
using Application.DTO.Model;
using Application.Interface.Repo;
using Application.Interface.Service.Shared;
using Application.Model;
using Application.Service;
using AutoMapper;
using Domain.Enum;
using Domain.Model;
using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnitTests.Helpers;
using Xunit;

namespace UnitTests.Services;

public class UserWorkoutBlockServiceTests
{
    private readonly Mock<IUserWorkoutBlockRepo> _repo;
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<IMapper> _mapper;
    private readonly Mock<ICacheService> _cacheService;
    private readonly Mock<IPublishEndpoint> _publishEndpoint;
    private readonly CurrentUser _currentUser;
    private readonly Mock<ILogger<UserWorkoutBlockService>> _logger;
    private readonly UserWorkoutBlockService _sut;

    public UserWorkoutBlockServiceTests()
    {
        _repo = new Mock<IUserWorkoutBlockRepo>();
        _unitOfWork = new Mock<IUnitOfWork>();
        _mapper = new Mock<IMapper>();
        _cacheService = new Mock<ICacheService>();
        _publishEndpoint = new Mock<IPublishEndpoint>();
        _currentUser = Mocks.SuperAdminCurrentUser(userId: 1);
        _logger = new Mock<ILogger<UserWorkoutBlockService>>();

        _publishEndpoint.Setup(x => x.Publish(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _sut = new UserWorkoutBlockService(
            _repo.Object,
            _unitOfWork.Object,
            _mapper.Object,
            _cacheService.Object,
            _publishEndpoint.Object,
            _currentUser,
            _logger.Object
        );
    }

    [Fact]
    public async Task AddAsync_ShouldThrowForbidden_WhenUserIsNotSuperAdmin()
    {
        // Arrange
        _currentUser.PlatformRole = null;
        var dto = new UserWorkoutBlockCDTO { BlockedUserId = 5, DurationDays = 7 };

        // Act
        Func<Task> act = async () => await _sut.AddAsync(dto, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task UnblockUserAsync_ShouldThrowForbidden_WhenUserIsNotSuperAdmin()
    {
        // Arrange
        _currentUser.PlatformRole = null;

        // Act
        Func<Task> act = async () => await _sut.UnblockUserAsync(5, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ForbiddenException>();
    }
}
