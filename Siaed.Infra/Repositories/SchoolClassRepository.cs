using Microsoft.EntityFrameworkCore;
using Siaed.Application.Common;
using Siaed.Application.Interfaces;
using Siaed.Domain.Entities;
using Siaed.Infra.Persistence;

namespace Siaed.Infra.Repositories;

public sealed class SchoolClassRepository : ISchoolClassRepository
{
    private readonly AppDbContext _context;

    public SchoolClassRepository(AppDbContext context) => _context = context;

    public async Task<SchoolClass?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _context.SchoolClasses.FirstOrDefaultAsync(t => t.Id == id, ct);

    public async Task<SchoolClass?> GetByIdWithTeachersAsync(Guid id, CancellationToken ct = default)
        => await _context.SchoolClasses
            .Include(t => t.Teachers)
            .FirstOrDefaultAsync(t => t.Id == id, ct);

    public async Task<PagedResult<SchoolClass>> ListAsync(int page, int pageSize, string? search, CancellationToken ct = default)
    {
        var query = _context.SchoolClasses.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(t => t.Name.Contains(search) || t.Grade.Contains(search));

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderBy(t => t.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<SchoolClass>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task AddAsync(SchoolClass schoolClass, CancellationToken ct = default)
        => await _context.SchoolClasses.AddAsync(schoolClass, ct);

    public void Update(SchoolClass schoolClass)
        => _context.SchoolClasses.Update(schoolClass);
}

