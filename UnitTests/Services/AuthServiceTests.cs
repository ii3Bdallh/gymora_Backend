using Application.DTO;
using Application.DTO.Auth;
using Application.DTO.Exceptions;
using Application.Interface.Repo.Shared;
using Application.Interface.Service.Shared;
using Application.Service;
using Domain.Model;
using Domain.Model.Auth;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Security.Claims;
using UnitTests.Helpers;
using Xunit;

namespace UnitTests.Services;

public class AuthServiceTests
{
    private readonly Mock<IAuthRepo> _authRepo;
    private readonly Mock<IHttpContextAccessor> _httpContextAccessor;
    private readonly Mock<IEmailService> _emailService;
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        _authRepo = new Mock<IAuthRepo>();
        _httpContextAccessor = new Mock<IHttpContextAccessor>();
        _emailService = new Mock<IEmailService>();
        _sut = new AuthService(_authRepo.Object, _httpContextAccessor.Object, _emailService.Object);
    }

    #region RegisterAsync

    [Fact]
    public async Task RegisterAsync_ShouldCallRepo_WhenDataIsValid()
    {
        var dto = new RegisterReqDto("test@test.com", "TestUser", "Password123!", "1234567890");

        _authRepo.Setup(r => r.RegisterAsync(dto, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _authRepo.Setup(r => r.GetUserByEmailAsync("test@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApplicationUser { Email = "test@test.com" });
        _authRepo.Setup(r => r.GenerateEmailConfirmationOtpAsync(It.IsAny<ApplicationUser>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("12345");
        _emailService.Setup(e => e.SendEmailAsync("test@test.com", It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        await _sut.RegisterAsync(dto, CancellationToken.None);

        _authRepo.Verify(r => r.RegisterAsync(dto, It.IsAny<CancellationToken>()), Times.Once);
        _emailService.Verify(e => e.SendEmailAsync("test@test.com", It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    #endregion

    #region LoginAsync

    [Fact]
    public async Task LoginAsync_ShouldReturnLoginResult_WhenCredentialsAreValid()
    {
        var dto = new LoginReqDto("test@test.com", "Password123!");
        var result = new LoginResDto(1, "test@test.com", "TestUser", "token123", 3600, "refresh123",
            new List<string> { "User" }, DateTime.UtcNow.AddDays(7));

        _authRepo.Setup(r => r.LoginAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        var response = await _sut.LoginAsync(dto, CancellationToken.None);

        response.Should().NotBeNull();
        response.Token.Should().Be("token123");
        response.Email.Should().Be("test@test.com");
    }

    [Fact]
    public async Task LoginAsync_ShouldThrow_WhenCredentialsAreInvalid()
    {
        var dto = new LoginReqDto("wrong@test.com", "WrongPassword!");

        _authRepo.Setup(r => r.LoginAsync(dto, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new BadRequestException("Invalid email or password"));

        var act = async () => await _sut.LoginAsync(dto, CancellationToken.None);

        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("*Invalid email or password*");
    }

    #endregion

    #region RefreshTokenAsync

    [Fact]
    public async Task RefreshTokenAsync_ShouldReturnNewTokens_WhenRefreshTokenIsValid()
    {
        var dto = new RefreshTokenReqDto("refresh123", "access123");
        var result = new LoginResDto(1, "test@test.com", "TestUser", "newtoken", 3600, "newrefresh",
            new List<string> { "User" }, DateTime.UtcNow.AddDays(7));

        _authRepo.Setup(r => r.RefreshTokenAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        var response = await _sut.RefreshTokenAsync(dto, CancellationToken.None);

        response.Should().NotBeNull();
        response.Token.Should().Be("newtoken");
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

    [Fact]
    public async Task ConfirmEmailAsync_ShouldThrow_WhenOtpIsInvalid()
    {
        _authRepo.Setup(r => r.ConfirmEmailAsync("test@test.com", "00000", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new BadRequestException("Invalid or expired code"));

        var act = async () => await _sut.ConfirmEmailAsync("test@test.com", "00000", CancellationToken.None);

        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("*Invalid or expired*");
    }

    #endregion

    #region ForgotPasswordAsync

    [Fact]
    public async Task ForgotPasswordAsync_ShouldReturnSuccess_WhenEmailExists()
    {
        var dto = new ForgotPasswordRequest("test@test.com");
        _authRepo.Setup(r => r.GeneratePasswordResetOtpAsync("test@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync("54321");
        _emailService.Setup(e => e.SendEmailAsync("test@test.com", It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        var result = await _sut.ForgotPasswordAsync(dto, CancellationToken.None);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        _emailService.Verify(e => e.SendEmailAsync("test@test.com", It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    #endregion

    #region VerifyOtpAsync

    [Fact]
    public async Task VerifyOtpAsync_ShouldReturnSuccess_WhenOtpIsValid()
    {
        var dto = new VerifyOtpRequest("test@test.com", "12345");
        _authRepo.Setup(r => r.VerifyOtpAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _sut.VerifyOtpAsync(dto, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region ResetPasswordAsync

    [Fact]
    public async Task ResetPasswordAsync_ShouldReturnSuccess_WhenOtpAndNewPasswordAreValid()
    {
        var dto = new ResetPasswordRequest("test@test.com", "12345", "NewPassword123!");
        _authRepo.Setup(r => r.VerifyOtpAsync(It.IsAny<VerifyOtpRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _authRepo.Setup(r => r.ResetPasswordAsync(dto, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _sut.ResetPasswordAsync(dto, CancellationToken.None);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task ResetPasswordAsync_ShouldThrow_WhenOtpIsInvalid()
    {
        var dto = new ResetPasswordRequest("test@test.com", "00000", "NewPassword123!");
        _authRepo.Setup(r => r.VerifyOtpAsync(It.IsAny<VerifyOtpRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new BadRequestException("Invalid or expired OTP"));

        var act = async () => await _sut.ResetPasswordAsync(dto, CancellationToken.None);

        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("*Invalid or expired OTP*");
    }

    #endregion

    #region LoginWithGoogle

    [Fact]
    public async Task LoginWithGoogle_ShouldReturnLoginResult_WhenTokenIsValid()
    {
        var dto = new GoogleLoginRequest { IdToken = "valid-google-token" };
        var result = new LoginResDto(1, "test@gmail.com", "Google User", "token123", 3600, "refresh123",
            new List<string> { "User" }, DateTime.UtcNow.AddDays(7));

        _authRepo.Setup(r => r.LoginWithGoogle(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        var response = await _sut.LoginWithGoogle(dto, CancellationToken.None);

        response.Should().NotBeNull();
        response.Email.Should().Be("test@gmail.com");
    }

    #endregion

    #region GetUserProfileAsync

    [Fact]
    public async Task GetUserProfileAsync_ShouldReturnProfile_WhenUserIsAuthenticated()
    {
        var claims = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new(ClaimTypes.NameIdentifier, "1")
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

    [Fact]
    public async Task GetUserProfileAsync_ShouldThrow_WhenNoHttpContext()
    {
        _httpContextAccessor.Setup(h => h.HttpContext).Returns((HttpContext?)null);

        var act = async () => await _sut.GetUserProfileAsync(CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*No HTTP context*");
    }

    [Fact]
    public async Task GetUserProfileAsync_ShouldThrow_WhenNoNameIdentifierClaim()
    {
        var claims = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { }));
        var httpContext = new DefaultHttpContext { User = claims };
        _httpContextAccessor.Setup(h => h.HttpContext).Returns(httpContext);

        var act = async () => await _sut.GetUserProfileAsync(CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*not authenticated*");
    }

    #endregion

    #region ChangePasswordAsync

    [Fact]
    public async Task ChangePasswordAsync_ShouldCallRepo_WhenDataIsValid()
    {
        var dto = new ChangePasswordRequest("OldPassword123!", "NewPassword123!");
        var claims = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new(ClaimTypes.NameIdentifier, "1")
        }));
        var httpContext = new DefaultHttpContext { User = claims };
        _httpContextAccessor.Setup(h => h.HttpContext).Returns(httpContext);
        _authRepo.Setup(r => r.ChangePasswordAsync(1, dto.CurrentPassword, dto.NewPassword, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _sut.ChangePasswordAsync(dto, CancellationToken.None);

        _authRepo.Verify(r => r.ChangePasswordAsync(1, dto.CurrentPassword, dto.NewPassword, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ChangePasswordAsync_ShouldThrow_WhenNoHttpContext()
    {
        _httpContextAccessor.Setup(h => h.HttpContext).Returns((HttpContext?)null);

        var dto = new ChangePasswordRequest("OldPassword123!", "NewPassword123!");
        var act = async () => await _sut.ChangePasswordAsync(dto, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    #endregion

    #region LogoutAsync

    [Fact]
    public async Task LogoutAsync_ShouldCallRepo_WhenRequestIsValid()
    {
        var dto = new LogoutRequest { UserId = 1, RefreshToken = "refresh123", LogoutFromAllDevices = false };
        _authRepo.Setup(r => r.LogoutAsync(dto, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _sut.LogoutAsync(dto, CancellationToken.None);

        _authRepo.Verify(r => r.LogoutAsync(dto, It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region ResendConfirmationEmailAsync

    [Fact]
    public async Task ResendConfirmationEmailAsync_ShouldCallRepo_WhenEmailExists()
    {
        _authRepo.Setup(r => r.ResendConfirmationEmailAsync("test@test.com", It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _sut.ResendConfirmationEmailAsync("test@test.com", CancellationToken.None);

        _authRepo.Verify(r => r.ResendConfirmationEmailAsync("test@test.com", It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion
}
