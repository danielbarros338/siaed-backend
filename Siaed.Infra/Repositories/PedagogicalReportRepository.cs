using Microsoft.EntityFrameworkCore;
using Siaed.Application.Interfaces;
using Siaed.Domain.Entities;
using Siaed.Infra.Persistence;

namespace Siaed.Infra.Repositories;

public sealed class PedagogicalReportRepository : IPedagogicalReportRepository
{
    private readonly AppDbContext _context;

    public PedagogicalReportRepository(AppDbContext context) => _context = context;

    public async Task<PedagogicalReport?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _context.PedagogicalReports
            .Include(r => r.User)
            .Include(r => r.Student)
            .FirstOrDefaultAsync(r => r.Id == id, ct);

    public async Task<(IReadOnlyList<PedagogicalReport> Items, int TotalCount)> GetByUserIdAsync(
        Guid userId, int page, int pageSize, bool? isAIGenerated = null, CancellationToken ct = default)
    {
        var query = _context.PedagogicalReports
                            .Include(r => r.User)
                            .Include(r => r.Student)
                            .Where(r => r.UserId == userId)
                            .OrderByDescending(r => r.CreatedAt)
                            .AsQueryable();
        if (isAIGenerated.HasValue)
            query = query.Where(r => r.IsAIGenerated == isAIGenerated.Value);
        var total = await query.CountAsync(ct);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return (items, total);
    }

    public async Task AddAsync(PedagogicalReport report, CancellationToken ct = default)
        => await _context.PedagogicalReports.AddAsync(report, ct);

    public void Update(PedagogicalReport report)
        => _context.PedagogicalReports.Update(report);
}
