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

public class AttendanceServiceTests
{
    private readonly Mock<IAttendanceRepo> _repo;
    private readonly Mock<IGymPersonRepo> _gymPersonRepo;
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<IMapper> _mapper;
    private readonly Mock<ICacheService> _cacheService;
    private readonly Mock<IPublishEndpoint> _publishEndpoint;
    private readonly CurrentUser _currentUser;
    private readonly Mock<ILogger<AttendanceService>> _logger;
    private readonly AttendanceService _sut;

    public AttendanceServiceTests()
    {
        _repo = new Mock<IAttendanceRepo>();
        _gymPersonRepo = new Mock<IGymPersonRepo>();
        _unitOfWork = new Mock<IUnitOfWork>();
        _mapper = new Mock<IMapper>();
        _cacheService = new Mock<ICacheService>();
        _publishEndpoint = new Mock<IPublishEndpoint>();
        _currentUser = Mocks.DefaultCurrentUser(userId: 10, gymId: 1);
        _currentUser.CurrentPersonId = 99; // Receptionist/Staff checking them in
        _logger = new Mock<ILogger<AttendanceService>>();

        _publishEndpoint.Setup(x => x.Publish(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _sut = new AttendanceService(
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
    public async Task RecordCheckInAsync_ShouldThrowForbidden_WhenGymIdDoesNotMatchCurrentUser()
    {
        // Arrange
        var dto = new RecordCheckInCDTO { MemberId = 1, GymId = 2 };

        // Act
        Func<Task> act = async () => await _sut.AddAsync(dto, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task RecordCheckInAsync_ShouldThrowNotFound_WhenMemberDoesNotExist()
    {
        // Arrange
        var dto = new RecordCheckInCDTO { MemberId = 1, GymId = 1 };
        _gymPersonRepo.Setup(x => x.GetByIdAsync(dto.MemberId, It.IsAny<bool>(), It.IsAny<CancellationToken>(), null))
            .ReturnsAsync((GymPerson?)null);

        // Act
        Func<Task> act = async () => await _sut.AddAsync(dto, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task RecordCheckInAsync_ShouldThrowUnprocessableEntity_WhenMembershipIsInactive()
    {
        // Arrange
        var dto = new RecordCheckInCDTO { MemberId = 1, GymId = 1 };
        var member = new GymPerson 
        { 
            Id = 1, 
            GymId = 1, 
            PersonType = PersonType.Member, 
            AccessStatus = GymPersonAccessStatus.Active,
            MemberProfile = new GymMemberProfile { MembershipEndDate = DateTime.UtcNow.AddDays(-1) } 
        };

        _gymPersonRepo.Setup(x => x.GetByIdAsync(dto.MemberId, It.IsAny<bool>(), It.IsAny<CancellationToken>(), null))
            .ReturnsAsync(member);

        // Act
        Func<Task> act = async () => await _sut.AddAsync(dto, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnprocessableEntityException>()
            .Where(e => e.Code == "MEMBERSHIP_INACTIVE_OR_EXPIRED");
    }

    [Fact]
    public async Task RecordCheckInAsync_ShouldRecordSuccessfully_WhenMemberIsActive()
    {
        // Arrange
        var dto = new RecordCheckInCDTO { MemberId = 1, GymId = 1 };
        var member = new GymPerson 
        { 
            Id = 1, 
            GymId = 1, 
            PersonType = PersonType.Member, 
            AccessStatus = GymPersonAccessStatus.Active,
            MemberProfile = new GymMemberProfile { MembershipEndDate = DateTime.UtcNow.AddDays(1) } 
        };

        _gymPersonRepo.Setup(x => x.GetByIdAsync(dto.MemberId, It.IsAny<bool>(), It.IsAny<CancellationToken>(), null))
            .ReturnsAsync(member);

        _repo.Setup(x => x.AddAsync(It.IsAny<Attendance>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Attendance a, CancellationToken ct) => a);

        _mapper.Setup(x => x.Map<Attendance>(dto))
            .Returns(new Attendance { Id = 1, MemberId = 1, GymId = 1, CheckInTime = DateTime.UtcNow, RecordedById = 99 });

        // Act
        await _sut.AddAsync(dto, CancellationToken.None);

        // Assert
        _repo.Verify(x => x.AddAsync(It.Is<Attendance>(a => a.MemberId == 1 && a.GymId == 1 && a.RecordedById == 99), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
