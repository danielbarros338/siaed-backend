using Microsoft.EntityFrameworkCore;
using Siaed.Application.Interfaces.Repositories;
using Siaed.Domain.Entities;
using Siaed.Infra.Persistence;

namespace Siaed.Infra.Repositories;

public sealed class ActivityRepository : IActivityRepository
{
    private readonly AppDbContext _context;

    public ActivityRepository(AppDbContext context) => _context = context;

    public async Task<Activity?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _context.Activities.FirstOrDefaultAsync(a => a.Id == id, ct);

    public async Task<IReadOnlyList<Activity>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default)
    {
        var activityIds = ids.Distinct().ToList();

        if (activityIds.Count == 0)
            return Array.Empty<Activity>();

        return await _context.Activities
            .Where(a => activityIds.Contains(a.Id))
            .ToListAsync(ct);
    }

    public async Task<(IReadOnlyList<Activity> Items, int TotalCount)> GetByTeacherIdAsync(
        Guid teacherId, int page, int pageSize, string? status = null, bool? isAIGenerated = null, CancellationToken ct = default)
    {
        var query = _context.Activities.Where(a => a.TeacherId == teacherId)
                            .OrderByDescending(a => a.CreatedAt)
                            .AsQueryable();
        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<Siaed.Domain.Enums.ActivityStatus>(status, true, out var parsedStatus))
            query = query.Where(a => a.Status == parsedStatus);
        if (isAIGenerated.HasValue)
            query = query.Where(a => a.IsAIGenerated == isAIGenerated.Value);
        var total = await query.CountAsync(ct);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return (items, total);
    }

    public async Task AddAsync(Activity activity, CancellationToken ct = default)
        => await _context.Activities.AddAsync(activity, ct);

    public void Update(Activity activity)
        => _context.Activities.Update(activity);
}
