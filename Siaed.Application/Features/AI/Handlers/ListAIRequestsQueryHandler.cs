using MediatR;
using Siaed.Application.Common;
using Siaed.Application.Features.AI.Queries;
using Siaed.Application.Interfaces.Repositories;

namespace Siaed.Application.Features.AI.Handlers;

public sealed class ListAIRequestsQueryHandler
    : IRequestHandler<ListAIRequestsQuery, Result<PagedResult<AIRequestDto>>>
{
    private readonly IAIRequestRepository _aiRequestRepo;

    public ListAIRequestsQueryHandler(IAIRequestRepository aiRequestRepo)
        => _aiRequestRepo = aiRequestRepo;

    public async Task<Result<PagedResult<AIRequestDto>>> Handle(ListAIRequestsQuery request, CancellationToken ct)
    {
        var (items, totalCount) = await _aiRequestRepo.GetPagedByTeacherIdAsync(request.RequestingUserId, request.Page, request.PageSize, ct);

        var dtos = items.Select(r => new AIRequestDto(
            r.Id,
            r.RequestType.ToString(),
            r.Status.ToString(),
            r.Model,
            r.MaxTokens,
            r.TokensUsed,
            r.EstimatedCost,
            r.ErrorMessage,
            r.CreatedAt,
            r.UpdatedAt)).ToList();

        return Result.Success(new PagedResult<AIRequestDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        });
    }
}
