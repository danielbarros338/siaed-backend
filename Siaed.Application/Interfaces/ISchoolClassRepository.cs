using Siaed.Application.Common;
using Siaed.Domain.Entities;

namespace Siaed.Application.Interfaces;

public interface ISchoolClassRepository
{
    Task<SchoolClass?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<SchoolClass?> GetByIdWithTeachersAsync(Guid id, CancellationToken ct = default);
    Task<PagedResult<SchoolClass>> ListAsync(int page, int pageSize, string? search, CancellationToken ct = default);
    Task AddAsync(SchoolClass schoolClass, CancellationToken ct = default);
    void Update(SchoolClass schoolClass);
}
