using Application.DTO.Model;
using Application.Interface.Repo;
using Application.Model;
using Application.Service;
using Domain.Events;
using Domain.Model;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using UnitTests.Helpers;
using Xunit;

namespace UnitTests.Services;

public class CouponRedemptionServiceTests
{
    private readonly Mock<ICouponRedemptionRepo> _repo;
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<IMapper> _mapper;
    private readonly Mock<ICacheService> _cacheService;
    private readonly Mock<IPublishEndpoint> _publishEndpoint;
    private readonly CurrentUser _currentUser;
    private readonly Mock<ILogger<CouponRedemptionService>> _logger;
    private readonly CouponRedemptionService _sut;

    public CouponRedemptionServiceTests()
    {
        _repo = Mocks.CouponRedemptionRepo();
        _unitOfWork = Mocks.UnitOfWork();
        _mapper = Mocks.Mapper();
        _cacheService = Mocks.CacheService();
        _publishEndpoint = Mocks.PublishEndpoint();
        _currentUser = Mocks.DefaultCurrentUser();
        _logger = Mocks.CouponRedemptionLogger();

        _sut = new CouponRedemptionService(
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
        var entity = TestData.CreateCouponRedemption();
        var cdto = new CouponRedemptionCDTO
        {
            CouponId = 1,
            PaymentRequestId = 1,
            DiscountAmount = 10m
        };
        var rDto = new CouponRedemptionRDTO
        {
            Id = 1,
            CouponId = 1,
            PaymentRequestId = 1,
            DiscountAmount = 10m,
            CreatedById = 1,
            CreatedOn = DateTime.UtcNow
        };

        _repo.Setup(r => r.AddAsync(It.IsAny<CouponRedemption>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);
        _mapper.Setup(m => m.Map<CouponRedemption>(It.IsAny<CouponRedemptionCDTO>()))
            .Returns(entity);
        _mapper.Setup(m => m.Map<CouponRedemptionRDTO>(It.IsAny<CouponRedemption>()))
            .Returns(rDto);
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        _publishEndpoint.Setup(p => p.Publish(It.IsAny<EntityChangedEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _sut.AddAsync(cdto);

        result.Should().NotBeNull();
        result.CouponId.Should().Be(1);
        _repo.Verify(r => r.AddAsync(It.IsAny<CouponRedemption>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region GetByIdAsync

    [Fact]
    public async Task GetByIdAsync_ShouldReturnEntity_WhenExists()
    {
        var entity = TestData.CreateCouponRedemption();
        var rDto = new CouponRedemptionRDTO
        {
            Id = 1,
            CouponId = 1,
            PaymentRequestId = 1,
            DiscountAmount = 10m
        };

        _repo.Setup(r => r.GetByIdAsync(1, true, false, It.IsAny<CancellationToken>(), null))
            .ReturnsAsync(entity);
        _cacheService.Setup(c => c.GetAsync<CouponRedemptionRDTO>(It.IsAny<string>()))
            .ReturnsAsync((CouponRedemptionRDTO?)null);
        _cacheService.Setup(c => c.SetAsync(It.IsAny<string>(), It.IsAny<CouponRedemptionRDTO>(), It.IsAny<TimeSpan?>()))
            .Returns(Task.CompletedTask);
        _mapper.Setup(m => m.Map<CouponRedemptionRDTO>(entity))
            .Returns(rDto);

        var result = await _sut.GetByIdAsync(1);

        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
    }

    #endregion

    #region GetPageAsync

    [Fact]
    public async Task GetPageAsync_ShouldReturnPaginatedResults()
    {
        var searchReq = new Application.DTO.Pagintion.PaginatedSearchReq
        {
            PageNumber = 1,
            PageSize = 10
        };

        var entities = new List<CouponRedemption> { TestData.CreateCouponRedemption() };
        var pageResult = new Application.DTO.Pagintion.PaginatedRes<CouponRedemption>
        {
            PageNumber = 1,
            PageSize = 10,
            TotalCount = 1,
            Items = entities
        };

        var rDtos = new List<CouponRedemptionRDTO>
        {
            new()
            {
                Id = 1,
                CouponId = 1,
                PaymentRequestId = 1,
                DiscountAmount = 10m
            }
        };

        var pagedRDto = new Application.DTO.Pagintion.PaginatedRes<CouponRedemptionRDTO>
        {
            PageNumber = 1,
            PageSize = 10,
            TotalCount = 1,
            Items = rDtos
        };

        _cacheService.Setup(c => c.GetAsync<Application.DTO.Pagintion.PaginatedRes<CouponRedemptionRDTO>>(It.IsAny<string>()))
            .ReturnsAsync((Application.DTO.Pagintion.PaginatedRes<CouponRedemptionRDTO>?)null);
        _cacheService.Setup(c => c.SetAsync(It.IsAny<string>(), It.IsAny<Application.DTO.Pagintion.PaginatedRes<CouponRedemptionRDTO>>(), It.IsAny<TimeSpan?>()))
            .Returns(Task.CompletedTask);
        _repo.Setup(r => r.GetPageAsync(searchReq, true, false, It.IsAny<CancellationToken>(), null))
            .ReturnsAsync(pageResult);
        _mapper.Setup(m => m.Map<List<CouponRedemptionRDTO>>(entities))
            .Returns(rDtos);

        var result = await _sut.GetPageAsync(searchReq);

        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
    }

    #endregion
}
