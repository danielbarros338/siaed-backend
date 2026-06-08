using Siaed.Domain.Entities;

namespace Siaed.Application.Interfaces.Repositories;

public interface ILessonPlanRepository
{
    Task<LessonPlan?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<(IReadOnlyList<LessonPlan> Items, int TotalCount)> GetByTeacherIdAsync(Guid teacherId, int page, int pageSize, string? status = null, bool? isAIGenerated = null, CancellationToken ct = default);
    Task AddAsync(LessonPlan lessonPlan, CancellationToken ct = default);
    void Update(LessonPlan lessonPlan);
}
