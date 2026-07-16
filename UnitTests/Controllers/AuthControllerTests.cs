using Api.Controllers;
using Application.DTO;
using Application.DTO.Auth;
using Application.DTO.Exceptions;
using Application.Interface.Service;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using Xunit;

namespace UnitTests.Controllers;

public class AuthControllerTests
{
    private readonly Mock<IAuthService> _authService;
    private readonly AuthController _sut;

    public AuthControllerTests()
    {
        _authService = new Mock<IAuthService>();
        _sut = new AuthController(_authService.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
    }

    #region Register

    [Fact]
    public async Task Register_ShouldReturnOk_WhenDataIsValid()
    {
        var dto = new RegisterReqDto("test@test.com", "TestUser", "Password123!", "1234567890");
        _authService.Setup(a => a.RegisterAsync(dto, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _sut.Register(dto, CancellationToken.None);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<Result<string>>().Subject;
        response.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Register_ShouldReturnBadRequest_WhenModelStateIsInvalid()
    {
        _sut.ModelState.AddModelError("Email", "Invalid email");

        var result = await _sut.Register(new RegisterReqDto("", "", "", ""), CancellationToken.None);

        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        var response = badRequestResult.Value.Should().BeAssignableTo<Result<string>>().Subject;
        response.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task Register_ShouldThrow_WhenEmailAlreadyExists()
    {
        var dto = new RegisterReqDto("existing@test.com", "TestUser", "Password123!", "1234567890");
        _authService.Setup(a => a.RegisterAsync(dto, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new BadRequestException("Email already exists"));

        var act = async () => await _sut.Register(dto, CancellationToken.None);

        await act.Should().ThrowAsync<BadRequestException>();
    }

    #endregion

    #region Login

    [Fact]
    public async Task Login_ShouldReturnOk_WhenCredentialsAreValid()
    {
        var dto = new LoginReqDto("test@test.com", "Password123!");
        var loginResult = new LoginResDto(1, "test@test.com", "Test", "token123", 3600, "refresh123",
            new List<string> { "User" }, DateTime.UtcNow.AddDays(7));

        _authService.Setup(a => a.LoginAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(loginResult);

        var result = await _sut.Login(dto, CancellationToken.None);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<Result<LoginResDto>>().Subject;
        response.IsSuccess.Should().BeTrue();
        response.Data!.Token.Should().Be("token123");
    }

    [Fact]
    public async Task Login_ShouldReturnBadRequest_WhenModelStateIsInvalid()
    {
        _sut.ModelState.AddModelError("Email", "Required");

        var result = await _sut.Login(new LoginReqDto("", ""), CancellationToken.None);

        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        var response = badRequestResult.Value.Should().BeAssignableTo<Result<string>>().Subject;
        response.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task Login_ShouldThrow_WhenPasswordIsIncorrect()
    {
        var dto = new LoginReqDto("test@test.com", "WrongPassword!");
        _authService.Setup(a => a.LoginAsync(dto, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new BadRequestException("Invalid email or password"));

        var act = async () => await _sut.Login(dto, CancellationToken.None);

        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("*Invalid email or password*");
    }

    #endregion

    #region RefreshToken

    [Fact]
    public async Task RefreshToken_ShouldReturnOk_WhenTokenIsValid()
    {
        var dto = new RefreshTokenReqDto("refresh123", "access123");
        var loginResult = new LoginResDto(1, "test@test.com", "Test", "newtoken", 3600, "newrefresh",
            new List<string> { "User" }, DateTime.UtcNow.AddDays(7));

        _authService.Setup(a => a.RefreshTokenAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(loginResult);

        var result = await _sut.RefreshToken(dto, CancellationToken.None);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<Result<LoginResDto>>().Subject;
        response.IsSuccess.Should().BeTrue();
    }

    #endregion

    #region Logout

    private void SetupUserClaim(int userId = 1)
    {
        var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var user = new ClaimsPrincipal(identity);
        _sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };
    }

    [Fact]
    public async Task Logout_ShouldReturnOk_WhenLogoutFromAllDevices()
    {
        SetupUserClaim();
        var dto = new LogoutRequest { LogoutFromAllDevices = true };
        _authService.Setup(a => a.LogoutAsync(It.IsAny<LogoutRequest>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _sut.Logout(dto, CancellationToken.None);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<Result<string>>().Subject;
        response.IsSuccess.Should().BeTrue();
        response.Data.Should().Contain("all devices");
    }

    [Fact]
    public async Task Logout_ShouldReturnOk_WhenLogoutFromSingleDevice()
    {
        SetupUserClaim();
        var dto = new LogoutRequest { LogoutFromAllDevices = false, RefreshToken = "refresh123" };
        _authService.Setup(a => a.LogoutAsync(It.IsAny<LogoutRequest>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _sut.Logout(dto, CancellationToken.None);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<Result<string>>().Subject;
        response.IsSuccess.Should().BeTrue();
    }

    #endregion

    #region ConfirmEmail

    [Fact]
    public async Task ConfirmEmail_ShouldReturnOk_WhenOtpIsValid()
    {
        var dto = new ConfirmEmailRequest("test@test.com", "12345");
        _authService.Setup(a => a.ConfirmEmailAsync(dto.Email, dto.Otp, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ConfirmEmailResponseDto { Success = true, Message = "Email confirmed" });

        var result = await _sut.ConfirmEmail(dto, CancellationToken.None);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<Result<ConfirmEmailResponseDto>>().Subject;
        response.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ConfirmEmail_ShouldReturnBadRequest_WhenModelStateIsInvalid()
    {
        _sut.ModelState.AddModelError("Email", "Required");

        var result = await _sut.ConfirmEmail(new ConfirmEmailRequest("", ""), CancellationToken.None);

        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        var response = badRequestResult.Value.Should().BeAssignableTo<Result<string>>().Subject;
        response.IsSuccess.Should().BeFalse();
    }

    #endregion

    #region ForgotPassword

    [Fact]
    public async Task ForgotPassword_ShouldReturnOk_WhenEmailExists()
    {
        var dto = new ForgotPasswordRequest("test@test.com");
        _authService.Setup(a => a.ForgotPasswordAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ForgotPasswordResponseDto { Success = true, Message = "OTP sent" });

        var result = await _sut.ForgotPassword(dto, CancellationToken.None);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<Result<ForgotPasswordResponseDto>>().Subject;
        response.IsSuccess.Should().BeTrue();
    }

    #endregion

    #region VerifyOtp

    [Fact]
    public async Task VerifyOtp_ShouldReturnOk_WhenOtpIsValid()
    {
        var dto = new VerifyOtpRequest("test@test.com", "12345");
        _authService.Setup(a => a.VerifyOtpAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new VerifyOtpResponseDto { IsValid = true, Message = "Verified" });

        var result = await _sut.VerifyOtp(dto, CancellationToken.None);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<Result<VerifyOtpResponseDto>>().Subject;
        response.IsSuccess.Should().BeTrue();
    }

    #endregion

    #region ResetPassword

    [Fact]
    public async Task ResetPassword_ShouldReturnOk_WhenDataIsValid()
    {
        var dto = new ResetPasswordRequest("test@test.com", "12345", "NewPassword123!");
        _authService.Setup(a => a.ResetPasswordAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResetPasswordResponseDto { Success = true, Message = "Password reset" });

        var result = await _sut.ResetPassword(dto, CancellationToken.None);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<Result<ResetPasswordResponseDto>>().Subject;
        response.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ResetPassword_ShouldReturnBadRequest_WhenModelStateIsInvalid()
    {
        _sut.ModelState.AddModelError("Email", "Required");

        var result = await _sut.ResetPassword(new ResetPasswordRequest("", "", ""), CancellationToken.None);

        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        var response = badRequestResult.Value.Should().BeAssignableTo<Result<string>>().Subject;
        response.IsSuccess.Should().BeFalse();
    }

    #endregion

    #region GetUserProfile

    [Fact]
    public async Task GetUserProfile_ShouldReturnOk_WhenUserIsAuthenticated()
    {
        var profile = new GetUserProfileDto("test@test.com", "Test User", "1234567890", new List<string> { "User" });
        _authService.Setup(a => a.GetUserProfileAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);

        var result = await _sut.GetUserProfile(CancellationToken.None);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<Result<GetUserProfileDto>>().Subject;
        response.IsSuccess.Should().BeTrue();
        response.Data!.Email.Should().Be("test@test.com");
    }

    #endregion

    #region ChangePassword

    [Fact]
    public async Task ChangePassword_ShouldReturnOk_WhenDataIsValid()
    {
        var dto = new ChangePasswordRequest("OldPassword123!", "NewPassword123!");
        _authService.Setup(a => a.ChangePasswordAsync(dto, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _sut.ChangePassword(dto, CancellationToken.None);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<Result<string>>().Subject;
        response.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ChangePassword_ShouldReturnBadRequest_WhenModelStateIsInvalid()
    {
        _sut.ModelState.AddModelError("CurrentPassword", "Required");

        var result = await _sut.ChangePassword(new ChangePasswordRequest("", ""), CancellationToken.None);

        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        var response = badRequestResult.Value.Should().BeAssignableTo<Result<string>>().Subject;
        response.IsSuccess.Should().BeFalse();
    }

    #endregion

    #region ResendConfirmationEmail

    [Fact]
    public async Task ResendConfirmationEmail_ShouldReturnOk_WhenEmailIsValid()
    {
        var dto = new ResendConfirmationRequest("test@test.com");
        _authService.Setup(a => a.ResendConfirmationEmailAsync(dto.Email, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _sut.ResendConfirmationEmail(dto, CancellationToken.None);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<Result<string>>().Subject;
        response.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ResendConfirmationEmail_ShouldReturnBadRequest_WhenModelStateIsInvalid()
    {
        _sut.ModelState.AddModelError("Email", "Invalid");

        var result = await _sut.ResendConfirmationEmail(new ResendConfirmationRequest(""), CancellationToken.None);

        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        var response = badRequestResult.Value.Should().BeAssignableTo<Result<string>>().Subject;
        response.IsSuccess.Should().BeFalse();
    }

    #endregion

    #region LoginGoogle

    [Fact]
    public async Task LoginGoogle_ShouldReturnOk_WhenTokenIsValid()
    {
        var dto = new GoogleLoginRequest { IdToken = "valid-google-token" };
        var loginResult = new LoginResDto(1, "test@gmail.com", "Google User", "token123", 3600, "refresh123",
            new List<string> { "User" }, DateTime.UtcNow.AddDays(7));

        _authService.Setup(a => a.LoginWithGoogle(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(loginResult);

        var result = await _sut.LoginGoogle(dto);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<Result<LoginResDto>>().Subject;
        response.IsSuccess.Should().BeTrue();
        response.Data!.Email.Should().Be("test@gmail.com");
    }

    #endregion
}
