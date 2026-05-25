using MediatR;
using Siaed.Application.Common;

namespace Siaed.Application.Features.LessonPlans.Commands;

public sealed record CreateLessonPlanCommand(
    Guid TeacherId,
    string Title,
    string Subject,
    string Grade,
    int DurationMinutes,
    string Objectives,
    string Content,
    string Methodology,
    string Resources,
    string Evaluation,
    string AgeRange) : IRequest<Result<Guid>>;
