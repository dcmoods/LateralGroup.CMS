using LateralGroup.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LateralGroup.Infrastructure.Persistence.Configurations;

public class ProcessedCmsEventConfiguration : IEntityTypeConfiguration<ProcessedCmsEvent>
{
    public void Configure(EntityTypeBuilder<ProcessedCmsEvent> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.ContentItemId)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.EventType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Version);

        builder.Property(x => x.TimestampUtc)
            .IsRequired();

        builder.Property(x => x.RawEventJson)
            .IsRequired()
            .HasColumnType("TEXT");

        builder.Property(x => x.Status)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.FailureReason)
            .HasMaxLength(2000);

        builder.Property(x => x.CreatedUtc)
            .IsRequired();

        builder.HasIndex(x => x.ContentItemId);

        builder.HasIndex(x => x.Status);

        builder.HasIndex(x => x.TimestampUtc);

        builder.HasIndex(x => new { x.ContentItemId, x.TimestampUtc });
    }
}
