using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Siaed.Application.Features.AI.Queries;
using System.Security.Claims;

namespace Siaed.Api.Controllers;

[ApiController]
[Route("api/v1/ai")]
[Authorize]
public sealed class AIController : ControllerBase
{
    private readonly IMediator _mediator;

    public AIController(IMediator mediator) => _mediator = mediator;

    [HttpGet("requests")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ListRequests([FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken ct = default)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _mediator.Send(new ListAIRequestsQuery(userId, page, pageSize), ct);
        if (result.IsSuccess) return Ok(result.Value);
        return NotFound(new { errors = result.Errors });
    }
}
