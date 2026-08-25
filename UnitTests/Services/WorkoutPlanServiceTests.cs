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

public class WorkoutPlanServiceTests
{
    private readonly Mock<IWorkoutPlanRepo> _repo;
    private readonly Mock<IUserWorkoutBlockRepo> _blockRepo;
    private readonly Mock<ICurrentPlanService> _currentPlanService;
    private readonly Mock<IMemberWorkoutPlanRepo> _memberWorkoutPlanRepo;
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<IMapper> _mapper;
    private readonly Mock<ICacheService> _cacheService;
    private readonly Mock<IPublishEndpoint> _publishEndpoint;
    private readonly CurrentUser _currentUser;
    private readonly Mock<IStorageService> _storageService;
    private readonly Mock<ILogger<WorkoutPlanService>> _logger;
    private readonly WorkoutPlanService _sut;

    public WorkoutPlanServiceTests()
    {
        _repo = new Mock<IWorkoutPlanRepo>();
        _blockRepo = new Mock<IUserWorkoutBlockRepo>();
        _currentPlanService = new Mock<ICurrentPlanService>();
        _memberWorkoutPlanRepo = new Mock<IMemberWorkoutPlanRepo>();
        _unitOfWork = new Mock<IUnitOfWork>();
        _mapper = new Mock<IMapper>();
        _cacheService = new Mock<ICacheService>();
        _publishEndpoint = new Mock<IPublishEndpoint>();
        _currentUser = Mocks.SuperAdminCurrentUser(userId: 1);
        _storageService = new Mock<IStorageService>();
        _logger = new Mock<ILogger<WorkoutPlanService>>();

        _publishEndpoint.Setup(x => x.Publish(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _sut = new WorkoutPlanService(
            _repo.Object,
            _blockRepo.Object,
            _currentPlanService.Object,
            _memberWorkoutPlanRepo.Object,
            _unitOfWork.Object,
            _mapper.Object,
            _cacheService.Object,
            _publishEndpoint.Object,
            _currentUser,
            _storageService.Object,
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
    public async Task ApproveAsync_ShouldThrowNotFound_WhenPlanDoesNotExist()
    {
        // Arrange
        _currentUser.PlatformRole = AppRole.SuperAdmin;
        _repo.Setup(r => r.GetByIdAsync(1, true, It.IsAny<CancellationToken>(), null))
            .ReturnsAsync((WorkoutPlan?)null);

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
        var plan = new WorkoutPlan { Id = 1, PlanName = "Full Body", IsApproved = false };
        _repo.Setup(r => r.GetByIdAsync(1, true, It.IsAny<CancellationToken>(), null))
            .ReturnsAsync(plan);

        // Act
        await _sut.ApproveAsync(1, CancellationToken.None);

        // Assert
        plan.IsApproved.Should().BeTrue();
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
