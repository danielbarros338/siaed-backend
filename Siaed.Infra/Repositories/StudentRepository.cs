using Microsoft.EntityFrameworkCore;
using Siaed.Application.Common;
using Siaed.Application.Interfaces.Repositories;
using Siaed.Domain.Entities;
using Siaed.Domain.Enums;
using Siaed.Infra.Persistence;

namespace Siaed.Infra.Repositories;

public sealed class StudentRepository : IStudentRepository
{
    private readonly AppDbContext _context;

    public StudentRepository(AppDbContext context) => _context = context;

    public async Task<Student?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _context.Students.FirstOrDefaultAsync(s => s.Id == id, ct);

    public async Task<IReadOnlyList<Student>> ListByClassIdAsync(Guid classId, CancellationToken ct = default)
        => await _context.Students
            .Where(s => s.ClassId == classId)
            .OrderBy(s => s.FullName)
            .ToListAsync(ct);

    public async Task<Student?> GetByDocumentAsync(string documentId, CancellationToken ct = default)
        => await _context.Students.FirstOrDefaultAsync(s => s.DocumentId == documentId, ct);

    public async Task<bool> ExistsByDocumentAsync(string documentId, CancellationToken ct = default)
        => await _context.Students.AnyAsync(s => s.DocumentId == documentId, ct);

    public async Task<PagedResult<Student>> ListAsync(
        int page,
        int pageSize,
        string? search,
        StudentStatus? status,
        Guid? classId,
        CancellationToken ct = default)
    {
        var query = _context.Students.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(s => s.FullName.Contains(search) || s.DocumentId.Contains(search));

        if (status.HasValue)
            query = query.Where(s => s.Status == status.Value);

        if (classId.HasValue)
            query = query.Where(s => s.ClassId == classId.Value);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderBy(s => s.FullName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<Student>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task AddAsync(Student student, CancellationToken ct = default)
        => await _context.Students.AddAsync(student, ct);

    public void Update(Student student)
        => _context.Students.Update(student);
}

