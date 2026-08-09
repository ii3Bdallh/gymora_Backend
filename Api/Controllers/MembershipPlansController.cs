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
using System.Threading;
using System.Threading.Tasks;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MembershipPlansController(IMembershipPlanService service, ILogger<MembershipPlansController> logger) : ControllerBase
    {
        [HttpPost]
        [GymAuthorize(GymRoleString.Owner, GymRoleString.Manager, GymRoleString.Receptionist, GymRoleString.Coach, GymRoleString.Member)]
        public async Task<ActionResult<Result<PaginatedRes<MembershipPlanRDTO>>>> GetPaged([FromBody] PaginatedSearchReq searchReq)
        {
            logger.LogInformation("Fetching membership plans");
            var result = await service.GetPageAsync(searchReq, false);
            return Ok(Result<PaginatedRes<MembershipPlanRDTO>>.Success(result));
        }

        [HttpGet("{id}")]
        [GymAuthorize]
        public async Task<ActionResult<Result<MembershipPlanRDTO>>> GetById(int id, CancellationToken ct)
        {
            logger.LogInformation("Fetching membership plan with Id: {Id}", id);
            var result = await service.GetByIdAsync(id, false, ct);
            return Ok(Result<MembershipPlanRDTO>.Success(result));
        }

        [HttpPost("Create")]
        [GymAuthorize(GymRoleString.Owner, GymRoleString.Manager)]
        public async Task<ActionResult<Result<MembershipPlanRDTO>>> Create([FromBody] MembershipPlanCDTO dto, CancellationToken ct)
        {
            logger.LogInformation("Creating membership plan: {@Dto}", dto);
            var result = await service.AddAsync(dto, ct);
            return StatusCode(StatusCodes.Status201Created, Result<MembershipPlanRDTO>.Success(result));
        }

        [HttpPut("{id}")]
        [GymAuthorize(GymRoleString.Owner, GymRoleString.Manager)]
        public async Task<ActionResult<Result<MembershipPlanRDTO>>> Update(int id, [FromBody] MembershipPlanUDTO dto, CancellationToken ct)
        {
            logger.LogInformation("Updating membership plan with Id: {Id}", id);
            var result = await service.UpdateAsync(id, dto, ct);
            return Ok(Result<MembershipPlanRDTO>.Success(result));
        }

        [HttpDelete("{id}")]
        [GymAuthorize(GymRoleString.Owner, GymRoleString.Manager)]
        public async Task<ActionResult<Result<MembershipPlanRDTO>>> Delete(int id, CancellationToken ct)
        {
            logger.LogInformation("Deleting membership plan with Id: {Id}", id);
            var result = await service.DeleteAsync(id, ct);
            return Ok(Result<MembershipPlanRDTO>.Success(result));
        }
    }
}
