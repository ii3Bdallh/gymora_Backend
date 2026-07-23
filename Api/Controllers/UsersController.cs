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
    public class UsersController(ILogger<UsersController> logger, IUsersService service) : ControllerBase
    {


        [HttpPost]
        public async Task<ActionResult<IEnumerable<ApplicationUserRDTO>>> GetPagedAsync([FromBody] PaginatedSearchReq searchReq)
        {
            logger.LogInformation("Fetching all Userss");
            PaginatedRes<ApplicationUserRDTO> Userss = await service.GetPageAsync(searchReq, true);
            logger.LogInformation("Successfully fetched all Userss");
            return Ok(Result<PaginatedRes<ApplicationUserRDTO>>.Success(Userss));
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<ApplicationUserRDTO>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            logger.LogInformation("Fetching Users with Id: {Id}", id);

            var Users = await service.GetByIdAsync(id, true, cancellationToken: cancellationToken);

            logger.LogInformation("Successfully fetched Users with Id: {Id}", id);
            return Ok(Result<ApplicationUserRDTO>.Success(Users));
        }



    }
}