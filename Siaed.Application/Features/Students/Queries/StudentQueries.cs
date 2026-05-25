using MediatR;
using Siaed.Application.Common;
using Siaed.Application.Features.Students.DTOs;
using Siaed.Domain.Enums;

namespace Siaed.Application.Features.Students.Queries;

public sealed record GetStudentByIdQuery(
    Guid Id
) : IRequest<Result<StudentDto>>;

public sealed record GetStudentsPagedQuery(
    int Page,
    int PageSize,
    string? Search,
    StudentStatus? Status,
    Guid? ClassId
) : IRequest<Result<PagedResult<StudentSummaryDto>>>;
