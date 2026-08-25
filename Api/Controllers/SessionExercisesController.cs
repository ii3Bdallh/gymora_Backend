using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Api.Filters;
using Application.DTO;
using Application.DTO.Model;
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
    public class SessionExercisesController(ISessionExerciseService service, ILogger<SessionExercisesController> logger) : ControllerBase
    {
        [HttpPost("Batch")]
        [GymAuthorize(GymRoleString.Owner)]
        public async Task<ActionResult<Result<IEnumerable<SessionExerciseRDTO>>>> BatchCreate([FromBody] IEnumerable<SessionExerciseCDTO> dtos, CancellationToken ct = default)
        {
            logger.LogInformation("Batch creating session exercises");
            var result = await service.AddRangeAsync(dtos, ct);
            return StatusCode(StatusCodes.Status201Created, Result<IEnumerable<SessionExerciseRDTO>>.Success(result));
        }

        [HttpDelete("Batch")]
        [GymAuthorize(GymRoleString.Owner)]
        public async Task<ActionResult<Result<string>>> BatchDelete([FromBody] IEnumerable<int> ids, CancellationToken ct = default)
        {
            logger.LogInformation("Batch deleting session exercises");
            await service.DeleteRangeAsync(ids, ct);
            return Ok(Result<string>.Success("Session exercises deleted successfully."));
        }
    }
}
