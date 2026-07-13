using Application.DTO;
using Application.DTO.Pagintion;
using Application.Interface.Service;
using Domain.Enum;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Application.Interface.Service.Entity;
using Application.DTO.CRUD.Create;
using Application.DTO.CRUD.Read;
using Application.DTO.CRUD.Update;
using Domain.Model;


namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = nameof(RoleType.SuperAdmin))]
    public class SubscriptionPlanController(ILogger<SubscriptionPlanController> logger, ISubscriptionPlanService service) : ControllerBase
    {


        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<SubscriptionPlanRDTO>>> GetPagedAsync([FromBody] PaginatedSearchReq searchReq)
        {
            logger.LogInformation("Fetching all SubscriptionPlans");
            PaginatedRes<SubscriptionPlanRDTO> SubscriptionPlans = await service.GetPageAsync(searchReq, true);
            logger.LogInformation("Successfully fetched all SubscriptionPlans");
            return Ok(Result<PaginatedRes<SubscriptionPlanRDTO>>.Success(SubscriptionPlans));
        }
        [HttpGet("{id}")]
        [AllowAnonymous]

        public async Task<ActionResult<SubscriptionPlanRDTO>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            logger.LogInformation("Fetching SubscriptionPlan with Id: {Id}", id);

            var SubscriptionPlan = await service.GetByIdAsync(id, true, cancellationToken: cancellationToken);

            logger.LogInformation("Successfully fetched SubscriptionPlan with Id: {Id}", id);
            return Ok(Result<SubscriptionPlanRDTO>.Success(SubscriptionPlan));
        }

        #region Plans

        [HttpPost("Create")]
        public async Task<ActionResult<SubscriptionPlanRDTO>> CreateAsync([FromBody] SubscriptionPlanCDTO SubscriptionPlanDto)
        {
            if (!ModelState.IsValid)
            {
                logger.LogWarning("Invalid ModelState while creating SubscriptionPlan: {@SubscriptionPlanDto}", SubscriptionPlanDto);
                return BadRequest(ModelState);
            }

            logger.LogInformation("Creating a new SubscriptionPlan: {@SubscriptionPlanDto}", SubscriptionPlanDto);

            var createdSubscriptionPlan = await service.AddAsync(SubscriptionPlanDto);


            return Ok(Result<SubscriptionPlanRDTO>.Success(createdSubscriptionPlan));
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateAsync(int id, [FromBody] SubscriptionPlanUDTO SubscriptionPlanDto)
        {

            if (!ModelState.IsValid)
            {
                logger.LogWarning("Invalid ModelState while updating SubscriptionPlan Id: {Id}", id);
                return BadRequest(ModelState);
            }

            logger.LogInformation("Updating SubscriptionPlan with Id: {Id}", id);

            var updatedSubscriptionPlan = await service.UpdateAsync(id, SubscriptionPlanDto);

            logger.LogInformation("Successfully updated SubscriptionPlan with Id: {Id}", id);
            return Ok(Result<SubscriptionPlanRDTO>.Success(updatedSubscriptionPlan));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteAsync(int id)
        {
            logger.LogInformation("Deleting SubscriptionPlan with Id: {Id}", id);

            var deletedSubscriptionPlan = await service.DeleteAsync(id);

            logger.LogInformation("Successfully deleted SubscriptionPlan with Id: {Id}", id);
            return Ok(Result<SubscriptionPlanRDTO>.Success(deletedSubscriptionPlan));
        }

        #endregion

        #region PlanPrice

        [HttpPost("{PlanId}/PlanPrices/Create")]
        public async Task<ActionResult<PlanPriceRDTO>> CreatePlanPriceAsync(int PlanId, [FromBody] PlanPriceCDTO planPriceDto)
        {
            if (!ModelState.IsValid)
            {
                logger.LogWarning("Invalid ModelState while creating PlanPrice: {@PlanPriceDto}", planPriceDto);
                return BadRequest(ModelState);
            }

            logger.LogInformation("Creating a new PlanPrice: {@PlanPriceDto}", planPriceDto);

            PlanPriceRDTO createdPlanPrice = await service.AddPlanPriceAsync(PlanId, planPriceDto);

            return Ok(Result<PlanPriceRDTO>.Success(createdPlanPrice));
        }

        [HttpPut("{PlanId}/PlanPrices/{id}")]
        public async Task<ActionResult<PlanPriceRDTO>> UpdatePlanPriceAsync(int PlanId, int id, [FromBody] PlanPriceUDTO planPriceDto)
        {
            if (!ModelState.IsValid)
            {
                logger.LogWarning("Invalid ModelState while updating PlanPrice Id: {Id}", id);
                return BadRequest(ModelState);
            }

            logger.LogInformation("Updating PlanPrice with Id: {Id}", id);

            PlanPriceRDTO updatedPlanPrice = await service.UpdatePlanPriceAsync(id, planPriceDto);

            logger.LogInformation("Successfully updated PlanPrice with Id: {Id}", id);
            return Ok(Result<PlanPriceRDTO>.Success(updatedPlanPrice));
        }

        [HttpDelete("PlanPrices/{id}")]
        public async Task<ActionResult<PlanPrice>> DeletePlanPriceAsync(int id)
        {
            logger.LogInformation("Deleting PlanPrice with Id: {Id}", id);

            PlanPriceRDTO deletedPlanPrice = await service.DeletePlanPriceAsync(id);

            logger.LogInformation("Successfully deleted PlanPrice with Id: {Id}", id);
            return Ok(Result<PlanPriceRDTO>.Success(deletedPlanPrice));
        }


        #endregion

    }
}


