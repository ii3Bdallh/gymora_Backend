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
    public class PaymentRequestController(ILogger<PaymentRequestController> logger, IPaymentRequestService service) : ControllerBase
    {


        [HttpPost]
        [Authorize(Roles =$"{AppRole.SuperAdmin}")]
        public async Task<ActionResult<IEnumerable<PaymentRequestRDTO>>> GetPagedAsync([FromBody] PaginatedSearchReq searchReq)
        {
            logger.LogInformation("Fetching all PaymentRequests");
            PaginatedRes<PaymentRequestRDTO> PaymentRequests = await service.GetPageAsync(searchReq, true);
            logger.LogInformation("Successfully fetched all PaymentRequests");
            return Ok(Result<PaginatedRes<PaymentRequestRDTO>>.Success(PaymentRequests));
        }
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<PaymentRequestRDTO>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            logger.LogInformation("Fetching PaymentRequest with Id: {Id}", id);

            var PaymentRequest = await service.GetByIdAsync(id, true, cancellationToken: cancellationToken);

            logger.LogInformation("Successfully fetched PaymentRequest with Id: {Id}", id);
            return Ok(Result<PaymentRequestRDTO>.Success(PaymentRequest));
        }

        [HttpPost("Create")]
        [Authorize]

        public async Task<ActionResult<PaymentRequestRDTO>> CreateAsync([FromForm] PaymentRequestCDTO PaymentRequestDto)
        {
            if (!ModelState.IsValid)
            {
                logger.LogWarning("Invalid ModelState while creating PaymentRequest: {@PaymentRequestDto}", PaymentRequestDto);
                return BadRequest(ModelState);
            }

            logger.LogInformation("Creating a new PaymentRequest: {@PaymentRequestDto}", PaymentRequestDto);

            var createdPaymentRequest = await service.AddAsync(PaymentRequestDto);


            return Ok(Result<PaymentRequestRDTO>.Success(createdPaymentRequest));
        }

        [HttpPut("{id}")]
        [Authorize(Roles = $"{AppRole.SuperAdmin}")]

        public async Task<ActionResult> UpdateAsync(int id, [FromForm] PaymentRequestUDTO PaymentRequestDto)
        {

            if (!ModelState.IsValid)
            {
                logger.LogWarning("Invalid ModelState while updating PaymentRequest Id: {Id}", id);
                return BadRequest(ModelState);
            }

            logger.LogInformation("Updating PaymentRequest with Id: {Id}", id);

            var updatedPaymentRequest = await service.UpdateAsync(id, PaymentRequestDto);

            logger.LogInformation("Successfully updated PaymentRequest with Id: {Id}", id);
            return Ok(Result<PaymentRequestRDTO>.Success(updatedPaymentRequest));
        }

        [HttpDelete("{id}")]
        [Authorize(Roles =$"{AppRole.SuperAdmin}")]

        public async Task<ActionResult> DeleteAsync(int id)
        {
            logger.LogInformation("Deleting PaymentRequest with Id: {Id}", id);

            var deletedPaymentRequest = await service.DeleteAsync(id);

            logger.LogInformation("Successfully deleted PaymentRequest with Id: {Id}", id);
            return Ok(Result<PaymentRequestRDTO>.Success(deletedPaymentRequest));
        }


    }
}