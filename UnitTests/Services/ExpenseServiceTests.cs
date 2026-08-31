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

public class ExpenseServiceTests
{
    private readonly Mock<IExpenseRepo> _repo;
    private readonly Mock<IGymPersonRepo> _gymPersonRepo;
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<IMapper> _mapper;
    private readonly Mock<ICacheService> _cacheService;
    private readonly Mock<IPublishEndpoint> _publishEndpoint;
    private readonly CurrentUser _currentUser;
    private readonly Mock<ILogger<ExpenseService>> _logger;
    private readonly ExpenseService _sut;

    public ExpenseServiceTests()
    {
        _repo = new Mock<IExpenseRepo>();
        _gymPersonRepo = new Mock<IGymPersonRepo>();
        _unitOfWork = new Mock<IUnitOfWork>();
        _mapper = new Mock<IMapper>();
        _cacheService = new Mock<ICacheService>();
        _publishEndpoint = new Mock<IPublishEndpoint>();
        _currentUser = Mocks.DefaultCurrentUser(userId: 10, gymId: 1);
        _logger = new Mock<ILogger<ExpenseService>>();

        _sut = new ExpenseService(
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
        var entity = new Expense { Id = id, Amount = 300.0m, GymId = 1, CreatedByPersonId = 10 };
        var rDto = new ExpenseRDTO { Id = id, Amount = 300.0m };

        _repo.Setup(r => r.GetByIdDetailsAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);
        _mapper.Setup(m => m.Map<ExpenseRDTO>(entity))
            .Returns(rDto);

        // Act
        var result = await _sut.GetByIdDetailsAsync(id, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(id);
        result.Amount.Should().Be(300.0m);
        _repo.Verify(r => r.GetByIdDetailsAsync(id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AddAsync_ShouldThrowNotFoundException_WhenGymStaffDoesNotExist()
    {
        // Arrange
        int staffId = 99;
        var dto = new ExpenseCDTO
        {
            Amount = 150.0m,
            GymStaffId = staffId,
            ExpenseCategory = ExpenseCategory.Rent,
            PaymentMethod = PaymentMethod.BankTransfer,
            ExpenseDate = DateTime.UtcNow
        };

        _gymPersonRepo.Setup(g => g.GetByIdAsync(staffId, false, It.IsAny<CancellationToken>(), null))
            .ReturnsAsync((GymPerson?)null);

        // Act
        Func<Task> act = async () => await _sut.AddAsync(dto, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task AddAsync_ShouldThrowInvalidOperationException_WhenGymStaffBelongsToDifferentGym()
    {
        // Arrange
        int staffId = 5;
        var dto = new ExpenseCDTO
        {
            Amount = 150.0m,
            GymStaffId = staffId,
            ExpenseCategory = ExpenseCategory.Rent,
            PaymentMethod = PaymentMethod.BankTransfer,
            ExpenseDate = DateTime.UtcNow
        };

        var staffOtherGym = new GymPerson { Id = staffId, GymId = 999 };
        _gymPersonRepo.Setup(g => g.GetByIdAsync(staffId, false, It.IsAny<CancellationToken>(), null))
            .ReturnsAsync(staffOtherGym);

        // Act
        Func<Task> act = async () => await _sut.AddAsync(dto, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*does not belong to this gym*");
    }

    [Fact]
    public async Task AddAsync_ShouldAddExpense_WhenPayloadIsValid()
    {
        // Arrange
        int staffId = 5;
        var dto = new ExpenseCDTO
        {
            Amount = 400.0m,
            GymStaffId = staffId,
            ExpenseCategory = ExpenseCategory.Equipment,
            PaymentMethod = PaymentMethod.Instapay,
            ExpenseDate = DateTime.UtcNow
        };

        var staffSameGym = new GymPerson { Id = staffId, GymId = 1 };
        var entity = new Expense { Id = 1, Amount = 400.0m, GymId = 1, GymStaffId = staffId };
        var rDto = new ExpenseRDTO { Id = 1, Amount = 400.0m, GymStaffId = staffId };

        _gymPersonRepo.Setup(g => g.GetByIdAsync(staffId, false, It.IsAny<CancellationToken>(), null))
            .ReturnsAsync(staffSameGym);
        _mapper.Setup(m => m.Map<Expense>(dto)).Returns(entity);
        _repo.Setup(r => r.AddAsync(entity, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _mapper.Setup(m => m.Map<ExpenseRDTO>(entity)).Returns(rDto);

        // Act
        var result = await _sut.AddAsync(dto, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.Amount.Should().Be(400.0m);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
