using MediatR;
using Siaed.Application.Common;
using Siaed.Application.Features.Teachers.DTOs;

namespace Siaed.Application.Features.Teachers.Queries;

public sealed record GetMyTeacherProfileQuery(Guid UserId) : IRequest<Result<TeacherDto>>;
