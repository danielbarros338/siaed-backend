using MediatR;
using Siaed.Application.Common;
using Siaed.Domain.Enums;

namespace Siaed.Application.Features.Activities.Commands;

public sealed record GenerateActivityCommand(
    Guid TeacherId,
    string Subject,
    string Grade,
    string AgeRange,
    ActivityType Type,
    int NumberOfQuestions = 10,
    Guid? LessonPlanId = null,
    string AdditionalInstructions = "") : IRequest<Result<Guid>>;
