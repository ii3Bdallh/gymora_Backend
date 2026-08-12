using System.Threading;
using System.Threading.Tasks;
using Api.Filters;
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
    [Authorize]
    public class SessionsController(ISessionService service, ILogger<SessionsController> logger) : ControllerBase
    {
        [HttpPost]
        [GymAuthorize(GymRoleString.Owner, GymRoleString.Manager, GymRoleString.Coach, GymRoleString.Member)]
        public async Task<ActionResult<Result<PaginatedRes<SessionRDTO>>>> GetPaged([FromBody] PaginatedSearchReq searchReq)
        {
            logger.LogInformation("Fetching workout sessions");
            var result = await service.GetPageAsync(searchReq, false);
            return Ok(Result<PaginatedRes<SessionRDTO>>.Success(result));
        }

        [HttpGet("{id}")]
        [GymAuthorize(GymRoleString.Owner, GymRoleString.Manager, GymRoleString.Coach, GymRoleString.Member)]
        public async Task<ActionResult<Result<SessionRDTO>>> GetById(int id, CancellationToken ct)
        {
            logger.LogInformation("Fetching workout session with Id: {Id}", id);
            var result = await service.GetByIdAsync(id, false, ct);
            return Ok(Result<SessionRDTO>.Success(result));
        }

        [HttpPost("Create")]
        [GymAuthorize(GymRoleString.Owner)]
        public async Task<ActionResult<Result<SessionRDTO>>> Create([FromBody] SessionCDTO dto, CancellationToken ct)
        {
            logger.LogInformation("Creating workout session: {@Dto}", dto);
            var result = await service.AddAsync(dto, ct);
            return StatusCode(StatusCodes.Status201Created, Result<SessionRDTO>.Success(result));
        }

        [HttpPut("{id}")]
        [Authorize(Roles = AppRole.SuperAdmin)]
        public async Task<ActionResult<Result<SessionRDTO>>> Update(int id, [FromBody] SessionUDTO dto, CancellationToken ct)
        {
            logger.LogInformation("Updating workout session with Id: {Id}", id);
            var result = await service.UpdateAsync(id, dto, ct);
            return Ok(Result<SessionRDTO>.Success(result));
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = AppRole.SuperAdmin)]
        public async Task<ActionResult<Result<SessionRDTO>>> Delete(int id, CancellationToken ct)
        {
            logger.LogInformation("Deleting workout session with Id: {Id}", id);
            var result = await service.DeleteAsync(id, ct);
            return Ok(Result<SessionRDTO>.Success(result));
        }

        [HttpPost("{sessionId}/Exercises")]
        [GymAuthorize(GymRoleString.Owner)]
        public async Task<ActionResult<Result<SessionExerciseRDTO>>> AddExerciseToSession(int sessionId, [FromBody] SessionExerciseCDTO dto, CancellationToken ct)
        {
            logger.LogInformation("Adding exercise to session with Id: {SessionId}", sessionId);
            var result = await service.AddExerciseToSessionAsync(sessionId, dto, ct);
            return StatusCode(StatusCodes.Status201Created, Result<SessionExerciseRDTO>.Success(result));
        }

        [HttpDelete("{sessionId}/Exercises/{exerciseId}")]
        [GymAuthorize(GymRoleString.Owner)]
        public async Task<ActionResult<Result<string>>> RemoveExerciseFromSession(int sessionId, int exerciseId, CancellationToken ct)
        {
            logger.LogInformation("Removing exercise {ExerciseId} from session with Id: {SessionId}", exerciseId, sessionId);
            await service.RemoveExerciseFromSessionAsync(sessionId, exerciseId, ct);
            return Ok(Result<string>.Success("Exercise removed successfully from session."));
        }

        [HttpPost("{id}/Approve")]
        [Authorize(Roles = AppRole.SuperAdmin)]
        public async Task<ActionResult<Result<string>>> Approve(int id, CancellationToken ct)
        {
            logger.LogInformation("Approving workout session with Id: {Id}", id);
            await service.ApproveAsync(id, ct);
            return Ok(Result<string>.Success("Workout session approved successfully."));
        }
    }
}
