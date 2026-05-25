using MediatR;
using Siaed.Application.Common;
using Siaed.Application.Features.SchoolClasses.DTOs;

namespace Siaed.Application.Features.SchoolClasses.Queries;

public sealed record GetSchoolClassByIdQuery(
    Guid Id
) : IRequest<Result<SchoolClassDto>>;

public sealed record GetSchoolClassesPagedQuery(
    int Page,
    int PageSize,
    string? Search
) : IRequest<Result<PagedResult<SchoolClassSummaryDto>>>;
