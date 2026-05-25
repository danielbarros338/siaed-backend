using Siaed.Application.Interfaces;

namespace Siaed.Infra.Persistence;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    public UnitOfWork(AppDbContext context) => _context = context;

    public Task<int> CommitAsync(CancellationToken ct = default)
        => _context.SaveChangesAsync(ct);
}
