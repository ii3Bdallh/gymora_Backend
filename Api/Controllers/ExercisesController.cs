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
    public class ExercisesController(IExerciseService service, ILogger<ExercisesController> logger) : ControllerBase
    {
        [HttpPost]
        [GymAuthorize(GymRoleString.Owner, GymRoleString.Manager, GymRoleString.Coach, GymRoleString.Member)]
        public async Task<ActionResult<Result<PaginatedRes<ExerciseRDTO>>>> GetPaged([FromBody] PaginatedSearchReq searchReq, CancellationToken ct = default)
        {
            logger.LogInformation("Fetching exercises");
            var result = await service.GetPageAsync(searchReq, false, ct);
            return Ok(Result<PaginatedRes<ExerciseRDTO>>.Success(result));
        }

        [HttpGet("{id}")]
        [GymAuthorize(GymRoleString.Owner, GymRoleString.Manager, GymRoleString.Coach, GymRoleString.Member)]
        public async Task<ActionResult<Result<ExerciseRDTO>>> GetById(int id, CancellationToken ct = default)
        {
            logger.LogInformation("Fetching exercise with Id: {Id}", id);
            var result = await service.GetByIdDetailsAsync(id, ct);
            return Ok(Result<ExerciseRDTO>.Success(result));
        }

        [HttpPost("Create")]
        [GymAuthorize(GymRoleString.Owner)]
        public async Task<ActionResult<Result<ExerciseRDTO>>> Create([FromBody] ExerciseCDTO dto, CancellationToken ct)
        {
            logger.LogInformation("Creating exercise: {@Dto}", dto);
            var result = await service.AddAsync(dto, ct);
            return StatusCode(StatusCodes.Status201Created, Result<ExerciseRDTO>.Success(result));
        }

        

        [HttpPut("{id}")]
        [Authorize(Roles = $"{AppRole.SuperAdmin}")]
        public async Task<ActionResult<Result<ExerciseRDTO>>> Update(int id, [FromBody] ExerciseUDTO dto, CancellationToken ct)
        {
            logger.LogInformation("Updating exercise with Id: {Id}", id);
            var result = await service.UpdateAsync(id, dto, ct);
            return Ok(Result<ExerciseRDTO>.Success(result));
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = AppRole.SuperAdmin)]
        public async Task<ActionResult<Result<ExerciseRDTO>>> Delete(int id, CancellationToken ct)
        {
            logger.LogInformation("Deleting exercise with Id: {Id}", id);
            var result = await service.DeleteAsync(id, ct);
            return Ok(Result<ExerciseRDTO>.Success(result));
        }

        [HttpPost("{id}/Approve")]
        [Authorize(Roles = AppRole.SuperAdmin)]
        public async Task<ActionResult<Result<string>>> Approve(int id, CancellationToken ct)
        {
            logger.LogInformation("Approving exercise with Id: {Id}", id);
            await service.ApproveAsync(id, ct);
            return Ok(Result<string>.Success("Exercise approved successfully."));
        }
    }
}
