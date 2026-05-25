using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Siaed.Application.Features.Students.Commands;
using Siaed.Application.Features.Students.Queries;
using Siaed.Domain.Enums;

namespace Siaed.Api.Controllers;

[ApiController]
[Route("api/v1/students")]
[Authorize]
public sealed class StudentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public StudentsController(IMediator mediator) => _mediator = mediator;

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterStudentCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value }, new { id = result.Value })
            : BadRequest(new { errors = result.Errors });
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] StudentStatus? status = null,
        [FromQuery] Guid? classId = null,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetStudentsPagedQuery(page, pageSize, search, status, classId), ct);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { errors = result.Errors });
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetStudentByIdQuery(id), ct);
        return result.IsSuccess ? Ok(result.Value) : NotFound(new { errors = result.Errors });
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateStudentCommand command, CancellationToken ct)
    {
        var request = command with { Id = id };
        var result = await _mediator.Send(request, ct);
        return result.IsSuccess ? NoContent() : NotFound(new { errors = result.Errors });
    }

    [HttpPatch("{id:guid}/transfer")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Transfer(Guid id, [FromBody] TransferStudentRequest body, CancellationToken ct)
    {
        var result = await _mediator.Send(new TransferStudentCommand(id, body.NewClassId), ct);
        return result.IsSuccess ? NoContent() : BadRequest(new { errors = result.Errors });
    }

    [HttpPatch("{id:guid}/deactivate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(Guid id, [FromBody] DeactivateStudentRequest body, CancellationToken ct)
    {
        var result = await _mediator.Send(new DeactivateStudentCommand(id, body.Status), ct);
        return result.IsSuccess ? NoContent() : BadRequest(new { errors = result.Errors });
    }

    [HttpPatch("{id:guid}/reactivate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Reactivate(Guid id, [FromBody] ReactivateStudentRequest body, CancellationToken ct)
    {
        var result = await _mediator.Send(new ReactivateStudentCommand(id, body.ClassId), ct);
        return result.IsSuccess ? NoContent() : NotFound(new { errors = result.Errors });
    }

    [HttpPost("import")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Import(IFormFile file, CancellationToken ct)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { errors = new[] { "Arquivo CSV não informado." } });

        using var ms = new MemoryStream();
        await file.CopyToAsync(ms, ct);

        var result = await _mediator.Send(new ImportStudentsFromCsvCommand(ms.ToArray()), ct);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { errors = result.Errors });
    }
}

public sealed record TransferStudentRequest(Guid NewClassId);
public sealed record DeactivateStudentRequest(StudentStatus Status);
public sealed record ReactivateStudentRequest(Guid ClassId);
