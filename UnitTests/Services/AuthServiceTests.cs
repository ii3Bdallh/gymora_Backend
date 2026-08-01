using Application.DTO;
using Application.DTO.Auth;
using Application.DTO.Exceptions;
using Application.Interface.Repo.Shared;
using Application.Interface.Service.Shared;
using Application.Service;
using Gymora.Contracts.Authentication;
using Domain.Model;
using Domain.Model.Auth;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using MassTransit;
using Infrastructure.Persistence;
using Application.Model;
using Infrastructure.Utils;

namespace UnitTests.Services
{
    public class AuthServiceTests
    {
        private readonly Mock<IAuthRepo> _authRepo;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessor;
        private readonly Mock<IEmailService> _emailService;
        private readonly ApplicationDbContext _context;
        private readonly CurrentUser _currentUser;
        private readonly Mock<IPublishEndpoint> _publishEndpoint;
        private readonly Mock<JwtProvider> _jwtProvider;
        private readonly Mock<UserManager<ApplicationUser>> _userManager;
        private readonly Mock<IConfiguration> _configuration;
        private readonly AuthService _sut;

        public AuthServiceTests()
        {
            _authRepo = new Mock<IAuthRepo>();
            _httpContextAccessor = new Mock<IHttpContextAccessor>();
            _emailService = new Mock<IEmailService>();
            
            _currentUser = new CurrentUser { UserId = 1, IsAuthenticated = true };
            
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new ApplicationDbContext(options);

            _publishEndpoint = new Mock<IPublishEndpoint>();

            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            _userManager = new Mock<UserManager<ApplicationUser>>(userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);
            
            _jwtProvider = new Mock<JwtProvider>(null!, null!, null!);
            _configuration = new Mock<IConfiguration>();

            _sut = new AuthService(
                _authRepo.Object,
                _httpContextAccessor.Object,
                _emailService.Object,
                _currentUser,
                _publishEndpoint.Object
            );
        }

        #region RegisterAsync

        [Fact]
        public async Task RegisterAsync_ShouldCallRepo_WhenDataIsValid()
        {
            var dto = new RegisterRequestDto 
            { 
                Email = "test@test.com", 
                FirstName = "Test", 
                LastName = "User", 
                Password = "Password123!" 
            };

            var expectedResponse = new RegisterResponseDto
            {
                AccessToken = "access_token",
                RefreshToken = "refresh_token",
                User = new UserInfoDto { UserId = "1", Email = "test@test.com", FullName = "Test User" }
            };

            _authRepo.Setup(r => r.RegisterAsync(dto, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            var response = await _sut.RegisterAsync(dto, "127.0.0.1", "TestAgent", CancellationToken.None);

            response.Should().NotBeNull();
            response.AccessToken.Should().Be("access_token");
            _authRepo.Verify(r => r.RegisterAsync(dto, "127.0.0.1", "TestAgent", It.IsAny<CancellationToken>()), Times.Once);
        }

        #endregion

        #region LoginAsync

        [Fact]
        public async Task LoginAsync_ShouldReturnLoginResult_WhenCredentialsAreValid()
        {
            var dto = new LoginRequestDto { Email = "test@test.com", Password = "Password123!" };
            var expectedResponse = new AuthResponseDto
            {
                AccessToken = "access_token",
                RefreshToken = "refresh_token",
                User = new UserInfoDto { UserId = "1", Email = "test@test.com", FullName = "Test User" }
            };

            _authRepo.Setup(r => r.LoginAsync(dto, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            var response = await _sut.LoginAsync(dto, "127.0.0.1", "TestAgent", CancellationToken.None);

            response.Should().NotBeNull();
            response.AccessToken.Should().Be("access_token");
        }

        #endregion

        #region ConfirmEmailAsync

        [Fact]
        public async Task ConfirmEmailAsync_ShouldReturnSuccess_WhenOtpIsValid()
        {
            _authRepo.Setup(r => r.ConfirmEmailAsync("test@test.com", "12345", It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await _sut.ConfirmEmailAsync("test@test.com", "12345", CancellationToken.None);

            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
        }

        #endregion

        #region GetUserProfileAsync

        [Fact]
        public async Task GetUserProfileAsync_ShouldReturnProfile_WhenUserIsAuthenticated()
        {
            var claims = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new("UserId", "1")
            }));
            var httpContext = new DefaultHttpContext { User = claims };
            _httpContextAccessor.Setup(h => h.HttpContext).Returns(httpContext);

            var profile = new GetUserProfileDto("test@test.com", "Test User", "1234567890", new List<string> { "User" });
            _authRepo.Setup(r => r.GetUserProfileAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(profile);

            var result = await _sut.GetUserProfileAsync(CancellationToken.None);

            result.Should().NotBeNull();
            result.Email.Should().Be("test@test.com");
        }

        #endregion
    }
}
