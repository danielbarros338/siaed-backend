using Siaed.Application.Common;
using Siaed.Domain.Entities;

namespace Siaed.Application.Interfaces.Repositories;

public interface IGradeRepository
{
    Task<Grade?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Grade?> GetActiveByActivityAndStudentAsync(Guid activityId, Guid studentId, CancellationToken ct = default);
    Task<IReadOnlyList<Grade>> ListByActivityIdAsync(Guid activityId, CancellationToken ct = default);
    Task<IReadOnlyList<Grade>> ListByStudentIdAsync(Guid studentId, CancellationToken ct = default);
    Task<PagedResult<Grade>> ListAsync(
        int page,
        int pageSize,
        Guid? activityId,
        Guid? schoolClassId,
        Guid? teacherId,
        string? gradeValue,
        CancellationToken ct = default);
    Task AddAsync(Grade grade, CancellationToken ct = default);
    void Update(Grade grade);
}
