using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Siaed.Application.Features.Reports.Commands;
using Siaed.Application.Features.Reports.Queries;
using System.Security.Claims;

namespace Siaed.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public sealed class ReportsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ReportsController(IMediator mediator) => _mediator = mediator;

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateReportCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value }, new { id = result.Value })
            : BadRequest(new { errors = result.Errors });
    }

    [HttpPost("generate")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Generate([FromBody] GenerateReportCommand command, CancellationToken ct)
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
        var result = await _mediator.Send(new GetReportByIdQuery(id, userId), ct);
        return result.IsSuccess ? Ok(result.Value) : NotFound(new { errors = result.Errors });
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] Guid userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] bool? isAIGenerated = null, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new ListReportsQuery(userId, page, pageSize, isAIGenerated), ct);
        return Ok(result.Value);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateReportCommand command, CancellationToken ct)
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
        var result = await _mediator.Send(new DeleteReportCommand(id, userId), ct);
        if (result.IsSuccess) return NoContent();
        if (result.Errors.Any(e => e.Contains("não encontrado"))) return NotFound(new { errors = result.Errors });
        return BadRequest(new { errors = result.Errors });
    }

    [HttpPost("{id:guid}/summarize")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Summarize(Guid id, CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _mediator.Send(new SummarizeReportCommand(id, userId), ct);
        if (result.IsSuccess) return Ok(result.Value);
        if (result.Errors.Any(e => e.Contains("não encontrado"))) return NotFound(new { errors = result.Errors });
        return BadRequest(new { errors = result.Errors });
    }

    [HttpPost("{id:guid}/parent-communication")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ParentCommunication(Guid id, CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _mediator.Send(new GenerateParentCommunicationCommand(id, userId), ct);
        if (result.IsSuccess) return Ok(result.Value);
        if (result.Errors.Any(e => e.Contains("não encontrado"))) return NotFound(new { errors = result.Errors });
        return BadRequest(new { errors = result.Errors });
    }
}
