using Application.DTO;
using Application.DTO.Model;
using Application.DTO.Pagintion;
using Application.Interface.Service;
using Domain.Enum;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Api.Filters;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GymPersonController(ILogger<GymPersonController> logger, IGymPersonService service) : ControllerBase
    {
        [HttpPost]
        [GymAuthorize(GymRoleString.Owner,
         GymRoleString.Manager,
         GymRoleString.Coach,
         GymRoleString.Receptionist
         )]
        public async Task<ActionResult<IEnumerable<GymPersonRDTO>>> GetPagedAsync([FromBody] PaginatedSearchReq searchReq)
        {
            logger.LogInformation("Fetching all GymPersons");
            PaginatedRes<GymPersonRDTO> GymPersons = await service.GetPageAsync(searchReq, false);
            logger.LogInformation("Successfully fetched all GymPersons");
            return Ok(Result<PaginatedRes<GymPersonRDTO>>.Success(GymPersons));
        }

        [HttpGet("{id}")]
        [GymAuthorize]
        public async Task<ActionResult<GymPersonRDTO>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            logger.LogInformation("Fetching GymPerson with Id: {Id}", id);

            var GymPerson = await service.GetByIdAsync(id, false, cancellationToken: cancellationToken);

            logger.LogInformation("Successfully fetched GymPerson with Id: {Id}", id);
            return Ok(Result<GymPersonRDTO>.Success(GymPerson));
        }

        [HttpPost("Create")]
        [GymAuthorize(GymRoleString.Owner,
         GymRoleString.Manager,
         GymRoleString.Coach,
         GymRoleString.Receptionist)]
        public async Task<ActionResult<GymPersonRDTO>> CreateAsync([FromBody] GymPersonCDTO GymPersonDto)
        {
            if (!ModelState.IsValid)
            {
                logger.LogWarning("Invalid ModelState while creating GymPerson: {@GymPersonDto}", GymPersonDto);
                return BadRequest(ModelState);
            }

            logger.LogInformation("Creating a new GymPerson: {@GymPersonDto}", GymPersonDto);

            var createdGymPerson = await service.AddAsync(GymPersonDto);

            return Ok(Result<GymPersonRDTO>.Success(createdGymPerson));
        }

        [HttpPut("{id}")]
        [GymAuthorize(
            GymRoleString.Owner,
            GymRoleString.Manager,
            GymRoleString.Coach,
            GymRoleString.Receptionist
        )]
        public async Task<ActionResult> UpdateAsync(int id, [FromBody] GymPersonUDTO GymPersonDto)
        {
            if (!ModelState.IsValid)
            {
                logger.LogWarning("Invalid ModelState while updating GymPerson Id: {Id}", id);
                return BadRequest(ModelState);
            }

            logger.LogInformation("Updating GymPerson with Id: {Id}", id);

            var updatedGymPerson = await service.UpdateAsync(id, GymPersonDto);

            logger.LogInformation("Successfully updated GymPerson with Id: {Id}", id);
            return Ok(Result<GymPersonRDTO>.Success(updatedGymPerson));
        }

        [HttpDelete("{id}")]
        [GymAuthorize(
            GymRoleString.Owner,
            GymRoleString.Manager,
            GymRoleString.Coach,
            GymRoleString.Receptionist
        )]
        public async Task<ActionResult> DeleteAsync(int id)
        {
            logger.LogInformation("Deleting GymPerson with Id: {Id}", id);

            var deletedGymPerson = await service.DeleteAsync(id);

            logger.LogInformation("Successfully deleted GymPerson with Id: {Id}", id);
            return Ok(Result<GymPersonRDTO>.Success(deletedGymPerson));
        }

        [HttpPost("LinkAccountToGym")]
        [Authorize]
        public async Task<ActionResult<GymPersonRDTO>> LinkAccountToGymAsync([FromQuery] int gymId, [FromQuery] Guid inviteCode, CancellationToken ct = default)
        {
            logger.LogInformation("Linking account to gym with Id: {GymId} using invite code: {InviteCode}", gymId, inviteCode);

            var linkedGymPerson = await service.LinkAccountToGymAsync(gymId, inviteCode, ct);

            logger.LogInformation("Successfully linked account to gym with Id: {GymId}", gymId);
            return Ok(Result<GymPersonRDTO>.Success(linkedGymPerson));
        }

        [HttpPost("{id}/pay-salary")]
        [GymAuthorize(
            GymRoleString.Owner,
            GymRoleString.Manager
        )]
        public async Task<ActionResult> PaySalaryAsync(
            int id,
            [FromQuery] DateTime? salaryValidFrom,
            [FromQuery] DateTime? salaryValidUntil,
            CancellationToken ct = default)
        {
            logger.LogInformation("Paying salary for GymPerson with Id: {Id}", id);
            await service.PaySalaryAsync(id, salaryValidFrom, salaryValidUntil, ct);
            logger.LogInformation("Successfully initiated salary payment for GymPerson with Id: {Id}", id);
            return Ok(Result<string>.Success("Salary payment initiated successfully."));
        }

        [HttpPost("{id}/renew-membership")]
        [GymAuthorize(
            GymRoleString.Owner,
            GymRoleString.Manager,
            GymRoleString.Receptionist
        )]
        public async Task<ActionResult<GymPersonRDTO>> RenewMembershipAsync(
            int id,
            [FromBody] RenewMembershipDTO dto,
            CancellationToken ct = default)
        {
            logger.LogInformation("Renewing membership for GymPerson with Id: {Id}", id);
            var result = await service.RenewMemberSubscriptionAsync(id, dto, ct);
            logger.LogInformation("Successfully renewed membership for GymPerson with Id: {Id}", id);
            return Ok(Result<GymPersonRDTO>.Success(result));
        }

        [HttpPost("{id}/change-status")]
        [GymAuthorize(
            GymRoleString.Owner,
            GymRoleString.Manager
        )]
        public async Task<ActionResult<GymPersonRDTO>> ChangeStatusAsync(
            int id,
            [FromBody] UpdateAccessStatusDTO dto,
            CancellationToken ct = default)
        {
            logger.LogInformation("Changing access status for GymPerson with Id: {Id}", id);
            var result = await service.UpdateAccessStatusAsync(id, dto, ct);
            logger.LogInformation("Successfully changed access status for GymPerson with Id: {Id}", id);
            return Ok(Result<GymPersonRDTO>.Success(result));
        }

        [HttpPost("leave-gym/{gymId}")]
        [Authorize]
        public async Task<ActionResult> LeaveGymAsync(int gymId, CancellationToken ct = default)
        {
            logger.LogInformation("User leaving gym with ID: {GymId}", gymId);
            await service.LeaveGymAsync(gymId, ct);
            logger.LogInformation("User successfully left gym with ID: {GymId}", gymId);
            return Ok(Result<string>.Success("Successfully left the gym."));
        }
    }
}
