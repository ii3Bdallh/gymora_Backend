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
    public class CouponController(ILogger<CouponController> logger, ICouponService service) : ControllerBase
    {


        [HttpPost]
        [Authorize(Roles = $"{AppRole.SuperAdmin}")]
        public async Task<ActionResult<IEnumerable<CouponRDTO>>> GetPagedAsync([FromBody] PaginatedSearchReq searchReq)
        {
            logger.LogInformation("Fetching all Coupons");
            PaginatedRes<CouponRDTO> Coupons = await service.GetPageAsync(searchReq, false);
            logger.LogInformation("Successfully fetched all Coupons");
            return Ok(Result<PaginatedRes<CouponRDTO>>.Success(Coupons));
        }
        [HttpGet("{id}")]
        [Authorize(Roles = $"{AppRole.SuperAdmin}")]
        public async Task<ActionResult<CouponRDTO>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            logger.LogInformation("Fetching Coupon with Id: {Id}", id);

            var Coupon = await service.GetByIdAsync(id, false, cancellationToken: cancellationToken);

            logger.LogInformation("Successfully fetched Coupon with Id: {Id}", id);
            return Ok(Result<CouponRDTO>.Success(Coupon));
        }

        [HttpPost("Create")]
        [Authorize(Roles = $"{AppRole.SuperAdmin}")]
        public async Task<ActionResult<CouponRDTO>> CreateAsync([FromBody] CouponCDTO CouponDto)
        {
            if (!ModelState.IsValid)
            {
                logger.LogWarning("Invalid ModelState while creating Coupon: {@CouponDto}", CouponDto);
                return BadRequest(ModelState);
            }

            logger.LogInformation("Creating a new Coupon: {@CouponDto}", CouponDto);

            var createdCoupon = await service.AddAsync(CouponDto);


            return Ok(Result<CouponRDTO>.Success(createdCoupon));
        }

        [HttpPut("{id}")]
        [Authorize(Roles = $"{AppRole.SuperAdmin}")]
        public async Task<ActionResult> UpdateAsync(int id, [FromBody] CouponUDTO CouponDto)
        {

            if (!ModelState.IsValid)
            {
                logger.LogWarning("Invalid ModelState while updating Coupon Id: {Id}", id);
                return BadRequest(ModelState);
            }

            logger.LogInformation("Updating Coupon with Id: {Id}", id);

            var updatedCoupon = await service.UpdateAsync(id, CouponDto);

            logger.LogInformation("Successfully updated Coupon with Id: {Id}", id);
            return Ok(Result<CouponRDTO>.Success(updatedCoupon));
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = $"{AppRole.SuperAdmin}")]
        public async Task<ActionResult> DeleteAsync(int id)
        {
            logger.LogInformation("Deleting Coupon with Id: {Id}", id);

            var deletedCoupon = await service.DeleteAsync(id);

            logger.LogInformation("Successfully deleted Coupon with Id: {Id}", id);
            return Ok(Result<CouponRDTO>.Success(deletedCoupon));
        }

        [HttpGet("Validate/{code}")]
        [Authorize(Roles = $"{AppRole.SuperAdmin}")]
        public async Task<ActionResult<CouponValidationResult>> ValidateCouponAsync(string code, decimal orderAmount, CancellationToken cancellationToken = default)
        {
            logger.LogInformation("Validating Coupon with Code: {Code}", code);

            var couponResult = await service.ValidateCouponAsync(code, orderAmount, 0, cancellationToken);

            logger.LogInformation("Successfully validated Coupon with Code: {Code}", code);
            return Ok(Result<CouponValidationResult>.Success(couponResult));
        }

    }
}