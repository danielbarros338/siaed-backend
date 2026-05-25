using Microsoft.EntityFrameworkCore;
using Siaed.Domain.Entities;

namespace Siaed.Infra.Persistence;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<LessonPlan> LessonPlans => Set<LessonPlan>();
    public DbSet<Activity> Activities => Set<Activity>();
    public DbSet<PedagogicalReport> PedagogicalReports => Set<PedagogicalReport>();
    public DbSet<AIRequest> AIRequests => Set<AIRequest>();
    public DbSet<AIResponse> AIResponses => Set<AIResponse>();
    public DbSet<User> Users => Set<User>();
    public DbSet<SchoolClass> SchoolClasses => Set<SchoolClass>();
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Grade> Grades => Set<Grade>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Modified)
            {
                entry.Property(nameof(BaseEntity.UpdatedAt)).CurrentValue = DateTime.UtcNow;
            }
        }
        return base.SaveChangesAsync(cancellationToken);
    }
}
