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

public class RevenueServiceTests
{
    private readonly Mock<IRevenueRepo> _repo;
    private readonly Mock<IGymPersonRepo> _gymPersonRepo;
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<IMapper> _mapper;
    private readonly Mock<ICacheService> _cacheService;
    private readonly Mock<IPublishEndpoint> _publishEndpoint;
    private readonly CurrentUser _currentUser;
    private readonly Mock<ILogger<RevenueService>> _logger;
    private readonly RevenueService _sut;

    public RevenueServiceTests()
    {
        _repo = new Mock<IRevenueRepo>();
        _gymPersonRepo = new Mock<IGymPersonRepo>();
        _unitOfWork = new Mock<IUnitOfWork>();
        _mapper = new Mock<IMapper>();
        _cacheService = new Mock<ICacheService>();
        _publishEndpoint = new Mock<IPublishEndpoint>();
        _currentUser = Mocks.DefaultCurrentUser(userId: 10, gymId: 1);
        _logger = new Mock<ILogger<RevenueService>>();

        _sut = new RevenueService(
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
    public async Task GetByIdDetailsAsync_ShouldReturnMappedDto_WhenEntityExists()
    {
        // Arrange
        int id = 1;
        var entity = new Revenue { Id = id, Amount = 150.0m, GymId = 1, CreatedByPersonId = 10 };
        var rDto = new RevenueRDTO { Id = id, Amount = 150.0m };

        _repo.Setup(r => r.GetByIdDetailsAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);
        _mapper.Setup(m => m.Map<RevenueRDTO>(entity))
            .Returns(rDto);

        // Act
        var result = await _sut.GetByIdDetailsAsync(id, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(id);
        result.Amount.Should().Be(150.0m);
        _repo.Verify(r => r.GetByIdDetailsAsync(id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AddAsync_ShouldThrowNotFoundException_WhenGymMemberDoesNotExist()
    {
        // Arrange
        int memberId = 99;
        var dto = new RevenueCDTO
        {
            Amount = 100.0m,
            GymMemberId = memberId,
            RevenueCategory = RevenueCategory.Membership,
            PaymentMethod = PaymentMethod.Cash,
            RevenueDate = DateTime.UtcNow
        };

        _gymPersonRepo.Setup(g => g.GetByIdAsync(memberId, false, It.IsAny<CancellationToken>(), null))
            .ReturnsAsync((GymPerson?)null);

        // Act
        Func<Task> act = async () => await _sut.AddAsync(dto, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task AddAsync_ShouldThrowInvalidOperationException_WhenGymMemberBelongsToDifferentGym()
    {
        // Arrange
        int memberId = 5;
        var dto = new RevenueCDTO
        {
            Amount = 100.0m,
            GymMemberId = memberId,
            RevenueCategory = RevenueCategory.Membership,
            PaymentMethod = PaymentMethod.Cash,
            RevenueDate = DateTime.UtcNow
        };

        var memberOtherGym = new GymPerson { Id = memberId, GymId = 999 };
        _gymPersonRepo.Setup(g => g.GetByIdAsync(memberId, false, It.IsAny<CancellationToken>(), null))
            .ReturnsAsync(memberOtherGym);

        // Act
        Func<Task> act = async () => await _sut.AddAsync(dto, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*does not belong to this gym*");
    }

    [Fact]
    public async Task AddAsync_ShouldAddRevenue_WhenPayloadIsValid()
    {
        // Arrange
        int memberId = 5;
        var dto = new RevenueCDTO
        {
            Amount = 200.0m,
            GymMemberId = memberId,
            RevenueCategory = RevenueCategory.Membership,
            PaymentMethod = PaymentMethod.Instapay,
            RevenueDate = DateTime.UtcNow
        };

        var memberSameGym = new GymPerson { Id = memberId, GymId = 1 };
        var entity = new Revenue { Id = 1, Amount = 200.0m, GymId = 1, GymMemberId = memberId };
        var rDto = new RevenueRDTO { Id = 1, Amount = 200.0m, GymMemberId = memberId };

        _gymPersonRepo.Setup(g => g.GetByIdAsync(memberId, false, It.IsAny<CancellationToken>(), null))
            .ReturnsAsync(memberSameGym);
        _mapper.Setup(m => m.Map<Revenue>(dto)).Returns(entity);
        _repo.Setup(r => r.AddAsync(entity, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _mapper.Setup(m => m.Map<RevenueRDTO>(entity)).Returns(rDto);

        // Act
        var result = await _sut.AddAsync(dto, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.Amount.Should().Be(200.0m);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
