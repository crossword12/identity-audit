using IdentityAudit.Application.Targets;
using Microsoft.AspNetCore.Mvc;

namespace IdentityAudit.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class TargetsController : ControllerBase
{
    private readonly ITargetService _targetService;

    public TargetsController(ITargetService targetService)
    {
        _targetService = targetService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TargetDto>>> GetAll(
        CancellationToken cancellationToken)
    {
        var targets = await _targetService.GetAllAsync(
            cancellationToken);

        return Ok(targets);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TargetDto>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var target = await _targetService.GetByIdAsync(
            id,
            cancellationToken);

        if (target is null)
        {
            return NotFound(new
            {
                message = "La cible demandée est introuvable."
            });
        }

        return Ok(target);
    }

    [HttpPost]
    public async Task<ActionResult<TargetDto>> Create(
        [FromBody] CreateTargetRequest request,
        CancellationToken cancellationToken)
    {
        var target = await _targetService.CreateAsync(
            request,
            cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = target.Id },
            target);
    }
}