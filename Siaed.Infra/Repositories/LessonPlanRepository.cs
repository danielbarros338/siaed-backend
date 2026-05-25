using Microsoft.EntityFrameworkCore;
using Siaed.Application.Interfaces;
using Siaed.Domain.Entities;
using Siaed.Infra.Persistence;

namespace Siaed.Infra.Repositories;

public sealed class LessonPlanRepository : ILessonPlanRepository
{
    private readonly AppDbContext _context;

    public LessonPlanRepository(AppDbContext context) => _context = context;

    public async Task<LessonPlan?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _context.LessonPlans.FirstOrDefaultAsync(l => l.Id == id, ct);

    public async Task<(IReadOnlyList<LessonPlan> Items, int TotalCount)> GetByTeacherIdAsync(
        Guid teacherId, int page, int pageSize, string? status = null, bool? isAIGenerated = null, CancellationToken ct = default)
    {
        var query = _context.LessonPlans.Where(l => l.TeacherId == teacherId)
                            .OrderByDescending(l => l.CreatedAt)
                            .AsQueryable();
        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<Siaed.Domain.Enums.LessonPlanStatus>(status, true, out var parsedStatus))
            query = query.Where(l => l.Status == parsedStatus);
        if (isAIGenerated.HasValue)
            query = query.Where(l => l.IsAIGenerated == isAIGenerated.Value);
        var total = await query.CountAsync(ct);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return (items, total);
    }

    public async Task AddAsync(LessonPlan lessonPlan, CancellationToken ct = default)
        => await _context.LessonPlans.AddAsync(lessonPlan, ct);

    public void Update(LessonPlan lessonPlan)
        => _context.LessonPlans.Update(lessonPlan);
}
