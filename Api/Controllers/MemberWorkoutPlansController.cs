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
    public class MemberWorkoutPlansController(IMemberWorkoutPlanService service, ILogger<MemberWorkoutPlansController> logger) : ControllerBase
    {
        [HttpPost]
        [GymAuthorize(GymRoleString.Owner, GymRoleString.Manager, GymRoleString.Coach, GymRoleString.Member)]
        public async Task<ActionResult<Result<PaginatedRes<MemberWorkoutPlanRDTO>>>> GetPaged([FromBody] PaginatedSearchReq searchReq)
        {
            logger.LogInformation("Fetching member workout plan assignments");
            var result = await service.GetPageAsync(searchReq, false);
            return Ok(Result<PaginatedRes<MemberWorkoutPlanRDTO>>.Success(result));
        }

        [HttpGet("{id}")]
        [GymAuthorize(GymRoleString.Owner, GymRoleString.Manager, GymRoleString.Coach, GymRoleString.Member)]
        public async Task<ActionResult<Result<MemberWorkoutPlanRDTO>>> GetById(int id, CancellationToken ct)
        {
            logger.LogInformation("Fetching member workout plan assignment with Id: {Id}", id);
            var result = await service.GetByIdAsync(id, false, ct);
            return Ok(Result<MemberWorkoutPlanRDTO>.Success(result));
        }

        [HttpPost("Create")]
        [GymAuthorize(GymRoleString.Owner, GymRoleString.Manager, GymRoleString.Coach)]
        public async Task<ActionResult<Result<MemberWorkoutPlanRDTO>>> Create([FromBody] MemberWorkoutPlanCDTO dto, CancellationToken ct)
        {
            logger.LogInformation("Creating member workout plan assignment: {@Dto}", dto);
            var result = await service.AddAsync(dto, ct);
            return StatusCode(StatusCodes.Status201Created, Result<MemberWorkoutPlanRDTO>.Success(result));
        }

        [HttpPut("{id}")]
        [GymAuthorize(GymRoleString.Owner, GymRoleString.Manager, GymRoleString.Coach)]
        public async Task<ActionResult<Result<MemberWorkoutPlanRDTO>>> Update(int id, [FromBody] MemberWorkoutPlanUDTO dto, CancellationToken ct)
        {
            logger.LogInformation("Updating member workout plan assignment with Id: {Id}", id);
            var result = await service.UpdateAsync(id, dto, ct);
            return Ok(Result<MemberWorkoutPlanRDTO>.Success(result));
        }

        [HttpDelete("{id}")]
        [GymAuthorize(GymRoleString.Owner, GymRoleString.Manager, GymRoleString.Coach)]
        public async Task<ActionResult<Result<MemberWorkoutPlanRDTO>>> Delete(int id, CancellationToken ct)
        {
            logger.LogInformation("Deleting member workout plan assignment with Id: {Id}", id);
            var result = await service.DeleteAsync(id, ct);
            return Ok(Result<MemberWorkoutPlanRDTO>.Success(result));
        }

        [HttpPost("{id}/Cancel")]
        [GymAuthorize(GymRoleString.Owner, GymRoleString.Manager, GymRoleString.Coach)]
        public async Task<ActionResult<Result<string>>> CancelAssignment(int id, CancellationToken ct)
        {
            logger.LogInformation("Cancelling member workout plan assignment with Id: {Id}", id);
            await service.CancelAssignmentAsync(id, ct);
            return Ok(Result<string>.Success("Workout plan assignment cancelled successfully."));
        }
    }
}
