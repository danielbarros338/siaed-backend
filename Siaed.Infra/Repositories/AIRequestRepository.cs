using Microsoft.EntityFrameworkCore;
using Siaed.Application.Interfaces.Repositories;
using Siaed.Domain.Entities;
using Siaed.Infra.Persistence;

namespace Siaed.Infra.Repositories;

public sealed class AIRequestRepository : IAIRequestRepository
{
    private readonly AppDbContext _context;

    public AIRequestRepository(AppDbContext context) => _context = context;

    public async Task<AIRequest?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _context.AIRequests.FirstOrDefaultAsync(r => r.Id == id, ct);

    public async Task AddAsync(AIRequest aiRequest, CancellationToken ct = default)
        => await _context.AIRequests.AddAsync(aiRequest, ct);

    public void Update(AIRequest aiRequest)
        => _context.AIRequests.Update(aiRequest);

    public async Task AddResponseAsync(AIResponse aiResponse, CancellationToken ct = default)
        => await _context.AIResponses.AddAsync(aiResponse, ct);

    public async Task<(IReadOnlyList<AIRequest> Items, int TotalCount)> GetPagedByTeacherIdAsync(
        Guid teacherId, int page, int pageSize, CancellationToken ct = default)
    {
        var query = _context.AIRequests.Where(r => r.TeacherId == teacherId)
                            .OrderByDescending(r => r.CreatedAt);
        var total = await query.CountAsync(ct);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return (items, total);
    }
}
