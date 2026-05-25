using MediatR;
using Siaed.Application.Common;

namespace Siaed.Application.Features.AI.Queries;

public sealed record ListAIRequestsQuery(
    Guid RequestingUserId,
    int Page = 1,
    int PageSize = 10) : IRequest<Result<PagedResult<AIRequestDto>>>;

public sealed record AIRequestDto(
    Guid Id,
    string RequestType,
    string Status,
    string Model,
    int MaxTokens,
    int TokensUsed,
    decimal EstimatedCost,
    string? ErrorMessage,
    DateTime CreatedAt,
    DateTime UpdatedAt);
