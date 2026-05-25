using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Siaed.Domain.Entities;

namespace Siaed.Infra.Persistence.Configurations;

public sealed class SchoolClassConfiguration : IEntityTypeConfiguration<SchoolClass>
{
    public void Configure(EntityTypeBuilder<SchoolClass> builder)
    {
        builder.ToTable("Classes");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.Grade)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(t => t.SchoolYear)
            .IsRequired();

        builder.HasMany(t => t.Teachers)
            .WithMany()
            .UsingEntity("ClassTeachers",
                l => l.HasOne(typeof(User)).WithMany().HasForeignKey("UserId").OnDelete(DeleteBehavior.Cascade),
                r => r.HasOne(typeof(SchoolClass)).WithMany().HasForeignKey("ClassId").OnDelete(DeleteBehavior.Cascade));

        builder.HasQueryFilter(t => t.DeletedAt == null);
    }
}
