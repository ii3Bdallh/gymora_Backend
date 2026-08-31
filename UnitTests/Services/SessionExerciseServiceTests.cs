using Application.DTO.Exceptions;
using Application.DTO.Model;
using Application.Interface.Repo;
using Application.Model;
using Application.Service;
using AutoMapper;
using Domain.Model;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnitTests.Helpers;
using Xunit;

namespace UnitTests.Services;

public class SessionExerciseServiceTests
{
    private readonly Mock<ISessionExerciseRepo> _repo;
    private readonly Mock<ISessionRepo> _sessionRepo;
    private readonly Mock<IExerciseRepo> _exerciseRepo;
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<IMapper> _mapper;
    private readonly CurrentUser _currentUser;
    private readonly SessionExerciseService _sut;

    public SessionExerciseServiceTests()
    {
        _repo = new Mock<ISessionExerciseRepo>();
        _sessionRepo = new Mock<ISessionRepo>();
        _exerciseRepo = new Mock<IExerciseRepo>();
        _unitOfWork = new Mock<IUnitOfWork>();
        _mapper = new Mock<IMapper>();
        _currentUser = Mocks.SuperAdminCurrentUser(userId: 1);

        _sut = new SessionExerciseService(
            _repo.Object,
            _sessionRepo.Object,
            _exerciseRepo.Object,
            _unitOfWork.Object,
            _mapper.Object,
            _currentUser
        );
    }

    [Fact]
    public async Task AddRangeAsync_ShouldThrowNotFound_WhenSessionDoesNotExist()
    {
        // Arrange
        var dtos = new List<SessionExerciseCDTO>
        {
            new() { SessionId = 99, ExerciseName = "Squat" }
        };

        _sessionRepo.Setup(r => r.GetByIdAsync(99, false, It.IsAny<CancellationToken>(), null))
            .ReturnsAsync((Session?)null);

        // Act
        Func<Task> act = async () => await _sut.AddRangeAsync(dtos, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task AddRangeAsync_ShouldThrowForbidden_WhenUserDoesNotOwnSessionAndNotSuperAdmin()
    {
        // Arrange
        _currentUser.PlatformRole = null;
        _currentUser.UserId = 5;

        var session = new Session { Id = 1, CreatedById = 10 };
        var dtos = new List<SessionExerciseCDTO>
        {
            new() { SessionId = 1, ExerciseName = "Squat" }
        };

        _sessionRepo.Setup(r => r.GetByIdAsync(1, false, It.IsAny<CancellationToken>(), null))
            .ReturnsAsync(session);

        // Act
        Func<Task> act = async () => await _sut.AddRangeAsync(dtos, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ForbiddenException>();
    }
}
