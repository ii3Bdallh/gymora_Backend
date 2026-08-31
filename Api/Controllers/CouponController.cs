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
        public async Task<ActionResult<Result<PaginatedRes<CouponRDTO>>>> GetPagedAsync([FromBody] PaginatedSearchReq searchReq, CancellationToken cancellationToken = default)
        {
            logger.LogInformation("Fetching all Coupons");
            PaginatedRes<CouponRDTO> Coupons = await service.GetPageAsync(searchReq, false, cancellationToken);
            logger.LogInformation("Successfully fetched all Coupons");
            return Ok(Result<PaginatedRes<CouponRDTO>>.Success(Coupons));
        }

        [HttpGet("{id}")]
        [Authorize(Roles = $"{AppRole.SuperAdmin}")]
        public async Task<ActionResult<Result<CouponRDTO>>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            logger.LogInformation("Fetching Coupon with Id: {Id}", id);

            var Coupon = await service.GetByIdAsync(id, false, cancellationToken: cancellationToken);

            logger.LogInformation("Successfully fetched Coupon with Id: {Id}", id);
            return Ok(Result<CouponRDTO>.Success(Coupon));
        }

        [HttpPost("Create")]
        [Authorize(Roles = $"{AppRole.SuperAdmin}")]
        public async Task<ActionResult<Result<CouponRDTO>>> CreateAsync([FromBody] CouponCDTO CouponDto, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                logger.LogWarning("Invalid ModelState while creating Coupon: {@CouponDto}", CouponDto);
                return BadRequest(ModelState);
            }

            logger.LogInformation("Creating a new Coupon: {@CouponDto}", CouponDto);

            var createdCoupon = await service.AddAsync(CouponDto, cancellationToken);

            return Ok(Result<CouponRDTO>.Success(createdCoupon));
        }

        [HttpPut("{id}")]
        [Authorize(Roles = $"{AppRole.SuperAdmin}")]
        public async Task<ActionResult<Result<CouponRDTO>>> UpdateAsync(int id, [FromBody] CouponUDTO CouponDto, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                logger.LogWarning("Invalid ModelState while updating Coupon Id: {Id}", id);
                return BadRequest(ModelState);
            }

            logger.LogInformation("Updating Coupon with Id: {Id}", id);

            var updatedCoupon = await service.UpdateAsync(id, CouponDto, cancellationToken);

            logger.LogInformation("Successfully updated Coupon with Id: {Id}", id);
            return Ok(Result<CouponRDTO>.Success(updatedCoupon));
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = $"{AppRole.SuperAdmin}")]
        public async Task<ActionResult<Result<CouponRDTO>>> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            logger.LogInformation("Deleting Coupon with Id: {Id}", id);

            var deletedCoupon = await service.DeleteAsync(id, cancellationToken);

            logger.LogInformation("Successfully deleted Coupon with Id: {Id}", id);
            return Ok(Result<CouponRDTO>.Success(deletedCoupon));
        }

        [HttpGet("Validate/{code}")]
        [Authorize]
        public async Task<ActionResult<Result<CouponValidationResult>>> ValidateCouponAsync(string code, decimal orderAmount, CancellationToken cancellationToken = default)
        {
            logger.LogInformation("Validating Coupon with Code: {Code}", code);

            var couponResult = await service.ValidateCouponAsync(code, orderAmount, 0, cancellationToken);

            logger.LogInformation("Successfully validated Coupon with Code: {Code}", code);
            return Ok(Result<CouponValidationResult>.Success(couponResult));
        }

    }
}