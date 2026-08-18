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
    [Authorize]
    public class CouponRedemptionController(ILogger<CouponRedemptionController> logger, ICouponRedemptionService service) : ControllerBase
    {


        [HttpPost]
        public async Task<ActionResult<Result<PaginatedRes<CouponRedemptionRDTO>>>> GetPagedAsync([FromBody] PaginatedSearchReq searchReq, CancellationToken cancellationToken = default)
        {
            logger.LogInformation("Fetching all CouponRedemptions");
            PaginatedRes<CouponRedemptionRDTO> CouponRedemptions = await service.GetPageAsync(searchReq, false, cancellationToken);
            logger.LogInformation("Successfully fetched all CouponRedemptions");
            return Ok(Result<PaginatedRes<CouponRedemptionRDTO>>.Success(CouponRedemptions));
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<Result<CouponRedemptionRDTO>>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            logger.LogInformation("Fetching CouponRedemption with Id: {Id}", id);

            var CouponRedemption = await service.GetByIdAsync(id, false, cancellationToken: cancellationToken);

            logger.LogInformation("Successfully fetched CouponRedemption with Id: {Id}", id);
            return Ok(Result<CouponRedemptionRDTO>.Success(CouponRedemption));
        }
    }
}