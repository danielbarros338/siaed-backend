using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Siaed.Application.Features.Grades.Commands;
using Siaed.Application.Features.Grades.Queries;
using System.Security.Claims;

namespace Siaed.Api.Controllers;

[ApiController]
[Route("api/v1/grades")]
[Authorize]
public sealed class GradesController : ControllerBase
{
    private readonly IMediator _mediator;

    public GradesController(IMediator mediator) => _mediator = mediator;

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreateGradeRequest request, CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var result = await _mediator.Send(new CreateGradeCommand(
            request.ActivityId,
            request.StudentId,
            request.SchoolClassId,
            request.TeacherId,
            request.GradeValue,
            request.ConventionKey,
            userId), ct);

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
        var result = await _mediator.Send(new GetGradeByIdQuery(id, userId), ct);
        return result.IsSuccess ? Ok(result.Value) : NotFound(new { errors = result.Errors });
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] Guid? activityId = null,
        [FromQuery] Guid? schoolClassId = null,
        [FromQuery] Guid? teacherId = null,
        [FromQuery] string? gradeValue = null,
        CancellationToken ct = default)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var result = await _mediator.Send(new ListGradesQuery(
            page,
            pageSize,
            activityId,
            schoolClassId,
            teacherId,
            gradeValue,
            userId), ct);

        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { errors = result.Errors });
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateGradeRequest request, CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var result = await _mediator.Send(new UpdateGradeCommand(
            id,
            request.GradeValue,
            request.ConventionKey,
            request.Version,
            userId), ct);

        if (result.IsSuccess) return Ok(result.Value);

        if (result.Errors.Any(e => e.Contains("concorrencia", StringComparison.OrdinalIgnoreCase)))
            return Conflict(new { errors = result.Errors });

        return BadRequest(new { errors = result.Errors });
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _mediator.Send(new DeleteGradeCommand(id, userId), ct);
        return result.IsSuccess ? NoContent() : NotFound(new { errors = result.Errors });
    }
}

public sealed record CreateGradeRequest(
    Guid ActivityId,
    Guid StudentId,
    Guid SchoolClassId,
    Guid TeacherId,
    string GradeValue,
    string ConventionKey);

public sealed record UpdateGradeRequest(
    string GradeValue,
    string ConventionKey,
    string Version);
