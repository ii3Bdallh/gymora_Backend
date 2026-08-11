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
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InvitationController(
        IInvitationService service,
        ILogger<InvitationController> logger) : ControllerBase
    {
        [HttpPost]
        [GymAuthorize(GymRoleString.Owner, GymRoleString.Manager)]
        public async Task<ActionResult<IEnumerable<InvitationRDTO>>> GetPagedAsync([FromBody] PaginatedSearchReq searchReq)
        {
            logger.LogInformation("Fetching paginated invitations");
            PaginatedRes<InvitationRDTO> invitations = await service.GetPageAsync(searchReq, false);
            logger.LogInformation("Successfully fetched paginated invitations");
            return Ok(Result<PaginatedRes<InvitationRDTO>>.Success(invitations));
        }

        [HttpPost("my-invitations")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<InvitationRDTO>>> GetMyInvitationsAsync([FromBody] GetMyInvitationsPagedReq searchReq)
        {
            logger.LogInformation("Fetching paginated invitations");
            PaginatedRes<InvitationRDTO> invitations = await service.GetPageAsync(searchReq, false);
            logger.LogInformation("Successfully fetched paginated invitations");
            return Ok(Result<PaginatedRes<InvitationRDTO>>.Success(invitations));
        }

        [HttpGet("{id}")]
        [GymAuthorize(GymRoleString.Owner, GymRoleString.Manager)]
        public async Task<ActionResult<InvitationRDTO>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            logger.LogInformation("Fetching Invitation with Id: {Id}", id);
            var invitation = await service.GetByIdAsync(id, false, cancellationToken: cancellationToken);
            logger.LogInformation("Successfully fetched Invitation with Id: {Id}", id);
            return Ok(Result<InvitationRDTO>.Success(invitation));
        }

        [HttpPost("Create")]
        [GymAuthorize(GymRoleString.Owner, GymRoleString.Manager)]
        public async Task<ActionResult<InvitationRDTO>> CreateAsync([FromBody] InvitationCDTO dto)
        {
            if (!ModelState.IsValid)
            {
                logger.LogWarning("Invalid ModelState while creating invitation: {@dto}", dto);
                return BadRequest(ModelState);
            }

            logger.LogInformation("Creating a new invitation: {@dto}", dto);
            var createdInvitation = await service.CreateInvitationAsync(dto);
            return Ok(Result<InvitationRDTO>.Success(createdInvitation));
        }

        [HttpPost("accept/{id}")]
        [Authorize]
        public async Task<ActionResult<InvitationRDTO>> AcceptAsync(int id, CancellationToken ct = default)
        {
            logger.LogInformation("Accepting invitation with ID: {id}", id);
            var result = await service.AcceptInvitationAsync(id, ct);
            logger.LogInformation("Successfully accepted invitation with ID: {id}", id);
            return Ok(Result<InvitationRDTO>.Success(result));
        }

        [HttpPost("reject/{id}")]
        [Authorize]
        public async Task<ActionResult<InvitationRDTO>> RejectAsync(int id, CancellationToken ct = default)
        {
            logger.LogInformation("Rejecting invitation with ID: {id}", id);
            var result = await service.RejectInvitationAsync(id, ct);
            logger.LogInformation("Successfully rejected invitation with ID: {id}", id);
            return Ok(Result<InvitationRDTO>.Success(result));
        }

        [HttpPost("cancel/{id}")]
        [GymAuthorize(GymRoleString.Owner, GymRoleString.Manager)]
        public async Task<ActionResult<InvitationRDTO>> CancelAsync(int id, CancellationToken ct = default)
        {
            logger.LogInformation("Cancelling invitation with ID: {id}", id);
            var result = await service.CancelInvitationAsync(id, ct);
            logger.LogInformation("Successfully cancelled invitation with ID: {id}", id);
            return Ok(Result<InvitationRDTO>.Success(result));
        }
    }
}
