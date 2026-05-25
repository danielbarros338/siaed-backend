using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Siaed.Application.Features.Activities.Commands;
using Siaed.Application.Features.Activities.Queries;
using Siaed.Application.Features.Grades.Queries;
using System.Security.Claims;

namespace Siaed.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public sealed class ActivitiesController : ControllerBase
{
    private readonly IMediator _mediator;

    public ActivitiesController(IMediator mediator) => _mediator = mediator;

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateActivityCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value }, new { id = result.Value })
            : BadRequest(new { errors = result.Errors });
    }

    [HttpPost("generate")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Generate([FromBody] GenerateActivityCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value }, new { id = result.Value })
            : BadRequest(new { errors = result.Errors });
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _mediator.Send(new GetActivityByIdQuery(id, userId), ct);
        return result.IsSuccess ? Ok(result.Value) : NotFound(new { errors = result.Errors });
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] Guid teacherId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? status = null, [FromQuery] bool? isAIGenerated = null, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new ListActivitiesQuery(teacherId, page, pageSize, status, isAIGenerated), ct);
        return Ok(result.Value);
    }

    [HttpGet("{id:guid}/grades")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetGrades(Guid id, CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _mediator.Send(new GetActivityGradesQuery(id, userId), ct);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { errors = result.Errors });
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateActivityCommand command, CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var commandWithId = command with { Id = id, RequestingUserId = userId };
        var result = await _mediator.Send(commandWithId, ct);
        if (result.IsSuccess) return NoContent();
        if (result.Errors.Any(e => e.Contains("não encontrado"))) return NotFound(new { errors = result.Errors });
        return BadRequest(new { errors = result.Errors });
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _mediator.Send(new DeleteActivityCommand(id, userId), ct);
        if (result.IsSuccess) return NoContent();
        if (result.Errors.Any(e => e.Contains("não encontrado"))) return NotFound(new { errors = result.Errors });
        return BadRequest(new { errors = result.Errors });
    }

    [HttpPatch("{id:guid}/publish")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Publish(Guid id, CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _mediator.Send(new PublishActivityCommand(id, userId), ct);
        if (result.IsSuccess) return NoContent();
        if (result.Errors.Any(e => e.Contains("não encontrado"))) return NotFound(new { errors = result.Errors });
        return BadRequest(new { errors = result.Errors });
    }

    [HttpPatch("{id:guid}/archive")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Archive(Guid id, CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _mediator.Send(new ArchiveActivityCommand(id, userId), ct);
        if (result.IsSuccess) return NoContent();
        if (result.Errors.Any(e => e.Contains("não encontrado"))) return NotFound(new { errors = result.Errors });
        return BadRequest(new { errors = result.Errors });
    }
}
