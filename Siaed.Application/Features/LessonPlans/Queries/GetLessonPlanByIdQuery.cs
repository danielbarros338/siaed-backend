using MediatR;
using Siaed.Application.Common;
using Siaed.Application.Features.LessonPlans.DTOs;

namespace Siaed.Application.Features.LessonPlans.Queries;

public sealed record GetLessonPlanByIdQuery(Guid Id, Guid RequestingUserId) : IRequest<Result<LessonPlanDto>>;
