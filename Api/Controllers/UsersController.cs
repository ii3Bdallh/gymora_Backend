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
    public class UsersController(ILogger<UsersController> logger, IUsersService service, IUserService profileService) : ControllerBase
    {


        [HttpPost]
        [Authorize(Roles = $"{AppRole.SuperAdmin}")]
        public async Task<ActionResult<IEnumerable<ApplicationUserRDTO>>> GetPagedAsync([FromBody] PaginatedSearchReq searchReq)
        {
            logger.LogInformation("Fetching all Userss");
            PaginatedRes<ApplicationUserRDTO> Userss = await service.GetPageAsync(searchReq, true);
            logger.LogInformation("Successfully fetched all Userss");
            return Ok(Result<PaginatedRes<ApplicationUserRDTO>>.Success(Userss));
        }
        [HttpGet("{id}")]
        [Authorize(Roles = $"{AppRole.SuperAdmin}")]
        public async Task<ActionResult<ApplicationUserRDTO>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            logger.LogInformation("Fetching Users with Id: {Id}", id);

            var Users = await service.GetByIdAsync(id, true, cancellationToken: cancellationToken);

            logger.LogInformation("Successfully fetched Users with Id: {Id}", id);
            return Ok(Result<ApplicationUserRDTO>.Success(Users));
        }
        [HttpGet("profile")]
        public async Task<ActionResult<Gymora.Contracts.Authentication.UserProfileRDTO>> GetProfile(CancellationToken cancellationToken)
        {
            logger.LogInformation("Fetching current user profile");
            var userIdStr = User.FindFirstValue("UserId") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out int userId))
            {
                return Unauthorized(Result<string>.Failure("AUTH_REQUIRED", "User authentication required."));
            }

            var profile = await profileService.GetUserProfileAsync(userId, cancellationToken);
            return Ok(Result<Gymora.Contracts.Authentication.UserProfileRDTO>.Success(profile));
        }

        [HttpPut("profile")]
        public async Task<ActionResult<Gymora.Contracts.Authentication.UserProfileRDTO>> UpdateProfile([FromBody] Gymora.Contracts.Authentication.UserProfileUDTO updateDto, CancellationToken cancellationToken)
        {
            logger.LogInformation("Updating current user profile");
            var userIdStr = User.FindFirstValue("UserId") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out int userId))
            {
                return Unauthorized(Result<string>.Failure("AUTH_REQUIRED", "User authentication required."));
            }

            var profile = await profileService.UpdateUserProfileAsync(userId, updateDto, cancellationToken);
            return Ok(Result<Gymora.Contracts.Authentication.UserProfileRDTO>.Success(profile));
        }

    }
}