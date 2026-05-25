using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Siaed.Domain.Entities;

namespace Siaed.Infra.Persistence.Configurations;

public sealed class GradeConfiguration : IEntityTypeConfiguration<Grade>
{
    public void Configure(EntityTypeBuilder<Grade> builder)
    {
        builder.HasKey(g => g.Id);

        builder.Property(g => g.ActivityId).IsRequired();
        builder.Property(g => g.StudentId).IsRequired();
        builder.Property(g => g.SchoolClassId).IsRequired();
        builder.Property(g => g.TeacherId).IsRequired();

        builder.Property(g => g.GradeValue)
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(g => g.ConventionKey)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(g => g.Version)
            .IsRowVersion();

        builder.HasIndex(g => new { g.ActivityId, g.StudentId, g.DeletedAt })
            .IsUnique()
            .HasDatabaseName("UX_Grades_Activity_Student_Active");

        builder.HasIndex(g => g.SchoolClassId).HasDatabaseName("IX_Grades_SchoolClassId");
        builder.HasIndex(g => g.TeacherId).HasDatabaseName("IX_Grades_TeacherId");
        builder.HasIndex(g => g.GradeValue).HasDatabaseName("IX_Grades_GradeValue");
        builder.HasIndex(g => g.ActivityId).HasDatabaseName("IX_Grades_ActivityId");

        builder.HasOne<Activity>()
            .WithMany()
            .HasForeignKey(g => g.ActivityId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Student>()
            .WithMany()
            .HasForeignKey(g => g.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<SchoolClass>()
            .WithMany()
            .HasForeignKey(g => g.SchoolClassId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(g => g.TeacherId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(g => g.DeletedAt == null);
    }
}
