using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Siaed.Domain.Entities;

namespace Siaed.Infra.Persistence.Configurations;

public sealed class AIRequestConfiguration : IEntityTypeConfiguration<AIRequest>
{
    public void Configure(EntityTypeBuilder<AIRequest> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Prompt).HasColumnType("longtext");
        builder.Property(r => r.InputData).HasColumnType("longtext");
        builder.Property(r => r.Model).HasMaxLength(100);
        builder.Property(r => r.ErrorMessage).HasMaxLength(1000);
        builder.Property(r => r.EstimatedCost).HasColumnType("decimal(10,6)");
        builder.HasIndex(r => r.TeacherId);
        builder.HasMany<AIResponse>()
               .WithOne()
               .HasForeignKey(r => r.AIRequestId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
