using System.Threading;
using System.Threading.Tasks;
using Application.DTO;
using Application.DTO.Model;
using Application.DTO.Pagintion;
using Application.Interface.Service;
using Domain.Enum;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = AppRole.SuperAdmin)]
    public class UserWorkoutBlocksController(IUserWorkoutBlockService service, ILogger<UserWorkoutBlocksController> logger) : ControllerBase
    {
        [HttpPost]
        public async Task<ActionResult<Result<PaginatedRes<UserWorkoutBlockRDTO>>>> GetPaged([FromBody] PaginatedSearchReq searchReq, CancellationToken ct = default)
        {
            logger.LogInformation("Fetching user workout blocks");
            var result = await service.GetPageAsync(searchReq, false, ct);
            return Ok(Result<PaginatedRes<UserWorkoutBlockRDTO>>.Success(result));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Result<UserWorkoutBlockRDTO>>> GetById(int id, CancellationToken ct = default)
        {
            logger.LogInformation("Fetching user workout block with Id: {Id}", id);
            var result = await service.GetByIdDetailsAsync(id, ct);
            return Ok(Result<UserWorkoutBlockRDTO>.Success(result));
        }

        [HttpPost("Create")]
        public async Task<ActionResult<Result<UserWorkoutBlockRDTO>>> Create([FromBody] UserWorkoutBlockCDTO dto, CancellationToken ct)
        {
            logger.LogInformation("Creating user workout block: {@Dto}", dto);
            var result = await service.AddAsync(dto, ct);
            return StatusCode(StatusCodes.Status201Created, Result<UserWorkoutBlockRDTO>.Success(result));
        }

        [HttpPost("Block")]
        public async Task<ActionResult<Result<UserWorkoutBlockRDTO>>> BlockUser([FromBody] UserWorkoutBlockCDTO dto, CancellationToken ct)
        {
            logger.LogInformation("Blocking user: {@Dto}", dto);
            var result = await service.AddAsync(dto, ct);
            return StatusCode(StatusCodes.Status201Created, Result<UserWorkoutBlockRDTO>.Success(result));
        }

        [HttpPost("Unblock/{userId}")]
        public async Task<ActionResult<Result<string>>> UnblockUser(int userId, CancellationToken ct)
        {
            logger.LogInformation("Unblocking user with Id: {UserId}", userId);
            await service.UnblockUserAsync(userId, ct);
            return Ok(Result<string>.Success("User unblocked successfully."));
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Result<UserWorkoutBlockRDTO>>> Update(int id, [FromBody] UserWorkoutBlockUDTO dto, CancellationToken ct)
        {
            logger.LogInformation("Updating user workout block with Id: {Id}", id);
            var result = await service.UpdateAsync(id, dto, ct);
            return Ok(Result<UserWorkoutBlockRDTO>.Success(result));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<Result<UserWorkoutBlockRDTO>>> Delete(int id, CancellationToken ct)
        {
            logger.LogInformation("Deleting user workout block with Id: {Id} (Unblocking user)", id);
            var result = await service.DeleteAsync(id, ct);
            return Ok(Result<UserWorkoutBlockRDTO>.Success(result));
        }
    }
}
