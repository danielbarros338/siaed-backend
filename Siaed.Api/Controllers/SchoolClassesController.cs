using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Siaed.Application.Features.SchoolClasses.Commands;
using Siaed.Application.Features.SchoolClasses.Queries;

namespace Siaed.Api.Controllers;

[ApiController]
[Route("api/v1/classes")]
[Authorize]
public sealed class SchoolClassesController : ControllerBase
{
    private readonly IMediator _mediator;

    public SchoolClassesController(IMediator mediator) => _mediator = mediator;

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateSchoolClassCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value }, new { id = result.Value })
            : BadRequest(new { errors = result.Errors });
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? search = null, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetSchoolClassesPagedQuery(page, pageSize, search), ct);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { errors = result.Errors });
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetSchoolClassByIdQuery(id), ct);
        return result.IsSuccess ? Ok(result.Value) : NotFound(new { errors = result.Errors });
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSchoolClassCommand command, CancellationToken ct)
    {
        var request = command with { Id = id };
        var result = await _mediator.Send(request, ct);
        return result.IsSuccess ? NoContent() : NotFound(new { errors = result.Errors });
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new DeactivateSchoolClassCommand(id), ct);
        return result.IsSuccess ? NoContent() : NotFound(new { errors = result.Errors });
    }

    [HttpPatch("{id:guid}/reactivate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Reactivate(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new ReactivateSchoolClassCommand(id), ct);
        return result.IsSuccess ? NoContent() : NotFound(new { errors = result.Errors });
    }
}
