using Siaed.Application.Common;
using Siaed.Domain.Entities;
using Siaed.Domain.Enums;

namespace Siaed.Application.Interfaces.Repositories;

public interface IStudentRepository
{
    Task<Student?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Student>> ListByClassIdAsync(Guid classId, CancellationToken ct = default);
    Task<Student?> GetByDocumentAsync(string documentId, CancellationToken ct = default);
    Task<bool> ExistsByDocumentAsync(string documentId, CancellationToken ct = default);
    Task<PagedResult<Student>> ListAsync(int page, int pageSize, string? search, StudentStatus? status, Guid? classId, CancellationToken ct = default);
    Task AddAsync(Student student, CancellationToken ct = default);
    void Update(Student student);
}
