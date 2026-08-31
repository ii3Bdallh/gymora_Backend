using Application.DTO;
using Application.DTO.Model;
using Application.DTO.Pagintion;
using Application.Interface.Service;
using Application.Interface.Service.Shared;
using Application.Model;
using Domain.Enum;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;


namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OwnerSubscriptionController(ILogger<OwnerSubscriptionController> logger, IOwnerSubscriptionService service, ICurrentPlanService currentPlanService, CurrentUser currentUser) : ControllerBase
    {


        [HttpPost]
        [Authorize(Roles = $"{AppRole.SuperAdmin}")]
        public async Task<ActionResult<Result<PaginatedRes<OwnerSubscriptionRDTO>>>> GetPagedAsync([FromBody] PaginatedSearchReq searchReq)
        {
            logger.LogInformation("Fetching all OwnerSubscriptions");
            PaginatedRes<OwnerSubscriptionRDTO> OwnerSubscriptions = await service.GetPageAsync(searchReq, false);
            logger.LogInformation("Successfully fetched all OwnerSubscriptions");
            return Ok(Result<PaginatedRes<OwnerSubscriptionRDTO>>.Success(OwnerSubscriptions));
        }
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<Result<OwnerSubscriptionRDTO>>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            logger.LogInformation("Fetching OwnerSubscription with Id: {Id}", id);

            var OwnerSubscription = await service.GetByIdDetailsAsync(id, cancellationToken);

            logger.LogInformation("Successfully fetched OwnerSubscription with Id: {Id}", id);
            return Ok(Result<OwnerSubscriptionRDTO>.Success(OwnerSubscription));
        }

        [HttpGet("get-my-current-subscription")]
        [Authorize]
        public async Task<ActionResult<Result<CurrentPlanResult>>> GetMySubscriptionsAsync(CancellationToken cancellationToken = default)
        {
            logger.LogInformation("Fetching my OwnerSubscriptions");

            CurrentPlanResult OwnerSubscriptions = await currentPlanService.GetCurrentPlanAsync(currentUser.UserId, ct: cancellationToken);

            logger.LogInformation("Successfully fetched my OwnerSubscriptions");
            return Ok(Result<CurrentPlanResult>.Success(OwnerSubscriptions));
        }

        // [HttpPost("Create")]
        // [Authorize]
        // public async Task<ActionResult<OwnerSubscriptionRDTO>> CreateAsync([FromBody] OwnerSubscriptionCDTO OwnerSubscriptionDto)
        // {
        //     if (!ModelState.IsValid)
        //     {
        //         logger.LogWarning("Invalid ModelState while creating OwnerSubscription: {@OwnerSubscriptionDto}", OwnerSubscriptionDto);
        //         return BadRequest(ModelState);
        //     }

        //     logger.LogInformation("Creating a new OwnerSubscription: {@OwnerSubscriptionDto}", OwnerSubscriptionDto);

        //     var createdOwnerSubscription = await service.AddAsync(OwnerSubscriptionDto);


        //     return Ok(Result<OwnerSubscriptionRDTO>.Success(createdOwnerSubscription));
        // }

        // [HttpPut("{id}")]
        // [Authorize]

        // public async Task<ActionResult> UpdateAsync(int id, [FromBody] OwnerSubscriptionUDTO OwnerSubscriptionDto )
        // {

        //     if (!ModelState.IsValid)
        //     {
        //         logger.LogWarning("Invalid ModelState while updating OwnerSubscription Id: {Id}", id);
        //         return BadRequest(ModelState);
        //     }

        //     logger.LogInformation("Updating OwnerSubscription with Id: {Id}", id);

        //     var updatedOwnerSubscription = await service.UpdateAsync(id, OwnerSubscriptionDto);

        //     logger.LogInformation("Successfully updated OwnerSubscription with Id: {Id}", id);
        //     return Ok(Result<OwnerSubscriptionRDTO>.Success(updatedOwnerSubscription));
        // }

        // [HttpDelete("{id}")]
        // [Authorize]

        // public async Task<ActionResult> DeleteAsync(int id)
        // {
        //     logger.LogInformation("Deleting OwnerSubscription with Id: {Id}", id);

        //     var deletedOwnerSubscription = await service.DeleteAsync(id );

        //     logger.LogInformation("Successfully deleted OwnerSubscription with Id: {Id}", id);
        //     return Ok(Result<OwnerSubscriptionRDTO>.Success(deletedOwnerSubscription));
        // }


    }
}