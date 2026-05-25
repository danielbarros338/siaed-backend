using MediatR;
using Siaed.Application.Common;

namespace Siaed.Application.Features.LessonPlans.Commands;

public sealed record GenerateLessonPlanCommand(
    Guid TeacherId,
    string Subject,
    string Grade,
    string AgeRange,
    int DurationMinutes,
    string AdditionalInstructions = "") : IRequest<Result<Guid>>;
