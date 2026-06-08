using Microsoft.EntityFrameworkCore;
using Siaed.Application.Common;
using Siaed.Application.Interfaces.Repositories;
using Siaed.Domain.Entities;
using Siaed.Domain.Enums;
using Siaed.Infra.Persistence;

namespace Siaed.Infra.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context) => _context = context;

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _context.Users.FirstOrDefaultAsync(u => u.Id == id, ct);

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
        => await _context.Users.FirstOrDefaultAsync(
            u => u.Email == email.ToLowerInvariant(), ct);

    public async Task<User?> GetByActivationTokenAsync(string activationToken, CancellationToken ct = default)
        => await _context.Users.FirstOrDefaultAsync(
            u => u.ActivationToken == activationToken, ct);

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default)
        => await _context.Users.AnyAsync(
            u => u.Email == email.ToLowerInvariant(), ct);

    public async Task<PagedResult<User>> ListByRoleAsync(
        UserRole role, int page, int pageSize, string? search, CancellationToken ct = default)
    {
        var query = _context.Users
            .Where(u => u.Role == role && u.IsActive);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(u => u.Name.Contains(search) || u.Email.Contains(search));

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderBy(u => u.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<User>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task AddAsync(User user, CancellationToken ct = default)
        => await _context.Users.AddAsync(user, ct);

    public async Task UpdateAsync(User user, CancellationToken ct = default)
    {
        _context.Users.Update(user);
        await Task.CompletedTask;
    }
}

