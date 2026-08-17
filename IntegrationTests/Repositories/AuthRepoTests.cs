using Application.DTO.Auth;
using Application.DTO.Model;
using Application.Interface.Repo.Shared;
using Application.Interface.Repo;
using Domain.Model;
using Domain.Model.Auth;
using Domain.Options;
using FluentAssertions;
using Infrastructure.Persistence;
using Infrastructure.Repo;
using Infrastructure.Utils;
using IntegrationTests.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using MassTransit;

namespace IntegrationTests.Repositories
{
    public class AuthRepoTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly JwtProvider _jwtProvider;
        private readonly Mock<IConfiguration> _configMock;
        private readonly Mock<IPublishEndpoint> _publishEndpointMock;
        private readonly Mock<IGymAccessRepo> _gymAccessRepoMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly string _dbName;
        private readonly AuthRepo _sut;

        public AuthRepoTests()
        {
            _dbName = Guid.NewGuid().ToString();
            _context = InMemoryDbContextFactory.Create(_dbName);

            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(
                userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);

            // Setup JwtProvider
            var jwtOptions = new JwtOptions
            {
                SecretKey = "super_secret_key_for_testing_1234567890!",
                Issuer = "TestIssuer",
                Audience = "TestAudience",
                AccessTokenExpirationMinutes = 15
            };
            var jwtLoggerMock = new Mock<ILogger<JwtProvider>>();
            _configMock = new Mock<IConfiguration>();
            _jwtProvider = new JwtProvider(jwtOptions, _configMock.Object, jwtLoggerMock.Object);

            _publishEndpointMock = new Mock<IPublishEndpoint>();
            _gymAccessRepoMock = new Mock<IGymAccessRepo>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .Returns((CancellationToken ct) => _context.SaveChangesAsync(ct));

            _configMock.Setup(c => c["Jwt:RefreshTokenExpirationInDays"]).Returns("7");

            _sut = new AuthRepo(
                _userManagerMock.Object,
                _jwtProvider,
                _context,
                _configMock.Object,
                _publishEndpointMock.Object,
                _gymAccessRepoMock.Object,
                _unitOfWorkMock.Object
            );
        }

        [Fact]
        public async Task ProcessTokenRotation_ShouldThrowUnauthorizedException_WhenUserMismatch()
        {
            // Arrange
            var user1 = new ApplicationUser { Id = 1, Email = "user1@test.com", PersonName = "User One" };
            var user2 = new ApplicationUser { Id = 2, Email = "user2@test.com", PersonName = "User Two" };

            // Generate an access token for User 1
            var (accessTokenUser1, _) = _jwtProvider.GenerateToken(
                user1, 
                new List<string> { "User" }, 
                new RefreshToken { CurrentGymId = 0 }
            );

            // Generate a refresh token owned by User 2
            var (plainRefreshToken, tokenHash) = _jwtProvider.GenerateRefreshToken();
            var refreshTokenEntity = new RefreshToken
            {
                Id = 10,
                Token = tokenHash,
                UserId = user2.Id,
                ExpirationAt = DateTime.UtcNow.AddDays(7),
                User = user2
            };

            _context.RefreshTokens.Add(refreshTokenEntity);
            await _context.SaveChangesAsync();

            // Act
            Func<Task> act = async () => await _sut.RefreshTokenAsync(plainRefreshToken, accessTokenUser1, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<Application.DTO.Exceptions.UnauthorizedException>()
                .WithMessage("Access token and refresh token user mismatch.");
        }

        [Fact]
        public async Task ProcessTokenRotation_ShouldClearGymContext_WhenUserNoLongerHasAccessToGym()
        {
            // Arrange
            var user = new ApplicationUser { Id = 1, Email = "user1@test.com", PersonName = "User One" };

            // Generate an access token for User 1
            var (accessToken, _) = _jwtProvider.GenerateToken(
                user,
                new List<string> { "User" },
                new RefreshToken { CurrentGymId = 5 }
            );

            var (plainRefreshToken, tokenHash) = _jwtProvider.GenerateRefreshToken();
            var refreshTokenEntity = new RefreshToken
            {
                Id = 11,
                Token = tokenHash,
                UserId = user.Id,
                CurrentGymId = 5,
                CurrentGymPeopleId = 10,
                GymRole = "Coach",
                ExpirationAt = DateTime.UtcNow.AddDays(7),
                User = user
            };

            _context.RefreshTokens.Add(refreshTokenEntity);
            await _context.SaveChangesAsync();

            // Mock user not having access to the gym anymore
            _gymAccessRepoMock.Setup(r => r.GetGymAccessAsync(user.Id, 5, It.IsAny<CancellationToken>()))
                .ReturnsAsync((MyGymDto?)null);

            _userManagerMock.Setup(u => u.GetRolesAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(new List<string> { "User" });

            // Act
            var res = await _sut.RefreshTokenAsync(plainRefreshToken, accessToken, CancellationToken.None);

            // Assert
            res.CurrentGym.Should().BeNull();

            // Check db
            var newRefreshToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(r => r.Token != tokenHash && r.UserId == user.Id);

            newRefreshToken.Should().NotBeNull();
            newRefreshToken!.CurrentGymId.Should().Be(0);
            newRefreshToken.CurrentGymPeopleId.Should().Be(0);
            newRefreshToken.GymRole.Should().BeNull();
        }

        [Fact]
        public async Task ProcessTokenRotation_ShouldUpdateGymContext_WhenUserHasUpdatedGymContext()
        {
            // Arrange
            var user = new ApplicationUser { Id = 1, Email = "user1@test.com", PersonName = "User One" };

            // Generate an access token for User 1
            var (accessToken, _) = _jwtProvider.GenerateToken(
                user,
                new List<string> { "User" },
                new RefreshToken { CurrentGymId = 5 }
            );

            var (plainRefreshToken, tokenHash) = _jwtProvider.GenerateRefreshToken();
            var refreshTokenEntity = new RefreshToken
            {
                Id = 12,
                Token = tokenHash,
                UserId = user.Id,
                CurrentGymId = 5,
                CurrentGymPeopleId = 10,
                GymRole = "Coach",
                ExpirationAt = DateTime.UtcNow.AddDays(7),
                User = user
            };

            _context.RefreshTokens.Add(refreshTokenEntity);
            await _context.SaveChangesAsync();

            // Mock user having updated access details (e.g. role changed to Owner, and GymPeopleId updated)
            var updatedGymContext = new MyGymDto
            {
                GymId = 5,
                GymPeopleId = 22,
                GymRole = "Owner",
                GymName = "Updated Gym"
            };
            _gymAccessRepoMock.Setup(r => r.GetGymAccessAsync(user.Id, 5, It.IsAny<CancellationToken>()))
                .ReturnsAsync(updatedGymContext);

            _userManagerMock.Setup(u => u.GetRolesAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(new List<string> { "User" });

            // Act
            var res = await _sut.RefreshTokenAsync(plainRefreshToken, accessToken, CancellationToken.None);

            // Assert
            res.CurrentGym.Should().NotBeNull();
            res.CurrentGym!.GymId.Should().Be("5");
            res.CurrentGym.Role.Should().Be("Owner");

            // Check db
            var newRefreshToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(r => r.Token != tokenHash && r.UserId == user.Id);

            newRefreshToken.Should().NotBeNull();
            newRefreshToken!.CurrentGymId.Should().Be(5);
            newRefreshToken.CurrentGymPeopleId.Should().Be(22);
            newRefreshToken.GymRole.Should().Be("Owner");
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
