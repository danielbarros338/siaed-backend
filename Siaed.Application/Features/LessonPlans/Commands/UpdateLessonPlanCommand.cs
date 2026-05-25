using MediatR;
using Siaed.Application.Common;

namespace Siaed.Application.Features.LessonPlans.Commands;

public sealed record UpdateLessonPlanCommand(
    Guid Id,
    Guid RequestingUserId,
    string Title,
    string Objectives,
    string Content,
    string Methodology,
    string Resources,
    string Evaluation) : IRequest<Result>;
