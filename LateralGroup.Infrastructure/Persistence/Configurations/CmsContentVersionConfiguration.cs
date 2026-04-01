using LateralGroup.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Security.Cryptography.X509Certificates;

namespace LateralGroup.Infrastructure.Persistence.Configurations;


public class CmsContentVersionConfiguration : IEntityTypeConfiguration<CmsContentVersion>
{
    public void Configure(EntityTypeBuilder<CmsContentVersion> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.ContentItemId)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Version)
            .IsRequired();

        builder.Property(x => x.PayloadJson)
            .IsRequired()
            .HasColumnType("TEXT");

        builder.Property(x => x.WasPublished)
            .IsRequired();

        builder.Property(x => x.WasUnpublished)
            .IsRequired();

        builder.Property(x => x.ObservedAtUtc)
            .IsRequired();

        builder.HasIndex(x => x.ContentItemId);

        builder.HasIndex(x => new { x.ContentItemId, x.Version })
            .IsUnique();

        builder.HasIndex(x => x.ObservedAtUtc);
    }
}
