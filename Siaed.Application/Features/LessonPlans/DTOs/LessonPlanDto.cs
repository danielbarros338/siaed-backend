using Siaed.Domain.Enums;

namespace Siaed.Application.Features.LessonPlans.DTOs;

public sealed record LessonPlanDto(
    Guid Id,
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
    string AgeRange,
    bool IsAIGenerated,
    LessonPlanStatus Status,
    DateTime CreatedAt,
    DateTime UpdatedAt);
