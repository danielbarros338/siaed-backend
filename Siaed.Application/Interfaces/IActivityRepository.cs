using Siaed.Domain.Entities;

namespace Siaed.Application.Interfaces;

public interface IActivityRepository
{
    Task<Activity?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Activity>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default);
    Task<(IReadOnlyList<Activity> Items, int TotalCount)> GetByTeacherIdAsync(Guid teacherId, int page, int pageSize, string? status = null, bool? isAIGenerated = null, CancellationToken ct = default);
    Task AddAsync(Activity activity, CancellationToken ct = default);
    void Update(Activity activity);
}
