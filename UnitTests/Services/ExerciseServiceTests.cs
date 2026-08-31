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
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnitTests.Helpers;
using Xunit;

namespace UnitTests.Services;

public class ExerciseServiceTests
{
    private readonly Mock<IExerciseRepo> _repo;
    private readonly Mock<ISessionExerciseRepo> _sessionExerciseRepo;
    private readonly Mock<IUserWorkoutBlockRepo> _blockRepo;
    private readonly Mock<ICurrentPlanService> _currentPlanService;
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<IMapper> _mapper;
    private readonly Mock<ICacheService> _cacheService;
    private readonly Mock<IPublishEndpoint> _publishEndpoint;
    private readonly CurrentUser _currentUser;
    private readonly Mock<IStorageService> _storageService;
    private readonly Mock<ILogger<ExerciseService>> _logger;
    private readonly ExerciseService _sut;

    public ExerciseServiceTests()
    {
        _repo = new Mock<IExerciseRepo>();
        _sessionExerciseRepo = new Mock<ISessionExerciseRepo>();
        _blockRepo = new Mock<IUserWorkoutBlockRepo>();
        _currentPlanService = new Mock<ICurrentPlanService>();
        _unitOfWork = new Mock<IUnitOfWork>();
        _mapper = new Mock<IMapper>();
        _cacheService = new Mock<ICacheService>();
        _publishEndpoint = new Mock<IPublishEndpoint>();
        _currentUser = Mocks.SuperAdminCurrentUser(userId: 1);
        _storageService = new Mock<IStorageService>();
        _logger = new Mock<ILogger<ExerciseService>>();

        _publishEndpoint.Setup(x => x.Publish(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _sut = new ExerciseService(
            _repo.Object,
            _sessionExerciseRepo.Object,
            _blockRepo.Object,
            _currentPlanService.Object,
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
        _currentUser.PlatformRole = null; // Regular user

        // Act
        Func<Task> act = async () => await _sut.ApproveAsync(1, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task ApproveAsync_ShouldThrowNotFound_WhenExerciseDoesNotExist()
    {
        // Arrange
        _currentUser.PlatformRole = AppRole.SuperAdmin;
        _repo.Setup(r => r.GetByIdAsync(1, true, It.IsAny<CancellationToken>(), null))
            .ReturnsAsync((Exercise?)null);

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
        var exercise = new Exercise { Id = 1, Name = "Deadlift", IsApproved = false };
        _repo.Setup(r => r.GetByIdAsync(1, true, It.IsAny<CancellationToken>(), null))
            .ReturnsAsync(exercise);

        // Act
        await _sut.ApproveAsync(1, CancellationToken.None);

        // Assert
        exercise.IsApproved.Should().BeTrue();
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
