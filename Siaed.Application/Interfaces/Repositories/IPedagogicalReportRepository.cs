using Siaed.Domain.Entities;

namespace Siaed.Application.Interfaces.Repositories;

public interface IPedagogicalReportRepository
{
    Task<PedagogicalReport?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<(IReadOnlyList<PedagogicalReport> Items, int TotalCount)> GetByUserIdAsync(Guid userId, int page, int pageSize, bool? isAIGenerated = null, CancellationToken ct = default);
    Task AddAsync(PedagogicalReport report, CancellationToken ct = default);
    void Update(PedagogicalReport report);
}
