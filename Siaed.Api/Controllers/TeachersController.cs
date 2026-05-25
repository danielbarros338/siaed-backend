using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Siaed.Application.Features.Teachers.Queries;
using System.Security.Claims;

namespace Siaed.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public sealed class TeachersController : ControllerBase
{
    private readonly IMediator _mediator;

    public TeachersController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? search = null, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetTeachersPagedQuery(page, pageSize, search), ct);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { errors = result.Errors });
    }

    [HttpGet("me")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMyProfile(CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _mediator.Send(new GetMyTeacherProfileQuery(userId), ct);
        return result.IsSuccess ? Ok(result.Value) : NotFound(new { errors = result.Errors });
    }
}
