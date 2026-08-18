using Application.DTO;
using Application.DTO.Model;
using Application.DTO.Pagintion;
using Application.Interface.Service;
using Domain.Enum;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController(ILogger<UsersController> logger, IUsersService service) : ControllerBase
    {
        [HttpPost]
        [Authorize(Roles = $"{AppRole.SuperAdmin}")]
        public async Task<ActionResult<Result<PaginatedRes<ApplicationUserRDTO>>>> GetPagedAsync([FromBody] PaginatedSearchReq searchReq, CancellationToken cancellationToken = default)
        {
            logger.LogInformation("Fetching all Users");
            PaginatedRes<ApplicationUserRDTO> users = await service.GetPageAsync(searchReq, true, cancellationToken: cancellationToken);
            logger.LogInformation("Successfully fetched all Users");
            return Ok(Result<PaginatedRes<ApplicationUserRDTO>>.Success(users));
        }

        [HttpGet("{id}")]
        [Authorize(Roles = $"{AppRole.SuperAdmin}")]
        public async Task<ActionResult<Result<ApplicationUserRDTO>>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            logger.LogInformation("Fetching User with Id: {Id}", id);

            var user = await service.GetByIdDetailsAsync(id, cancellationToken);

            logger.LogInformation("Successfully fetched User with Id: {Id}", id);
            return Ok(Result<ApplicationUserRDTO>.Success(user));
        }

        [HttpGet("profile")]
        public async Task<ActionResult<Result<Gymora.Contracts.Authentication.UserProfileRDTO>>> GetProfile(CancellationToken cancellationToken = default)
        {
            logger.LogInformation("Fetching current user profile");
            var userIdStr = User.FindFirstValue("UserId") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out int userId))
            {
                return Unauthorized(Result<string>.Failure("AUTH_REQUIRED", "User authentication required."));
            }

            var profile = await service.GetUserProfileAsync(userId, cancellationToken);
            return Ok(Result<Gymora.Contracts.Authentication.UserProfileRDTO>.Success(profile));
        }

        [HttpPut("profile")]
        public async Task<ActionResult<Result<Gymora.Contracts.Authentication.UserProfileRDTO>>> UpdateProfile([FromBody] Gymora.Contracts.Authentication.UserProfileUDTO updateDto, CancellationToken cancellationToken = default)
        {
            logger.LogInformation("Updating current user profile");
            var userIdStr = User.FindFirstValue("UserId") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out int userId))
            {
                return Unauthorized(Result<string>.Failure("AUTH_REQUIRED", "User authentication required."));
            }

            var profile = await service.UpdateUserProfileAsync(userId, updateDto, cancellationToken);
            return Ok(Result<Gymora.Contracts.Authentication.UserProfileRDTO>.Success(profile));
        }

        [HttpPost("profile/picture")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<Result<Gymora.Contracts.Authentication.UserProfileRDTO>>> UploadProfilePicture([FromForm] Gymora.Contracts.Authentication.UserProfilePictureUploadDTO uploadDto, CancellationToken cancellationToken = default)
        {
            logger.LogInformation("Uploading profile picture for current user");
            var userIdStr = User.FindFirstValue("UserId") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out int userId))
            {
                return Unauthorized(Result<string>.Failure("AUTH_REQUIRED", "User authentication required."));
            }

            var profile = await service.UploadProfilePictureAsync(userId, uploadDto.File, cancellationToken);
            return Ok(Result<Gymora.Contracts.Authentication.UserProfileRDTO>.Success(profile));
        }
    }
}