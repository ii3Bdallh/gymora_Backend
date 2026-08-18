using Api.Controllers;
using Application.DTO;
using Application.DTO.Exceptions;
using Application.DTO.Model;
using Application.DTO.Pagintion;
using Application.Interface.Service;
using Domain.Enum;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using Xunit;

namespace UnitTests.Controllers;

public class UsersControllerTests
{
    private readonly Mock<IUsersService> _service;
    private readonly Mock<ILogger<UsersController>> _logger;
    private readonly UsersController _sut;

    public UsersControllerTests()
    {
        _service = new Mock<IUsersService>();
        _logger = new Mock<ILogger<UsersController>>();
        _sut = new UsersController(_logger.Object, _service.Object);
    }

    private void SetupUserClaims(string? userIdStr)
    {
        var claims = new List<Claim>();
        if (userIdStr != null)
        {
            claims.Add(new Claim("UserId", userIdStr));
            claims.Add(new Claim(ClaimTypes.NameIdentifier, userIdStr));
        }

        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };
    }

    #region GetPagedAsync

    [Fact]
    public async Task GetPagedAsync_ShouldReturnOk_WhenDataExists()
    {
        var searchReq = new PaginatedSearchReq { PageNumber = 1, PageSize = 10 };
        var pageResult = new PaginatedRes<ApplicationUserRDTO>
        {
            PageNumber = 1,
            PageSize = 10,
            TotalCount = 1,
            Items = new List<ApplicationUserRDTO>
            {
                new()
                {
                    Id = 1,
                    PersonName = "John Doe",
                    Email = "john@example.com",
                    PhoneNumber = "+123456789",
                    CreatedOn = DateTime.UtcNow
                }
            }
        };

        _service.Setup(s => s.GetPageAsync(searchReq, true, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pageResult);

        var result = await _sut.GetPagedAsync(searchReq);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<Result<PaginatedRes<ApplicationUserRDTO>>>().Subject;
        response.IsSuccess.Should().BeTrue();
        response.Data!.Items.Should().HaveCount(1);
        response.Data.Items.First().PersonName.Should().Be("John Doe");
    }

    #endregion

    #region GetByIdAsync

    [Fact]
    public async Task GetByIdAsync_ShouldReturnOk_WhenUserExists()
    {
        var user = new ApplicationUserRDTO
        {
            Id = 1,
            PersonName = "Jane Doe",
            Email = "jane@example.com"
        };
        _service.Setup(s => s.GetByIdDetailsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _sut.GetByIdAsync(1);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<Result<ApplicationUserRDTO>>().Subject;
        response.IsSuccess.Should().BeTrue();
        response.Data!.PersonName.Should().Be("Jane Doe");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldThrowNotFoundException_WhenUserNotFound()
    {
        _service.Setup(s => s.GetByIdDetailsAsync(999, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException("ApplicationUser with ID 999 was not found."));

        var act = async () => await _sut.GetByIdAsync(999);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region GetProfile

    [Fact]
    public async Task GetProfile_ShouldReturnOk_WhenUserIsAuthenticated()
    {
        SetupUserClaims("1");
        var profile = new Gymora.Contracts.Authentication.UserProfileRDTO(
            "1",
            "John Doe",
            "john@example.com",
            "+123456789",
            null,
            DateTime.UtcNow,
            "User"
        );

        _service.Setup(s => s.GetUserProfileAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);

        var result = await _sut.GetProfile();

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<Result<Gymora.Contracts.Authentication.UserProfileRDTO>>().Subject;
        response.IsSuccess.Should().BeTrue();
        response.Data!.PersonName.Should().Be("John Doe");
    }

    [Fact]
    public async Task GetProfile_ShouldReturnUnauthorized_WhenUserIsNotAuthenticated()
    {
        SetupUserClaims(null);

        var result = await _sut.GetProfile();

        result.Result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    #endregion

    #region UploadProfilePicture

    [Fact]
    public async Task UploadProfilePicture_ShouldReturnOk_WhenUserIsAuthenticated()
    {
        SetupUserClaims("1");
        var mockFile = new Mock<IFormFile>();
        var uploadDto = new Gymora.Contracts.Authentication.UserProfilePictureUploadDTO { File = mockFile.Object };
        var profile = new Gymora.Contracts.Authentication.UserProfileRDTO(
            "1",
            "John Doe",
            "john@example.com",
            "+123456789",
            "https://storage.bunnycdn.com/users/profile.jpg",
            DateTime.UtcNow,
            "User"
        );

        _service.Setup(s => s.UploadProfilePictureAsync(1, mockFile.Object, It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);

        var result = await _sut.UploadProfilePicture(uploadDto);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<Result<Gymora.Contracts.Authentication.UserProfileRDTO>>().Subject;
        response.IsSuccess.Should().BeTrue();
        response.Data!.ProfilePictureUrl.Should().Be("https://storage.bunnycdn.com/users/profile.jpg");
    }

    [Fact]
    public async Task UploadProfilePicture_ShouldReturnUnauthorized_WhenUserIsNotAuthenticated()
    {
        SetupUserClaims(null);
        var mockFile = new Mock<IFormFile>();
        var uploadDto = new Gymora.Contracts.Authentication.UserProfilePictureUploadDTO { File = mockFile.Object };

        var result = await _sut.UploadProfilePicture(uploadDto);

        result.Result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    #endregion
}
