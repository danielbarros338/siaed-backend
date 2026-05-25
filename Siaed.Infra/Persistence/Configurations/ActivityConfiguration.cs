using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Siaed.Domain.Entities;

namespace Siaed.Infra.Persistence.Configurations;

public sealed class ActivityConfiguration : IEntityTypeConfiguration<Activity>
{
    public void Configure(EntityTypeBuilder<Activity> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Title).IsRequired().HasMaxLength(200);
        builder.Property(a => a.Description).HasMaxLength(1000);
        builder.Property(a => a.Subject).IsRequired().HasMaxLength(100);
        builder.Property(a => a.Grade).IsRequired().HasMaxLength(50);
        builder.Property(a => a.GradeConventionKey).HasMaxLength(100);
        builder.Property(a => a.AgeRange).HasMaxLength(50);
        builder.Property(a => a.Content).HasColumnType("longtext");
        builder.Property(a => a.AnswerKey).HasColumnType("longtext");
        builder.Property(a => a.SimplifiedVersion).HasColumnType("longtext");
        builder.HasIndex(a => a.SchoolClassId);
        builder.HasIndex(a => a.TeacherId);
        builder.HasQueryFilter(a => a.DeletedAt == null);
    }
}
