using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Siaed.Domain.Entities;

namespace Siaed.Infra.Persistence.Configurations;

public sealed class AIResponseConfiguration : IEntityTypeConfiguration<AIResponse>
{
    public void Configure(EntityTypeBuilder<AIResponse> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Content).HasColumnType("longtext");
        builder.Property(r => r.FinishReason).HasMaxLength(50);
        builder.Property(r => r.Model).HasMaxLength(100);
        builder.HasIndex(r => r.AIRequestId);
    }
}
