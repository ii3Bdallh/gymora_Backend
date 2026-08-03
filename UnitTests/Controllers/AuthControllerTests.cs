using Api.Controllers;
using Application.DTO;
using Application.DTO.Auth;
using Application.DTO.Exceptions;
using Application.Interface.Repo;
using Application.Interface.Service;
using Gymora.Contracts.Authentication;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using System.Collections.Generic;

namespace UnitTests.Controllers
{
    public class AuthControllerTests
    {
        private readonly Mock<IAuthService> _authService;
        private readonly Mock<IGymService> _gymService;
        private readonly AuthController _sut;

        public AuthControllerTests()
        {
            _authService = new Mock<IAuthService>();
            _gymService = new Mock<IGymService>();
            _sut = new AuthController(_authService.Object, _gymService.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };
        }

        #region Register

        [Fact]
        public async Task Register_ShouldReturnCreated_WhenDataIsValid()
        {
            var dto = new RegisterRequestDto 
            { 
                Email = "test@test.com", 
                PersonName = "Test User", 
                Password = "Password123!" 
            };
            
            var responseDto = new RegisterResponseDto
            {
         
                IsNewUser = true,
                User = new UserInfoDto { UserId = "1", Email = "test@test.com", FullName = "Test User" }
            };

            _authService.Setup(a => a.RegisterAsync(dto, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await _sut.Register(dto, CancellationToken.None);

            var createdResult = result.Should().BeOfType<ObjectResult>().Subject;
            createdResult.StatusCode.Should().Be(201);
            var response = createdResult.Value.Should().BeAssignableTo<Result<string>>().Subject;
            response.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Register_ShouldReturnUnprocessableEntity_WhenModelStateIsInvalid()
        {
            _sut.ModelState.AddModelError("Email", "Invalid email");

            var result = await _sut.Register(new RegisterRequestDto(), CancellationToken.None);

            var unprocessableResult = result.Should().BeOfType<UnprocessableEntityObjectResult>().Subject;
            var response = unprocessableResult.Value.Should().BeAssignableTo<Result<string>>().Subject;
            response.IsSuccess.Should().BeFalse();
        }

        #endregion

        #region Login

        [Fact]
        public async Task Login_ShouldReturnOk_WhenCredentialsAreValidAndHasGym()
        {
            var dto = new LoginRequestDto { Email = "test@test.com", Password = "Password123!" };
            var authResponse = new AuthResponseDto
            {
                AccessToken = "access_token",
                RefreshToken = "refresh_token",
                User = new UserInfoDto { UserId = "1", Email = "test@test.com", FullName = "Test" },
                CurrentGym = new CurrentGymDto { GymId = "1", Role = "Owner" }
            };

            _authService.Setup(a => a.LoginAsync(dto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(authResponse);

            var result = await _sut.Login(dto, CancellationToken.None);

            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var response = okResult.Value.Should().BeAssignableTo<Result<AuthResponseDto>>().Subject;
            response.IsSuccess.Should().BeTrue();
        }

        #endregion

        #region Logout

        private void SetupUserClaim(int userId = 1)
        {
            var claims = new List<Claim> { new Claim("UserId", userId.ToString()) };
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
            _authService.Setup(a => a.LogoutAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await _sut.Logout(dto, CancellationToken.None);

            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var response = okResult.Value.Should().BeAssignableTo<Result<string>>().Subject;
            response.IsSuccess.Should().BeTrue();
            response.Data.Should().Contain("all devices");
        }

        #endregion

        // #region ConfirmEmail

        // [Fact]
        // public async Task ConfirmEmail_ShouldReturnOk_WhenOtpIsValid()
        // {
        //     var dto = new ConfirmEmailRequest { Email = "test@test.com", Otp = "12345" };
        //     _authService.Setup(a => a.ConfirmEmailAsync(dto.Email, dto.Otp, It.IsAny<CancellationToken>()))
        //         .ReturnsAsync(new ConfirmEmailResponseDto { Success = true, Message = "Email confirmed" });

        //     var result = await _sut.ConfirmEmail(dto, CancellationToken.None);

        //     var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        //     var response = okResult.Value.Should().BeAssignableTo<Result<ConfirmEmailResponseDto>>().Subject;
        //     response.IsSuccess.Should().BeTrue();
        // }

        // #endregion

        #region ForgotPassword

        [Fact]
        public async Task ForgotPassword_ShouldReturnOk_WhenEmailExists()
        {
            var dto = new ForgotPasswordRequestDto { Email = "test@test.com" };
            _authService.Setup(a => a.ForgotPasswordAsync(dto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ForgotPasswordResponseDto { Message = "OTP sent", ExpirationInMinutes = 10 });

            var result = await _sut.ForgotPassword(dto, CancellationToken.None);

            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var response = okResult.Value.Should().BeAssignableTo<Result<ForgotPasswordResponseDto>>().Subject;
            response.IsSuccess.Should().BeTrue();
        }

        #endregion

        // #region VerifyOtp

        // [Fact]
        // public async Task VerifyOtp_ShouldReturnOk_WhenOtpIsValid()
        // {
        //     var dto = new VerifyOtpRequestDto { Email = "test@test.com", Code = "123456" };
        //     _authService.Setup(a => a.VerifyOtpAsync(dto, It.IsAny<CancellationToken>()))
        //         .ReturnsAsync(new VerifyOtpResponseDto { ResetToken = "reset_token", Message = "Verified" });

        //     var result = await _sut.VerifyOtp(dto, CancellationToken.None);

        //     var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        //     var response = okResult.Value.Should().BeAssignableTo<Result<VerifyOtpResponseDto>>().Subject;
        //     response.IsSuccess.Should().BeTrue();
        // }

        // #endregion

        #region ResetPassword

        [Fact]
        public async Task ResetPassword_ShouldReturnOk_WhenDataIsValid()
        {
            var dto = new ResetPasswordRequestDto 
            { 
                Email = "test@test.com", 
                ResetToken = "12345", 
                NewPassword = "NewPassword123!",
                ConfirmPassword = "NewPassword123!"
            };
            _authService.Setup(a => a.ResetPasswordAsync(dto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ResetPasswordResponseDto { Message = "Password reset" });

            var result = await _sut.ResetPassword(dto, CancellationToken.None);

            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var response = okResult.Value.Should().BeAssignableTo<Result<ResetPasswordResponseDto>>().Subject;
            response.IsSuccess.Should().BeTrue();
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
            var dto = new ChangePasswordRequest { CurrentPassword = "OldPassword123!", NewPassword = "NewPassword123!" };
            _authService.Setup(a => a.ChangePasswordAsync(dto, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await _sut.ChangePassword(dto, CancellationToken.None);

            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var response = okResult.Value.Should().BeAssignableTo<Result<string>>().Subject;
            response.IsSuccess.Should().BeTrue();
        }

        #endregion
    }
}
