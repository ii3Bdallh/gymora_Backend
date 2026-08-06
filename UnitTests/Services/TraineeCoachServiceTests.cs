using Application.DTO.Exceptions;
using Application.DTO.Model;
using Application.DTO.Pagintion;
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
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnitTests.Helpers;
using Xunit;

namespace UnitTests.Services;

public class CoachAssignmentServiceTests
{
    private readonly Mock<ICoachAssignmentRepo> _repo;
    private readonly Mock<IGymPersonRepo> _gymPersonRepo;
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<IMapper> _mapper;
    private readonly Mock<ICacheService> _cacheService;
    private readonly Mock<IPublishEndpoint> _publishEndpoint;
    private readonly CurrentUser _currentUser;
    private readonly Mock<ILogger<CoachAssignmentService>> _logger;
    private readonly CoachAssignmentService _sut;

    public CoachAssignmentServiceTests()
    {
        _repo = new Mock<ICoachAssignmentRepo>();
        _gymPersonRepo = new Mock<IGymPersonRepo>();
        _unitOfWork = new Mock<IUnitOfWork>();
        _mapper = new Mock<IMapper>();
        _cacheService = new Mock<ICacheService>();
        _publishEndpoint = new Mock<IPublishEndpoint>();
        _currentUser = Mocks.DefaultCurrentUser(userId: 10, gymId: 1);
        _logger = new Mock<ILogger<CoachAssignmentService>>();

        _sut = new CoachAssignmentService(
            _repo.Object,
            _gymPersonRepo.Object,
            _unitOfWork.Object,
            _mapper.Object,
            _cacheService.Object,
            _publishEndpoint.Object,
            _currentUser,
            _logger.Object
        );
    }

    [Fact]
    public async Task AddAsync_ShouldThrowForbidden_WhenGymIdDoesNotMatchCurrentUser()
    {
        // Arrange
        var dto = new CoachAssignmentCDTO { GymId = 2, MemberId = 1, CoachStaffId = 2 };

        // Act
        Func<Task> act = async () => await _sut.AddAsync(dto, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ForbiddenException>()
            .WithMessage("You are not authorized to perform this action.");
    }

    [Fact]
    public async Task AddAsync_ShouldThrowNotFound_WhenMemberDoesNotExist()
    {
        // Arrange
        var dto = new CoachAssignmentCDTO { GymId = 1, MemberId = 99, CoachStaffId = 2 };
        _gymPersonRepo.Setup(x => x.GetByIdAsync(dto.MemberId, It.IsAny<bool>(), It.IsAny<CancellationToken>(), null))
            .ReturnsAsync((GymPerson?)null);

        // Act
        Func<Task> act = async () => await _sut.AddAsync(dto, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"Member with ID {dto.MemberId} was not found.");
    }

    [Fact]
    public async Task AddAsync_ShouldThrowNotFound_WhenCoachDoesNotExist()
    {
        // Arrange
        var dto = new CoachAssignmentCDTO { GymId = 1, MemberId = 1, CoachStaffId = 99 };
        var member = new GymPerson { Id = 1, GymId = 1, PersonType = PersonType.Member };
        
        _gymPersonRepo.Setup(x => x.GetByIdAsync(dto.MemberId, It.IsAny<bool>(), It.IsAny<CancellationToken>(), null))
            .ReturnsAsync(member);
        _gymPersonRepo.Setup(x => x.GetByIdAsync(dto.CoachStaffId, It.IsAny<bool>(), It.IsAny<CancellationToken>(), null))
            .ReturnsAsync((GymPerson?)null);

        // Act
        Func<Task> act = async () => await _sut.AddAsync(dto, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"Coach with ID {dto.CoachStaffId} was not found.");
    }

    [Fact]
    public async Task AddAsync_ShouldThrowNotFound_WhenCoachIsAMember()
    {
        // Arrange
        var dto = new CoachAssignmentCDTO { GymId = 1, MemberId = 1, CoachStaffId = 2 };
        var member = new GymPerson { Id = 1, GymId = 1, PersonType = PersonType.Member };
        var coachAsMember = new GymPerson { Id = 2, GymId = 1, PersonType = PersonType.Member };

        _gymPersonRepo.Setup(x => x.GetByIdAsync(dto.MemberId, It.IsAny<bool>(), It.IsAny<CancellationToken>(), null))
            .ReturnsAsync(member);
        _gymPersonRepo.Setup(x => x.GetByIdAsync(dto.CoachStaffId, It.IsAny<bool>(), It.IsAny<CancellationToken>(), null))
            .ReturnsAsync(coachAsMember);

        // Act
        Func<Task> act = async () => await _sut.AddAsync(dto, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"Coach with ID {dto.CoachStaffId} was not found.");
    }
}
