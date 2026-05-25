using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Siaed.Domain.Entities;

namespace Siaed.Infra.Persistence.Configurations;

public sealed class PedagogicalReportConfiguration : IEntityTypeConfiguration<PedagogicalReport>
{
    public void Configure(EntityTypeBuilder<PedagogicalReport> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.UserId).IsRequired();
        builder.Property(r => r.StudentId).IsRequired();
        builder.Property(r => r.Content).HasColumnType("longtext");
        builder.Property(r => r.Summary).HasColumnType("longtext");
        builder.Property(r => r.ParentCommunication).HasColumnType("longtext");

        builder.HasIndex(r => r.UserId);
        builder.HasIndex(r => r.StudentId);

        builder.HasOne(r => r.Student)
            .WithMany()
            .HasForeignKey(r => r.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.User)
            .WithMany()
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(r => r.DeletedAt == null);
    }
}
