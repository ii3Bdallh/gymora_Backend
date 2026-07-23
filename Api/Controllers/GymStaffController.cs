using Application.DTO;
using Application.DTO.Model;
using Application.DTO.Pagintion;
using Application.Interface.Service;
using Domain.Enum;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;


namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class GymStaffController(ILogger<GymStaffController> logger, IGymStaffService service) : ControllerBase
    {


        [HttpPost]
        public async Task<ActionResult<IEnumerable<GymStaffRDTO>>> GetPagedAsync([FromBody] PaginatedSearchReq searchReq)
        {
            logger.LogInformation("Fetching all GymStaffs");
            PaginatedRes<GymStaffRDTO> GymStaffs = await service.GetPageAsync(searchReq, true);
            logger.LogInformation("Successfully fetched all GymStaffs");
            return Ok(Result<PaginatedRes<GymStaffRDTO>>.Success(GymStaffs));
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<GymStaffRDTO>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            logger.LogInformation("Fetching GymStaff with Id: {Id}", id);

            var GymStaff = await service.GetByIdAsync(id, true, cancellationToken: cancellationToken);

            logger.LogInformation("Successfully fetched GymStaff with Id: {Id}", id);
            return Ok(Result<GymStaffRDTO>.Success(GymStaff));
        }

        [HttpPost("Create")]
        public async Task<ActionResult<GymStaffRDTO>> CreateAsync([FromBody] GymStaffCDTO GymStaffDto)
        {
            if (!ModelState.IsValid)
            {
                logger.LogWarning("Invalid ModelState while creating GymStaff: {@GymStaffDto}", GymStaffDto);
                return BadRequest(ModelState);
            }

            logger.LogInformation("Creating a new GymStaff: {@GymStaffDto}", GymStaffDto);

            var createdGymStaff = await service.AddAsync(GymStaffDto);


            return Ok(Result<GymStaffRDTO>.Success(createdGymStaff));
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateAsync(int id, [FromBody] GymStaffUDTO GymStaffDto)
        {

            if (!ModelState.IsValid)
            {
                logger.LogWarning("Invalid ModelState while updating GymStaff Id: {Id}", id);
                return BadRequest(ModelState);
            }

            logger.LogInformation("Updating GymStaff with Id: {Id}", id);

            var updatedGymStaff = await service.UpdateAsync(id, GymStaffDto);

            logger.LogInformation("Successfully updated GymStaff with Id: {Id}", id);
            return Ok(Result<GymStaffRDTO>.Success(updatedGymStaff));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteAsync(int id)
        {
            logger.LogInformation("Deleting GymStaff with Id: {Id}", id);

            var deletedGymStaff = await service.DeleteAsync(id);

            logger.LogInformation("Successfully deleted GymStaff with Id: {Id}", id);
            return Ok(Result<GymStaffRDTO>.Success(deletedGymStaff));
        }

        [HttpPost("LinkAccountToGym")]
        public async Task<ActionResult<GymStaffRDTO>> LinkAccountToGymAsync([FromQuery] int gymId, [FromQuery] Guid inviteCode, CancellationToken ct = default)
        {
            logger.LogInformation("Linking account to gym with Id: {GymId} using invite code: {InviteCode}", gymId, inviteCode);

            var linkedGymStaff = await service.LinkAccountToGymAsync(gymId, inviteCode, ct);

            logger.LogInformation("Successfully linked account to gym with Id: {GymId}", gymId);
            return Ok(Result<GymStaffRDTO>.Success(linkedGymStaff));
        }

        [HttpPost("{id}/pay-salary")]
        public async Task<ActionResult> PaySalaryAsync(
            int id, 
            [FromQuery] DateTime? salaryValidFrom, 
            [FromQuery] DateTime? salaryValidUntil, 
            CancellationToken ct = default)
        {
            logger.LogInformation("Paying salary for GymStaff with Id: {Id}", id);
            await service.PaySalaryAsync(id, salaryValidFrom, salaryValidUntil, ct);
            logger.LogInformation("Successfully initiated salary payment for GymStaff with Id: {Id}", id);
            return Ok(Result<string>.Success("Salary payment initiated successfully."));
        }


    }
}