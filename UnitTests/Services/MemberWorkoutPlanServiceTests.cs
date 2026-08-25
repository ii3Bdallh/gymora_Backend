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

public class MemberWorkoutPlanServiceTests
{
    private readonly Mock<IMemberWorkoutPlanRepo> _repo;
    private readonly Mock<IGymPersonRepo> _gymPersonRepo;
    private readonly Mock<IWorkoutPlanRepo> _workoutPlanRepo;
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<IMapper> _mapper;
    private readonly Mock<ICacheService> _cacheService;
    private readonly Mock<IPublishEndpoint> _publishEndpoint;
    private readonly CurrentUser _currentUser;
    private readonly Mock<ILogger<MemberWorkoutPlanService>> _logger;
    private readonly MemberWorkoutPlanService _sut;

    public MemberWorkoutPlanServiceTests()
    {
        _repo = new Mock<IMemberWorkoutPlanRepo>();
        _gymPersonRepo = new Mock<IGymPersonRepo>();
        _workoutPlanRepo = new Mock<IWorkoutPlanRepo>();
        _unitOfWork = new Mock<IUnitOfWork>();
        _mapper = new Mock<IMapper>();
        _cacheService = new Mock<ICacheService>();
        _publishEndpoint = new Mock<IPublishEndpoint>();
        _currentUser = Mocks.DefaultCurrentUser(userId: 1, gymId: 10);
        _logger = new Mock<ILogger<MemberWorkoutPlanService>>();

        _publishEndpoint.Setup(x => x.Publish(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _sut = new MemberWorkoutPlanService(
            _repo.Object,
            _gymPersonRepo.Object,
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
    public async Task CancelAssignmentAsync_ShouldThrowNotFound_WhenAssignmentDoesNotExist()
    {
        // Arrange
        _repo.Setup(r => r.GetByIdAsync(1, true, It.IsAny<CancellationToken>(), null))
            .ReturnsAsync((MemberWorkoutPlan?)null);

        // Act
        Func<Task> act = async () => await _sut.CancelAssignmentAsync(1, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task CancelAssignmentAsync_ShouldThrowForbidden_WhenGymIdDoesNotMatchAndNotSuperAdmin()
    {
        // Arrange
        var assignment = new MemberWorkoutPlan { Id = 1, GymId = 99 };
        _repo.Setup(r => r.GetByIdAsync(1, true, It.IsAny<CancellationToken>(), null))
            .ReturnsAsync(assignment);

        // Act
        Func<Task> act = async () => await _sut.CancelAssignmentAsync(1, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task CancelAssignmentAsync_ShouldDeleteAssignment_WhenAuthorized()
    {
        // Arrange
        var assignment = new MemberWorkoutPlan { Id = 1, GymId = 10 };
        _repo.Setup(r => r.GetByIdAsync(1, true, It.IsAny<CancellationToken>(), null))
            .ReturnsAsync(assignment);

        // Act
        await _sut.CancelAssignmentAsync(1, CancellationToken.None);

        // Assert
        _repo.Verify(r => r.DeleteAsync(assignment, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
