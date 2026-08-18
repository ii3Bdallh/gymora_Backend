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
        [Authorize(Roles = $"{AppRole.SuperAdmin}")]
        public async Task<ActionResult<Result<PaginatedRes<PaymentRequestRDTO>>>> GetPagedAsync([FromBody] PaginatedSearchReq searchReq)
        {
            logger.LogInformation("Fetching all PaymentRequests");
            PaginatedRes<PaymentRequestRDTO> PaymentRequests = await service.GetPageAsync(searchReq, false);
            logger.LogInformation("Successfully fetched all PaymentRequests");
            return Ok(Result<PaginatedRes<PaymentRequestRDTO>>.Success(PaymentRequests));
        }
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<Result<PaymentRequestRDTO>>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            logger.LogInformation("Fetching PaymentRequest with Id: {Id}", id);

            var PaymentRequest = await service.GetByIdAsync(id, false, cancellationToken: cancellationToken);

            logger.LogInformation("Successfully fetched PaymentRequest with Id: {Id}", id);
            return Ok(Result<PaymentRequestRDTO>.Success(PaymentRequest));
        }

        [HttpPost("Create")]
        [Authorize]

        public async Task<ActionResult<Result<PaymentRequestRDTO>>> CreateAsync([FromForm] PaymentRequestCDTO PaymentRequestDto)
        {
            if (!ModelState.IsValid)
            {
                logger.LogWarning("Invalid ModelState while creating PaymentRequest: {@PaymentRequestDto}", PaymentRequestDto);
                return BadRequest(ModelState);
            }

            logger.LogInformation("Creating a new PaymentRequest: {@PaymentRequestDto}", PaymentRequestDto);
            PaymentRequestDto.IsPublic = false;
            var createdPaymentRequest = await service.AddAsync(PaymentRequestDto);


            return Ok(Result<PaymentRequestRDTO>.Success(createdPaymentRequest));
        }

        // [HttpPut("{id}")]
        // [Authorize(Roles = $"{AppRole.SuperAdmin}")]

        // public async Task<ActionResult> UpdateAsync(int id, [FromForm] PaymentRequestUDTO PaymentRequestDto)
        // {

        //     if (!ModelState.IsValid)
        //     {
        //         logger.LogWarning("Invalid ModelState while updating PaymentRequest Id: {Id}", id);
        //         return BadRequest(ModelState);
        //     }

        //     logger.LogInformation("Updating PaymentRequest with Id: {Id}", id);

        //     var updatedPaymentRequest = await service.UpdateAsync(id, PaymentRequestDto);

        //     logger.LogInformation("Successfully updated PaymentRequest with Id: {Id}", id);
        //     return Ok(Result<PaymentRequestRDTO>.Success(updatedPaymentRequest));
        // }

        [HttpPut("Approve/{id}")]
        [Authorize(Roles = $"{AppRole.SuperAdmin}")]
        public async Task<ActionResult<Result<PaymentRequestRDTO>>> ApproveAsync(int id, PaymentRequestApprove dto)
        {
            logger.LogInformation("Approving PaymentRequest with Id: {Id}", id);

            var approvedPaymentRequest = await service.ApproveAsync(id, dto);

            logger.LogInformation("Successfully approved PaymentRequest with Id: {Id}", id);
            return Ok(Result<PaymentRequestRDTO>.Success(approvedPaymentRequest));
        }

        [HttpPut("Reject/{id}")]
        [Authorize(Roles = $"{AppRole.SuperAdmin}")]
        public async Task<ActionResult<Result<PaymentRequestRDTO>>> RejectAsync(int id, PaymentRequestReject dto)
        {
            logger.LogInformation("Rejecting PaymentRequest with Id: {Id}", id);

            var rejectedPaymentRequest = await service.RejectAsync(id, dto);

            logger.LogInformation("Successfully rejected PaymentRequest with Id: {Id}", id);
            return Ok(Result<PaymentRequestRDTO>.Success(rejectedPaymentRequest));
        }

        // [HttpDelete("{id}")]
        // [Authorize(Roles = $"{AppRole.SuperAdmin}")]

        // public async Task<ActionResult> DeleteAsync(int id, PaymentRequestReject dto)
        // {
        //     logger.LogInformation("Deleting PaymentRequest with Id: {Id}", id);

        //     var deletedPaymentRequest = await service.DeleteAsync(id);

        //     logger.LogInformation("Successfully deleted PaymentRequest with Id: {Id}", id);
        //     return Ok(Result<PaymentRequestRDTO>.Success(deletedPaymentRequest));
        // }


    }
}