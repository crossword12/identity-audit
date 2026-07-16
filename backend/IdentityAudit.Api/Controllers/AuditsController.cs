using IdentityAudit.Application.Audits;
using Microsoft.AspNetCore.Mvc;

namespace IdentityAudit.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuditsController : ControllerBase
{
    private readonly IAuditService _auditService;

    public AuditsController(IAuditService auditService)
    {
        _auditService = auditService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AuditDto>>> GetAll(
        CancellationToken cancellationToken)
    {
        var audits = await _auditService.GetAllAsync(
            cancellationToken);

        return Ok(audits);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AuditDto>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var audit = await _auditService.GetByIdAsync(
            id,
            cancellationToken);

        if (audit is null)
        {
            return NotFound(new
            {
                message = "L’audit demandé est introuvable."
            });
        }

        return Ok(audit);
    }

    [HttpPost]
    public async Task<ActionResult<AuditDto>> Create(
        [FromBody] CreateAuditRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _auditService.CreateAsync(
            request,
            cancellationToken);

        if (!result.IsSuccess || result.Audit is null)
        {
            return BadRequest(new
            {
                message = result.Error
            });
        }

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Audit.Id },
            result.Audit);
    }
}