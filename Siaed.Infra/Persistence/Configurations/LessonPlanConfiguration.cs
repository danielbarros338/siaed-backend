using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Siaed.Domain.Entities;

namespace Siaed.Infra.Persistence.Configurations;

public sealed class LessonPlanConfiguration : IEntityTypeConfiguration<LessonPlan>
{
    public void Configure(EntityTypeBuilder<LessonPlan> builder)
    {
        builder.HasKey(l => l.Id);
        builder.Property(l => l.Title).IsRequired().HasMaxLength(200);
        builder.Property(l => l.Subject).IsRequired().HasMaxLength(100);
        builder.Property(l => l.Grade).IsRequired().HasMaxLength(50);
        builder.Property(l => l.Objectives).HasMaxLength(2000);
        builder.Property(l => l.Content).HasColumnType("longtext");
        builder.Property(l => l.Methodology).HasMaxLength(2000);
        builder.Property(l => l.Resources).HasMaxLength(2000);
        builder.Property(l => l.Evaluation).HasMaxLength(2000);
        builder.Property(l => l.AgeRange).HasMaxLength(50);
        builder.HasIndex(l => l.TeacherId);
        builder.HasQueryFilter(l => l.DeletedAt == null);
    }
}
