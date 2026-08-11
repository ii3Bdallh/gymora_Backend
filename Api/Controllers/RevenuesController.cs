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
    public class RevenuesController(IRevenueService service, ILogger<RevenuesController> logger) : ControllerBase
    {
        [HttpPost]
        [GymAuthorize(GymRoleString.Owner, GymRoleString.Manager)]
        public async Task<ActionResult<Result<PaginatedRes<RevenueRDTO>>>> GetPaged([FromBody] PaginatedSearchReq searchReq)
        {
            logger.LogInformation("Fetching revenues");
            var result = await service.GetPageAsync(searchReq, false);
            return Ok(Result<PaginatedRes<RevenueRDTO>>.Success(result));
        }

        [HttpGet("{id}")]
        [GymAuthorize(GymRoleString.Owner, GymRoleString.Manager)]
        public async Task<ActionResult<Result<RevenueRDTO>>> GetById(int id, CancellationToken ct)
        {
            logger.LogInformation("Fetching revenue with Id: {Id}", id);
            var result = await service.GetByIdAsync(id, false, ct);
            return Ok(Result<RevenueRDTO>.Success(result));
        }

        [HttpPost("Create")]
        [GymAuthorize(GymRoleString.Owner, GymRoleString.Manager)]
        public async Task<ActionResult<Result<RevenueRDTO>>> Create([FromBody] RevenueCDTO dto, CancellationToken ct)
        {
            logger.LogInformation("Creating revenue: {@Dto}", dto);
            var result = await service.AddAsync(dto, ct);
            return StatusCode(StatusCodes.Status201Created, Result<RevenueRDTO>.Success(result));
        }

        [HttpPut("{id}")]
        [GymAuthorize(GymRoleString.Owner, GymRoleString.Manager)]
        public async Task<ActionResult<Result<RevenueRDTO>>> Update(int id, [FromBody] RevenueUDTO dto, CancellationToken ct)
        {
            logger.LogInformation("Updating revenue with Id: {Id}", id);
            var result = await service.UpdateAsync(id, dto, ct);
            return Ok(Result<RevenueRDTO>.Success(result));
        }

        [HttpDelete("{id}")]
        [GymAuthorize(GymRoleString.Owner, GymRoleString.Manager)]
        public async Task<ActionResult<Result<RevenueRDTO>>> Delete(int id, CancellationToken ct)
        {
            logger.LogInformation("Deleting revenue with Id: {Id}", id);
            var result = await service.DeleteAsync(id, ct);
            return Ok(Result<RevenueRDTO>.Success(result));
        }
    }
}
