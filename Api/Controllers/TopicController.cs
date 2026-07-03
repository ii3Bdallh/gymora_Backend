using Application.DTO;
using Application.DTO.Create;
using Application.DTO.Pagintion;
using Application.DTO.Read;
using Application.DTO.Update;
using Application.Interface.Service;
using Domain.Enum;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TopicController(ILogger<TopicController> logger, ITopicService topicService) : ControllerBase
    {
        [Authorize(Roles = $"{nameof(RoleType.User)},{nameof(RoleType.Admin)},{nameof(RoleType.Owner)},{nameof(RoleType.Guest)}")]
        [HttpPost]
        public async Task<ActionResult<Result<PaginatedRes<TopicRDTO>>>> GetPagedAsync([FromBody] TopicPagedReq searchReq)
        {
            logger.LogInformation("Fetching all Topics");
            var exams = await topicService.GetPageAsync(searchReq);
            return Ok(Result<PaginatedRes<TopicRDTO>>.Success(exams));
        }

        [Authorize(Roles = $"{nameof(RoleType.User)},{nameof(RoleType.Admin)},{nameof(RoleType.Owner)},{nameof(RoleType.Guest)}")]
        [HttpGet("{id}")]
        public async Task<ActionResult<Result<TopicRDTO>>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            logger.LogInformation("Fetching Topic with Id: {Id}", id);
            var exam = await topicService.GetByIdAsync(id, IsActive: true, cancellationToken: cancellationToken);
            return Ok(Result<TopicRDTO>.Success(exam));
        }

        [Authorize(Roles = $"{nameof(RoleType.Owner)},{nameof(RoleType.Admin)},{nameof(RoleType.User)}")]
        [HttpPost("Create")]
        public async Task<ActionResult<Result<TopicRDTO>>> CreateAsync([FromBody] TopicCDTO examDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(Result<string>.Failure("VALIDATION_ERROR", "Invalid input"));

            var createdTopic = await topicService.AddAsync(examDto);
            return Ok(Result<TopicRDTO>.Success(createdTopic));
        }

        [Authorize(Roles = $"{nameof(RoleType.Admin)},{nameof(RoleType.Owner)},{nameof(RoleType.User)}")]
        [HttpPut("{id}")]
        public async Task<ActionResult<Result<TopicRDTO>>> UpdateAsync(int id, [FromBody] TopicUDTO examDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(Result<string>.Failure("VALIDATION_ERROR", "Invalid input"));

            var updatedTopic = await topicService.UpdateAsync(id, examDto);
            return Ok(Result<TopicRDTO>.Success(updatedTopic));
        }

        [Authorize(Roles = $"{nameof(RoleType.Admin)},{nameof(RoleType.Owner)}")]
        [HttpDelete("{id}")]
        public async Task<ActionResult<Result<TopicRDTO>>> DeleteAsync(int id)
        {
            var deletedTopic = await topicService.DeleteAsync(id);
            return Ok(Result<TopicRDTO>.Success(deletedTopic));
        }
    }
}
