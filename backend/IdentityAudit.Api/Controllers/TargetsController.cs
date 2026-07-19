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
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
    Guid id,
    [FromBody] UpdateTargetRequest request,
    CancellationToken cancellationToken)
    {
        var result = await _targetService.UpdateAsync(
            id,
            request,
            cancellationToken);

        if (!result.Succeeded)
        {
            return NotFound(new
            {
                message = result.ErrorMessage
            });
        }

        return Ok(result.Target);
    }
    [HttpPost("{id:guid}/test-connection")]
    public async Task<IActionResult> TestConnection(
    Guid id,
    CancellationToken cancellationToken)
    {
        var result = await _targetService.TestConnectionAsync(
            id,
            cancellationToken);

        if (!result.TargetFound)
        {
            return NotFound(new
            {
                message = result.Message,
                testedAt = result.TestedAt
            });
        }

        if (!result.Succeeded)
        {
            return BadRequest(new
            {
                succeeded = result.Succeeded,
                message = result.Message,
                testedAt = result.TestedAt
            });
        }

        return Ok(new
        {
            succeeded = result.Succeeded,
            message = result.Message,
            testedAt = result.TestedAt
        });
    }
}