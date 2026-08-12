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
    public class BodyMeasurementsController(IBodyMeasurementService service, ILogger<BodyMeasurementsController> logger) : ControllerBase
    {
        [HttpPost]
        public async Task<ActionResult<Result<PaginatedRes<BodyMeasurementRDTO>>>> GetPaged([FromBody] PaginatedSearchReq searchReq)
        {
            logger.LogInformation("Fetching body measurements");
            var result = await service.GetPageAsync(searchReq, false);
            return Ok(Result<PaginatedRes<BodyMeasurementRDTO>>.Success(result));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Result<BodyMeasurementRDTO>>> GetById(int id, CancellationToken ct)
        {
            logger.LogInformation("Fetching body measurement with Id: {Id}", id);
            var result = await service.GetByIdAsync(id, false, ct);
            return Ok(Result<BodyMeasurementRDTO>.Success(result));
        }

        [HttpPost("Create")]
        [GymAuthorize(GymRoleString.Owner, GymRoleString.Manager, GymRoleString.Coach)]
        public async Task<ActionResult<Result<BodyMeasurementRDTO>>> Create([FromBody] BodyMeasurementCDTO dto, CancellationToken ct)
        {
            logger.LogInformation("Creating body measurement: {@Dto}", dto);
            var result = await service.AddAsync(dto, ct);
            return StatusCode(StatusCodes.Status201Created, Result<BodyMeasurementRDTO>.Success(result));
        }

        [HttpPut("{id}")]
        [GymAuthorize(GymRoleString.Owner, GymRoleString.Manager, GymRoleString.Coach)]
        public async Task<ActionResult<Result<BodyMeasurementRDTO>>> Update(int id, [FromBody] BodyMeasurementUDTO dto, CancellationToken ct)
        {
            logger.LogInformation("Updating body measurement with Id: {Id}", id);
            var result = await service.UpdateAsync(id, dto, ct);
            return Ok(Result<BodyMeasurementRDTO>.Success(result));
        }

        [HttpDelete("{id}")]
        [GymAuthorize(GymRoleString.Owner, GymRoleString.Manager, GymRoleString.Coach)]
        public async Task<ActionResult<Result<BodyMeasurementRDTO>>> Delete(int id, CancellationToken ct)
        {
            logger.LogInformation("Deleting body measurement with Id: {Id}", id);
            var result = await service.DeleteAsync(id, ct);
            return Ok(Result<BodyMeasurementRDTO>.Success(result));
        }
    }
}
