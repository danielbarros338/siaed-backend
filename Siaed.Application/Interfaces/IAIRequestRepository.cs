using Siaed.Domain.Entities;

namespace Siaed.Application.Interfaces;

public interface IAIRequestRepository
{
    Task<AIRequest?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(AIRequest aiRequest, CancellationToken ct = default);
    void Update(AIRequest aiRequest);
    Task AddResponseAsync(AIResponse aiResponse, CancellationToken ct = default);
    Task<(IReadOnlyList<AIRequest> Items, int TotalCount)> GetPagedByTeacherIdAsync(Guid teacherId, int page, int pageSize, CancellationToken ct = default);
}
