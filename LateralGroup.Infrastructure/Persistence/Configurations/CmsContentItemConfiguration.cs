using LateralGroup.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace LateralGroup.Infrastructure.Persistence.Configurations;

public partial class CmsContentItemConfiguration : IEntityTypeConfiguration<CmsContentItem>
{
    public void Configure(EntityTypeBuilder<CmsContentItem> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.LatestKnownVersion);

        builder.Property(x => x.LatestPublishedVersion);

        builder.Property(x => x.LatestPayloadJson)
            .HasColumnType("TEXT");

        builder.Property(x => x.IsPublished)
            .IsRequired();

        builder.Property(x => x.IsDisabledByCms)
            .IsRequired();

        builder.Property(x => x.IsDisabledByAdmin)
            .IsRequired();

        builder.Property(x => x.LastEventTimestampUtc)
            .IsRequired();

        builder.Property(x => x.LastEventType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.CreatedUtc)
            .IsRequired();

        builder.Property(x => x.UpdatedUtc)
            .IsRequired();

        builder.HasMany(x => x.Versions)
            .WithOne(x => x.ContentItem)
            .HasForeignKey(x => x.ContentItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.IsPublished);

        builder.HasIndex(x => x.IsDisabledByCms);

        builder.HasIndex(x => x.IsDisabledByAdmin);

        builder.HasIndex(x => x.LastEventTimestampUtc);

        builder.HasIndex(x => new
        {
            x.IsPublished,
            x.IsDisabledByCms,
            x.IsDisabledByAdmin
        });
    }
}
