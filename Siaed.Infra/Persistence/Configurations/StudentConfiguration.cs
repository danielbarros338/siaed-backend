using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Siaed.Domain.Entities;

namespace Siaed.Infra.Persistence.Configurations;

public sealed class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.FullName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(s => s.DocumentType)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(s => s.DocumentId)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(s => s.BirthDate)
            .IsRequired();

        builder.Property(s => s.ClassId)
            .IsRequired();

        builder.Property(s => s.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(s => s.EnrollmentDate)
            .IsRequired();

        builder.Property(s => s.Notes)
            .HasMaxLength(2000);

        builder.HasIndex(s => s.DocumentId)
            .IsUnique()
            .HasDatabaseName("UX_Students_DocumentId");

        builder.HasOne<SchoolClass>()
            .WithMany()
            .HasForeignKey(s => s.ClassId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(s => s.DeletedAt == null);
    }
}
