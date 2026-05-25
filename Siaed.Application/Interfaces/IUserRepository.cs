using Siaed.Application.Common;
using Siaed.Domain.Entities;
using Siaed.Domain.Enums;

namespace Siaed.Application.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default);
    Task<PagedResult<User>> ListByRoleAsync(UserRole role, int page, int pageSize, string? search, CancellationToken ct = default);
    Task AddAsync(User user, CancellationToken ct = default);
}
