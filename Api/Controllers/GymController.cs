using Application.DTO;
using Application.DTO.Model;
using Application.DTO.Pagintion;
using Application.Interface.Service;
using Domain.Enum;
using Gymora.Contracts.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;


namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GymController(ILogger<GymController> logger, IGymService service) : ControllerBase
    {


        [HttpPost]

        public async Task<ActionResult<IEnumerable<GymRDTO>>> GetPagedAsync([FromBody] PaginatedSearchReq searchReq)
        {
            logger.LogInformation("Fetching all Gyms");
            PaginatedRes<GymRDTO> Gyms = await service.GetPageAsync(searchReq, true);
            logger.LogInformation("Successfully fetched all Gyms");
            return Ok(Result<PaginatedRes<GymRDTO>>.Success(Gyms));
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<GymRDTO>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            logger.LogInformation("Fetching Gym with Id: {Id}", id);

            var Gym = await service.GetByIdAsync(id, true, cancellationToken: cancellationToken);

            logger.LogInformation("Successfully fetched Gym with Id: {Id}", id);
            return Ok(Result<GymRDTO>.Success(Gym));
        }

        [HttpPost("Create")]
        [Authorize]

        public async Task<ActionResult<GymRDTO>> CreateAsync([FromForm] GymCDTO GymDto)
        {
            if (!ModelState.IsValid)
            {
                logger.LogWarning("Invalid ModelState while creating Gym: {@GymDto}", GymDto);
                return BadRequest(ModelState);
            }

            logger.LogInformation("Creating a new Gym: {@GymDto}", GymDto);

            var createdGym = await service.AddAsync(GymDto);


            return Ok(Result<GymRDTO>.Success(createdGym));
        }

        [HttpPut("{id}")]
        [Authorize]

        public async Task<ActionResult> UpdateAsync(int id, [FromForm] GymUDTO GymDto)
        {

            if (!ModelState.IsValid)
            {
                logger.LogWarning("Invalid ModelState while updating Gym Id: {Id}", id);
                return BadRequest(ModelState);
            }

            logger.LogInformation("Updating Gym with Id: {Id}", id);

            var updatedGym = await service.UpdateAsync(id, GymDto);

            logger.LogInformation("Successfully updated Gym with Id: {Id}", id);
            return Ok(Result<GymRDTO>.Success(updatedGym));
        }

        [HttpDelete("{id}")]
        [Authorize]

        public async Task<ActionResult> DeleteAsync(int id)
        {
            logger.LogInformation("Deleting Gym with Id: {Id}", id);

            var deletedGym = await service.DeleteAsync(id);

            logger.LogInformation("Successfully deleted Gym with Id: {Id}", id);
            return Ok(Result<GymRDTO>.Success(deletedGym));
        }



        [HttpPost("change-owner")]
        public async Task<IActionResult> ChangeOwnerAsync([FromBody] ChangeOwnerDTO dto, CancellationToken ct)
        {
            await service.ChangeOwnerOfGymAsync(dto.GymId, dto.NewOwnerUserId, ct);

            return Ok("Owner changed successfully.");
        }


        [Authorize]
        [HttpGet("user-gyms")]
        public async Task<ActionResult<UserGymsListRDTO>> GetUserGymsAsync([FromQuery] UserGymsPagedReq req, CancellationToken cancellationToken)
        {
            return Ok(Result<UserGymsListRDTO>.Success(await service.GetUserGymsAsync(req, cancellationToken)));
        }

    }
}