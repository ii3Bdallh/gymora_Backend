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
using System.Threading.Tasks;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ExpensesController(IExpenseService service, ILogger<ExpensesController> logger) : ControllerBase
    {
        [HttpPost]
        [GymAuthorize(GymRoleString.Owner, GymRoleString.Manager)]
        public async Task<ActionResult<Result<PaginatedRes<ExpenseRDTO>>>> GetPaged([FromBody] PaginatedSearchReq searchReq)
        {
            logger.LogInformation("Fetching expenses");
            var result = await service.GetPageAsync(searchReq, false);
            return Ok(Result<PaginatedRes<ExpenseRDTO>>.Success(result));
        }

        [HttpGet("{id}")]
        [GymAuthorize(GymRoleString.Owner, GymRoleString.Manager)]
        public async Task<ActionResult<Result<ExpenseRDTO>>> GetById(int id, CancellationToken ct)
        {
            logger.LogInformation("Fetching expense with Id: {Id}", id);
            var result = await service.GetByIdAsync(id, false, ct);
            return Ok(Result<ExpenseRDTO>.Success(result));
        }

        [HttpPost("Create")]
        [GymAuthorize(GymRoleString.Owner, GymRoleString.Manager)]
        public async Task<ActionResult<Result<ExpenseRDTO>>> Create([FromBody] ExpenseCDTO dto, CancellationToken ct)
        {
            logger.LogInformation("Creating expense: {@Dto}", dto);
            var result = await service.AddAsync(dto, ct);
            return StatusCode(StatusCodes.Status201Created, Result<ExpenseRDTO>.Success(result));
        }

        [HttpPut("{id}")]
        [GymAuthorize(GymRoleString.Owner, GymRoleString.Manager)]
        public async Task<ActionResult<Result<ExpenseRDTO>>> Update(int id, [FromBody] ExpenseUDTO dto, CancellationToken ct)
        {
            logger.LogInformation("Updating expense with Id: {Id}", id);
            var result = await service.UpdateAsync(id, dto, ct);
            return Ok(Result<ExpenseRDTO>.Success(result));
        }

        [HttpDelete("{id}")]
        [GymAuthorize(GymRoleString.Owner, GymRoleString.Manager)]
        public async Task<ActionResult<Result<ExpenseRDTO>>> Delete(int id, CancellationToken ct)
        {
            logger.LogInformation("Deleting expense with Id: {Id}", id);
            var result = await service.DeleteAsync(id, ct);
            return Ok(Result<ExpenseRDTO>.Success(result));
        }
    }
}
