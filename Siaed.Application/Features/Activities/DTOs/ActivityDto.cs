using Siaed.Domain.Enums;

namespace Siaed.Application.Features.Activities.DTOs;

public sealed record ActivityDto(
    Guid Id,
    Guid TeacherId,
    Guid? LessonPlanId,
    string Title,
    string Description,
    string Subject,
    string Grade,
    string AgeRange,
    string Content,
    string AnswerKey,
    string SimplifiedVersion,
    ActivityType Type,
    bool IsAIGenerated,
    ActivityStatus Status,
    DateTime CreatedAt,
    DateTime UpdatedAt);
