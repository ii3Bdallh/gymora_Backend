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
    [Authorize(Roles = $"{AppRole.SuperAdmin}")]
    public class CouponRedemptionController(ILogger<CouponRedemptionController> logger, ICouponRedemptionService service) : ControllerBase
    {

 
        [HttpPost]
        public async Task <ActionResult<IEnumerable<CouponRedemptionRDTO>>> GetPagedAsync([FromBody] PaginatedSearchReq searchReq)
        {
            logger.LogInformation("Fetching all CouponRedemptions");
            PaginatedRes<CouponRedemptionRDTO> CouponRedemptions = await service.GetPageAsync(searchReq , false);
            logger.LogInformation("Successfully fetched all CouponRedemptions");
            return Ok(Result<PaginatedRes<CouponRedemptionRDTO>>.Success(CouponRedemptions));
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<CouponRedemptionRDTO>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            logger.LogInformation("Fetching CouponRedemption with Id: {Id}", id);

            var CouponRedemption = await service.GetByIdAsync(id, false, cancellationToken: cancellationToken);

            logger.LogInformation("Successfully fetched CouponRedemption with Id: {Id}", id);
            return Ok(Result<CouponRedemptionRDTO>.Success(CouponRedemption));
        }

        // [HttpPost("Create")]
        // public async Task<ActionResult<CouponRedemptionRDTO>> CreateAsync([FromBody] CouponRedemptionCDTO CouponRedemptionDto)
        // {
        //     if (!ModelState.IsValid)
        //     {
        //         logger.LogWarning("Invalid ModelState while creating CouponRedemption: {@CouponRedemptionDto}", CouponRedemptionDto);
        //         return BadRequest(ModelState);
        //     }

        //     logger.LogInformation("Creating a new CouponRedemption: {@CouponRedemptionDto}", CouponRedemptionDto);

        //     var createdCouponRedemption = await service.AddAsync(CouponRedemptionDto);


        //     return Ok(Result<CouponRedemptionRDTO>.Success(createdCouponRedemption));
        // }

        // [HttpPut("{id}")]

        // public async Task<ActionResult> UpdateAsync(int id, [FromBody] CouponRedemptionUDTO CouponRedemptionDto )
        // {

        //     if (!ModelState.IsValid)
        //     {
        //         logger.LogWarning("Invalid ModelState while updating CouponRedemption Id: {Id}", id);
        //         return BadRequest(ModelState);
        //     }

        //     logger.LogInformation("Updating CouponRedemption with Id: {Id}", id);

        //     var updatedCouponRedemption = await service.UpdateAsync(id, CouponRedemptionDto);

        //     logger.LogInformation("Successfully updated CouponRedemption with Id: {Id}", id);
        //     return Ok(Result<CouponRedemptionRDTO>.Success(updatedCouponRedemption));
        // }

        // [HttpDelete("{id}")]

        // public async Task<ActionResult> DeleteAsync(int id)
        // {
        //     logger.LogInformation("Deleting CouponRedemption with Id: {Id}", id);

        //     var deletedCouponRedemption = await service.DeleteAsync(id );

        //     logger.LogInformation("Successfully deleted CouponRedemption with Id: {Id}", id);
        //     return Ok(Result<CouponRedemptionRDTO>.Success(deletedCouponRedemption));
        // }


    }
}