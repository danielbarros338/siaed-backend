using MediatR;
using Siaed.Application.Common;
using Siaed.Application.Features.Reports.DTOs;

namespace Siaed.Application.Features.Reports.Queries;

public sealed record ListReportsQuery(
    Guid UserId,
    int Page = 1,
    int PageSize = 10,
    bool? IsAIGenerated = null) : IRequest<Result<PagedResult<PedagogicalReportDto>>>;
