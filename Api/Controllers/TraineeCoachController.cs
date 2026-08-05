
using System.Threading;
using System.Threading.Tasks;
using Application.DTO;
using Application.DTO.Model;
using Application.DTO.Pagintion;
using Application.Interface.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Logging;

namespace Api.Controllers;

[ApiController]
[Route("api/v1/gyms/{gymId}")]
[Authorize]
public class TraineeCoachController(
    ICoachAssignmentService coachAssignmentService,
    ILogger<TraineeCoachController> logger)
    : ControllerBase
{
    [HttpGet("coaches/me/trainees")]
    [EnableRateLimiting("UserRateLimiter")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Result<PaginatedRes<CoachAssignmentRDTO>>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(Result<object>))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(Result<object>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(Result<object>))]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity, Type = typeof(Result<object>))]
    public async Task<IActionResult> GetAssignedTrainees(
        [FromRoute] int gymId,
        [FromQuery] GetAssignedMemberForCoachPagedReq req,
        CancellationToken ct)
    {
        logger.LogInformation("Coach retrieving assigned trainees for gym: {GymId}", gymId);
        
        if (req.PageNumber < 1 || req.PageSize < 1 || req.PageSize > 50)
        {
            return StatusCode(StatusCodes.Status422UnprocessableEntity, 
                Result<object>.Failure("VALIDATION_ERROR", "Invalid pagination parameters."));
        }

        var result = await coachAssignmentService.GetPageAsync(req, false, ct);
        return Ok(Result<PaginatedRes<CoachAssignmentRDTO>>.Success(result));
    }

    [HttpPost("coach-assignments")]
    [EnableRateLimiting("UserRateLimiter")]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(Result<CoachAssignmentRDTO>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(Result<object>))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(Result<object>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(Result<object>))]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity, Type = typeof(Result<object>))]
    public async Task<IActionResult> AssignCoach(
        [FromRoute] int gymId,
        [FromBody] CoachAssignmentCDTO dto,
        CancellationToken ct)
    {
        logger.LogInformation("Assigning coach in gym: {GymId}", gymId);

        if (dto == null)
        {
            return StatusCode(StatusCodes.Status422UnprocessableEntity, 
                Result<object>.Failure("VALIDATION_ERROR", "Request body is required."));
        }

        var result = await coachAssignmentService.AddAsync(dto, ct);
        return StatusCode(StatusCodes.Status201Created, Result<CoachAssignmentRDTO>.Success(result));
    }
}
