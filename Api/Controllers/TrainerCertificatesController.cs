using Application.DTO;
using Application.DTO.Pagintion;
using Application.DTO.TrainerCertificate;
using Application.Interface.Service;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TrainerCertificatesController : ControllerBase
{
    private readonly ITrainerCertificateService _service;
    private readonly ILogger<TrainerCertificatesController> _logger;

    public TrainerCertificatesController(ITrainerCertificateService service, ILogger<TrainerCertificatesController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetPage([FromQuery] PaginatedSearchReq searchReq, CancellationToken ct)
    {
        var result = await _service.GetPageAsync(searchReq, cancellationToken: ct);
        return Ok(Result<PaginatedRes<TrainerCertificateRDTO>>.Success(result));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await _service.GetByIdAsync(id, cancellationToken: ct);
        return Ok(Result<TrainerCertificateRDTO>.Success(result));
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Create([FromForm] TrainerCertificateCDTO dto, CancellationToken ct)
    {
        var result = await _service.AddAsync(dto, ct);
        return Ok(Result<TrainerCertificateRDTO>.Success(result));
    }

    [HttpPut("{id}")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Update(int id, [FromForm] TrainerCertificateUDTO dto, CancellationToken ct)
    {
        var result = await _service.UpdateAsync(id, dto, ct);
        return Ok(Result<TrainerCertificateRDTO>.Success(result));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var result = await _service.DeleteAsync(id, ct);
        return Ok(Result<TrainerCertificateRDTO>.Success(result));
    }
}
