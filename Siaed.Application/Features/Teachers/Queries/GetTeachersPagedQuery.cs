using MediatR;
using Siaed.Application.Common;
using Siaed.Application.Features.Teachers.DTOs;

namespace Siaed.Application.Features.Teachers.Queries;

public sealed record GetTeachersPagedQuery(
    int Page,
    int PageSize,
    string? Search
) : IRequest<Result<PagedResult<TeacherDto>>>;
