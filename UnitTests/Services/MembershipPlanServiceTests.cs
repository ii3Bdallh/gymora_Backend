using Application.Cache;
using Application.DTO.Exceptions;
using Application.DTO.Model;
using Application.Interface.Repo;
using Application.Model;
using Application.Service;
using Domain.Events;
using Domain.Model;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using UnitTests.Helpers;
using Xunit;

namespace UnitTests.Services;

public class MembershipPlanServiceTests
{
    private readonly Mock<IMembershipPlanRepo> _repo;
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<IMapper> _mapper;
    private readonly Mock<ICacheService> _cacheService;
    private readonly Mock<IPublishEndpoint> _publishEndpoint;
    private readonly CurrentUser _currentUser;
    private readonly Mock<ILogger<MembershipPlanService>> _logger;
    private readonly MembershipPlanService _sut;

    public MembershipPlanServiceTests()
    {
        _repo = new Mock<IMembershipPlanRepo>();
        _unitOfWork = Mocks.UnitOfWork();
        _mapper = Mocks.Mapper();
        _cacheService = Mocks.CacheService();
        _publishEndpoint = Mocks.PublishEndpoint();
        _currentUser = new CurrentUser
        {
            UserId = 1,
            CurrentPersonId = 1,
            CurrentGymId = 1,
            PlatformRole = AppRole.SuperAdmin,
            IsAuthenticated = true
        };
        _logger = new Mock<ILogger<MembershipPlanService>>();

        _sut = new MembershipPlanService(
            _repo.Object,
            _unitOfWork.Object,
            _mapper.Object,
            _cacheService.Object,
            _publishEndpoint.Object,
            _currentUser,
            _logger.Object);
    }

    #region AddAsync

    [Fact]
    public async Task AddAsync_ShouldReturnCreatedEntity_WhenDataIsValid()
    {
        var entity = new MembershipPlan { Id = 1, Name = "Monthly Gold", DurationDays = 30, Price = 100m, GymId = 1 };
        var cdto = new MembershipPlanCDTO { Name = "Monthly Gold", DurationDays = 30, Price = 100m };
        var rDto = new MembershipPlanRDTO { Id = 1, Name = "Monthly Gold", DurationDays = 30, Price = 100m };

        _repo.Setup(r => r.AddAsync(It.IsAny<MembershipPlan>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);
        _mapper.Setup(m => m.Map<MembershipPlan>(It.IsAny<MembershipPlanCDTO>()))
            .Returns(entity);
        _mapper.Setup(m => m.Map<MembershipPlanRDTO>(It.IsAny<MembershipPlan>()))
            .Returns(rDto);
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        _publishEndpoint.Setup(p => p.Publish(It.IsAny<EntityChangedEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _sut.AddAsync(cdto);

        result.Should().NotBeNull();
        result.Name.Should().Be("Monthly Gold");
        _repo.Verify(r => r.AddAsync(It.IsAny<MembershipPlan>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region GetByIdDetailsAsync

    [Fact]
    public async Task GetByIdDetailsAsync_ShouldCallRepoGetByIdDetailsAsync_AndReturnDto()
    {
        var entity = new MembershipPlan { Id = 1, Name = "Monthly Gold", DurationDays = 30, Price = 100m, GymId = 1 };
        var rDto = new MembershipPlanRDTO { Id = 1, Name = "Monthly Gold", DurationDays = 30, Price = 100m };

        _repo.Setup(r => r.GetByIdDetailsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);
        _mapper.Setup(m => m.Map<MembershipPlanRDTO>(entity))
            .Returns(rDto);

        var result = await _sut.GetByIdDetailsAsync(1);

        result.Should().NotBeNull();
        result.Name.Should().Be("Monthly Gold");
        _repo.Verify(r => r.GetByIdDetailsAsync(1, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetByIdDetailsAsync_ShouldThrowNotFoundException_WhenEntityDoesNotExist()
    {
        _repo.Setup(r => r.GetByIdDetailsAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((MembershipPlan?)null);

        var act = async () => await _sut.GetByIdDetailsAsync(999);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region UpdateAsync

    [Fact]
    public async Task UpdateAsync_ShouldReturnUpdatedEntity_WhenValid()
    {
        var entity = new MembershipPlan { Id = 1, Name = "Gold", DurationDays = 30, Price = 100m, GymId = 1, CreatedByPersonId = 1 };
        var udto = new MembershipPlanUDTO { Name = "Updated Gold", DurationDays = 60, Price = 180m };
        var rDto = new MembershipPlanRDTO { Id = 1, Name = "Updated Gold", DurationDays = 60, Price = 180m };

        _repo.Setup(r => r.GetByIdAsync(1, true, It.IsAny<CancellationToken>(), null))
            .ReturnsAsync(entity);
        _repo.Setup(r => r.UpdateAsync(It.IsAny<MembershipPlan>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);
        _mapper.Setup(m => m.Map(udto, entity))
            .Returns(entity);
        _mapper.Setup(m => m.Map<MembershipPlanRDTO>(entity))
            .Returns(rDto);
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await _sut.UpdateAsync(1, udto);

        result.Should().NotBeNull();
        result.Name.Should().Be("Updated Gold");
    }

    #endregion

    #region DeleteAsync

    [Fact]
    public async Task DeleteAsync_ShouldReturnDeletedEntity_WhenExists()
    {
        var entity = new MembershipPlan { Id = 1, Name = "Monthly Gold", GymId = 1 };
        var rDto = new MembershipPlanRDTO { Id = 1, Name = "Monthly Gold" };

        _repo.Setup(r => r.GetByIdAsync(1, true, It.IsAny<CancellationToken>(), null))
            .ReturnsAsync(entity);
        _repo.Setup(r => r.DeleteAsync(entity, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);
        _mapper.Setup(m => m.Map<MembershipPlanRDTO>(entity))
            .Returns(rDto);
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await _sut.DeleteAsync(1);

        result.Should().NotBeNull();
        _repo.Verify(r => r.DeleteAsync(entity, It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion
}
