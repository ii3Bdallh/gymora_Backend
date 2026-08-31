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
    public class WorkoutPlansController(IWorkoutPlanService service, ILogger<WorkoutPlansController> logger) : ControllerBase
    {
        [HttpPost]
        [GymAuthorize(GymRoleString.Owner, GymRoleString.Manager, GymRoleString.Coach, GymRoleString.Member)]
        public async Task<ActionResult<Result<PaginatedRes<WorkoutPlanRDTO>>>> GetPaged([FromBody] PaginatedSearchReq searchReq, CancellationToken ct = default)
        {
            logger.LogInformation("Fetching workout plans");
            var result = await service.GetPageAsync(searchReq, false, ct);
            return Ok(Result<PaginatedRes<WorkoutPlanRDTO>>.Success(result));
        }

        [HttpGet("{id}")]
        [GymAuthorize(GymRoleString.Owner, GymRoleString.Manager, GymRoleString.Coach, GymRoleString.Member)]
        public async Task<ActionResult<Result<WorkoutPlanRDTO>>> GetById(int id, CancellationToken ct = default)
        {
            logger.LogInformation("Fetching workout plan with Id: {Id}", id);
            var result = await service.GetByIdDetailsAsync(id, ct);
            return Ok(Result<WorkoutPlanRDTO>.Success(result));
        }

        [HttpPost("Create")]
        [GymAuthorize(GymRoleString.Owner)]
        public async Task<ActionResult<Result<WorkoutPlanRDTO>>> Create([FromBody] WorkoutPlanCDTO dto, CancellationToken ct)
        {
            logger.LogInformation("Creating workout plan: {@Dto}", dto);
            var result = await service.AddAsync(dto, ct);
            return StatusCode(StatusCodes.Status201Created, Result<WorkoutPlanRDTO>.Success(result));
        }

        [HttpPut("{id}")]
        [Authorize(Roles = AppRole.SuperAdmin)]
        public async Task<ActionResult<Result<WorkoutPlanRDTO>>> Update(int id, [FromBody] WorkoutPlanUDTO dto, CancellationToken ct)
        {
            logger.LogInformation("Updating workout plan with Id: {Id}", id);
            var result = await service.UpdateAsync(id, dto, ct);
            return Ok(Result<WorkoutPlanRDTO>.Success(result));
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = AppRole.SuperAdmin)]
        public async Task<ActionResult<Result<WorkoutPlanRDTO>>> Delete(int id, CancellationToken ct)
        {
            logger.LogInformation("Deleting workout plan with Id: {Id}", id);
            var result = await service.DeleteAsync(id, ct);
            return Ok(Result<WorkoutPlanRDTO>.Success(result));
        }

        [HttpPost("{id}/Approve")]
        [Authorize(Roles = AppRole.SuperAdmin)]
        public async Task<ActionResult<Result<string>>> Approve(int id, CancellationToken ct)
        {
            logger.LogInformation("Approving workout plan with Id: {Id}", id);
            await service.ApproveAsync(id, ct);
            return Ok(Result<string>.Success("Workout plan approved successfully."));
        }
    }
}
