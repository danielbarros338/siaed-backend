using MediatR;
using Siaed.Application.Common;
using Siaed.Domain.Enums;

namespace Siaed.Application.Features.Activities.Commands;

public sealed record CreateActivityCommand(
    Guid TeacherId,
    string Title,
    string Description,
    string Subject,
    string Grade,
    Guid? SchoolClassId,
    string? GradeConventionKey,
    string AgeRange,
    string Content,
    ActivityType Type,
    Guid? LessonPlanId = null) : IRequest<Result<Guid>>;
