using Application.DTO.Exceptions;
using Application.DTO.Model;
using Application.DTO.Pagintion;
using Application.Interface.Repo;
using Application.Interface.Service.Shared;
using Application.Model;
using Application.Service;
using AutoMapper;
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

public class BodyMeasurementServiceTests
{
    private readonly Mock<IBodyMeasurementRepo> _repo;
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<IMapper> _mapper;
    private readonly Mock<ICacheService> _cacheService;
    private readonly Mock<IPublishEndpoint> _publishEndpoint;
    private readonly CurrentUser _currentUser;
    private readonly Mock<ILogger<BodyMeasurementService>> _logger;
    private readonly BodyMeasurementService _sut;

    public BodyMeasurementServiceTests()
    {
        _repo = new Mock<IBodyMeasurementRepo>();
        _unitOfWork = new Mock<IUnitOfWork>();
        _mapper = new Mock<IMapper>();
        _cacheService = new Mock<ICacheService>();
        _publishEndpoint = new Mock<IPublishEndpoint>();
        _currentUser = Mocks.DefaultCurrentUser(userId: 10, gymId: 1);
        _logger = new Mock<ILogger<BodyMeasurementService>>();

        _sut = new BodyMeasurementService(
            _repo.Object,
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
        var entity = new BodyMeasurement { Id = id, WeightKg = 75.5m, HeightCm = 180m, CreatedById = 10 };
        var rDto = new BodyMeasurementRDTO { Id = id, WeightKg = 75.5m, HeightCm = 180m, CreatedById = 10 };

        _repo.Setup(r => r.GetByIdDetailsAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);
        _mapper.Setup(m => m.Map<BodyMeasurementRDTO>(entity))
            .Returns(rDto);

        // Act
        var result = await _sut.GetByIdDetailsAsync(id, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(id);
        result.WeightKg.Should().Be(75.5m);
        _repo.Verify(r => r.GetByIdDetailsAsync(id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AddAsync_ShouldAddEntityAndSaveChanges_WhenDtoIsValid()
    {
        // Arrange
        var dto = new BodyMeasurementCDTO { WeightKg = 80.0m, HeightCm = 175m, Notes = "Baseline" };
        var entity = new BodyMeasurement { Id = 1, WeightKg = 80.0m, HeightCm = 175m, Notes = "Baseline", CreatedById = 10 };
        var rDto = new BodyMeasurementRDTO { Id = 1, WeightKg = 80.0m, HeightCm = 175m, Notes = "Baseline", CreatedById = 10 };

        _mapper.Setup(m => m.Map<BodyMeasurement>(dto)).Returns(entity);
        _repo.Setup(r => r.AddAsync(entity, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _mapper.Setup(m => m.Map<BodyMeasurementRDTO>(entity)).Returns(rDto);

        // Act
        var result = await _sut.AddAsync(dto, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.WeightKg.Should().Be(80.0m);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowNotFound_WhenEntityDoesNotExist()
    {
        // Arrange
        int id = 99;
        var dto = new BodyMeasurementUDTO { WeightKg = 85.0m };
        _repo.Setup(r => r.GetByIdAsync(id, true, It.IsAny<CancellationToken>(), null))
            .ReturnsAsync((BodyMeasurement?)null);

        // Act
        Func<Task> act = async () => await _sut.UpdateAsync(id, dto, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteEntity_WhenEntityExists()
    {
        // Arrange
        int id = 1;
        var entity = new BodyMeasurement { Id = id, WeightKg = 70.0m, CreatedById = 10 };
        var rDto = new BodyMeasurementRDTO { Id = id, WeightKg = 70.0m, CreatedById = 10 };

        _repo.Setup(r => r.GetByIdAsync(id, true, It.IsAny<CancellationToken>(), null))
            .ReturnsAsync(entity);
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _mapper.Setup(m => m.Map<BodyMeasurementRDTO>(entity)).Returns(rDto);

        // Act
        var result = await _sut.DeleteAsync(id, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(id);
    }
}
