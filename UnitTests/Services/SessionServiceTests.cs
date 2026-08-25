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

public class SessionServiceTests
{
    private readonly Mock<ISessionRepo> _repo;
    private readonly Mock<IUserWorkoutBlockRepo> _blockRepo;
    private readonly Mock<ICurrentPlanService> _currentPlanService;
    private readonly Mock<IWorkoutPlanRepo> _workoutPlanRepo;
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<IMapper> _mapper;
    private readonly Mock<ICacheService> _cacheService;
    private readonly Mock<IPublishEndpoint> _publishEndpoint;
    private readonly CurrentUser _currentUser;
    private readonly Mock<ILogger<SessionService>> _logger;
    private readonly SessionService _sut;

    public SessionServiceTests()
    {
        _repo = new Mock<ISessionRepo>();
        _blockRepo = new Mock<IUserWorkoutBlockRepo>();
        _currentPlanService = new Mock<ICurrentPlanService>();
        _workoutPlanRepo = new Mock<IWorkoutPlanRepo>();
        _unitOfWork = new Mock<IUnitOfWork>();
        _mapper = new Mock<IMapper>();
        _cacheService = new Mock<ICacheService>();
        _publishEndpoint = new Mock<IPublishEndpoint>();
        _currentUser = Mocks.SuperAdminCurrentUser(userId: 1);
        _logger = new Mock<ILogger<SessionService>>();

        _publishEndpoint.Setup(x => x.Publish(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _sut = new SessionService(
            _repo.Object,
            _blockRepo.Object,
            _currentPlanService.Object,
            _workoutPlanRepo.Object,
            _unitOfWork.Object,
            _mapper.Object,
            _cacheService.Object,
            _publishEndpoint.Object,
            _currentUser,
            _logger.Object
        );
    }

    [Fact]
    public async Task ApproveAsync_ShouldThrowForbidden_WhenUserIsNotSuperAdmin()
    {
        // Arrange
        _currentUser.PlatformRole = null;

        // Act
        Func<Task> act = async () => await _sut.ApproveAsync(1, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task ApproveAsync_ShouldThrowNotFound_WhenSessionDoesNotExist()
    {
        // Arrange
        _currentUser.PlatformRole = AppRole.SuperAdmin;
        _repo.Setup(r => r.GetByIdAsync(1, true, It.IsAny<CancellationToken>(), null))
            .ReturnsAsync((Session?)null);

        // Act
        Func<Task> act = async () => await _sut.ApproveAsync(1, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task ApproveAsync_ShouldApprove_WhenUserIsSuperAdmin()
    {
        // Arrange
        _currentUser.PlatformRole = AppRole.SuperAdmin;
        var session = new Session { Id = 1, SessionName = "Leg Day", IsApproved = false };
        _repo.Setup(r => r.GetByIdAsync(1, true, It.IsAny<CancellationToken>(), null))
            .ReturnsAsync(session);

        // Act
        await _sut.ApproveAsync(1, CancellationToken.None);

        // Assert
        session.IsApproved.Should().BeTrue();
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
