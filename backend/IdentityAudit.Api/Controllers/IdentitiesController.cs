using IdentityAudit.Application.Audits;
using IdentityAudit.Application.Identities;
using Microsoft.AspNetCore.Mvc;

namespace IdentityAudit.Api.Controllers;

[ApiController]
[Route("api/audits/{auditId:guid}/identities")]
public sealed class IdentitiesController : ControllerBase
{
    private readonly IIdentityService _identityService;
    private readonly IAuditService _auditService;

    public IdentitiesController(
        IIdentityService identityService,
        IAuditService auditService)
    {
        _identityService = identityService;
        _auditService = auditService;
    }

    [HttpGet]
    public async Task<ActionResult<
        IReadOnlyList<DirectoryIdentityDto>>> GetByAuditId(
            Guid auditId,
            CancellationToken cancellationToken)
    {
        var audit = await _auditService.GetByIdAsync(
            auditId,
            cancellationToken);

        if (audit is null)
        {
            return NotFound(new
            {
                message = "L'audit demandé est introuvable."
            });
        }

        var identities =
            await _identityService.GetByAuditIdAsync(
                auditId,
                cancellationToken);

        return Ok(identities);
    }

    [HttpPost]
    public async Task<IActionResult> Import(
        Guid auditId,
        [FromBody] ImportIdentitiesRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _identityService.ImportAsync(
            auditId,
            request,
            cancellationToken);

        if (!result.Succeeded)
        {
            return BadRequest(new
            {
                message = result.ErrorMessage
            });
        }

        return CreatedAtAction(
            nameof(GetByAuditId),
            new { auditId },
            new
            {
                importedCount = result.ImportedCount,
                identities = result.Identities
            });
    }
}