using Microsoft.EntityFrameworkCore;
using Siaed.Application.Common;
using Siaed.Application.Interfaces;
using Siaed.Domain.Entities;
using Siaed.Infra.Persistence;

namespace Siaed.Infra.Repositories;

public sealed class GradeRepository : IGradeRepository
{
    private readonly AppDbContext _context;

    public GradeRepository(AppDbContext context) => _context = context;

    public async Task<Grade?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _context.Set<Grade>().FirstOrDefaultAsync(g => g.Id == id, ct);

    public async Task<Grade?> GetActiveByActivityAndStudentAsync(Guid activityId, Guid studentId, CancellationToken ct = default)
        => await _context.Set<Grade>()
            .FirstOrDefaultAsync(g => g.ActivityId == activityId && g.StudentId == studentId, ct);

    public async Task<IReadOnlyList<Grade>> ListByActivityIdAsync(Guid activityId, CancellationToken ct = default)
        => await _context.Set<Grade>()
            .Where(g => g.ActivityId == activityId)
            .OrderBy(g => g.CreatedAt)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Grade>> ListByStudentIdAsync(Guid studentId, CancellationToken ct = default)
        => await _context.Set<Grade>()
            .Where(g => g.StudentId == studentId)
            .OrderByDescending(g => g.UpdatedAt)
            .ThenByDescending(g => g.CreatedAt)
            .ToListAsync(ct);

    public async Task<PagedResult<Grade>> ListAsync(
        int page,
        int pageSize,
        Guid? activityId,
        Guid? schoolClassId,
        Guid? teacherId,
        string? gradeValue,
        CancellationToken ct = default)
    {
        var query = _context.Set<Grade>().AsQueryable();

        if (activityId.HasValue)
            query = query.Where(g => g.ActivityId == activityId.Value);

        if (schoolClassId.HasValue)
            query = query.Where(g => g.SchoolClassId == schoolClassId.Value);

        if (teacherId.HasValue)
            query = query.Where(g => g.TeacherId == teacherId.Value);

        if (!string.IsNullOrWhiteSpace(gradeValue))
            query = query.Where(g => g.GradeValue == gradeValue);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(g => g.UpdatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<Grade>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task AddAsync(Grade grade, CancellationToken ct = default)
        => await _context.Set<Grade>().AddAsync(grade, ct);

    public void Update(Grade grade)
        => _context.Set<Grade>().Update(grade);
}
