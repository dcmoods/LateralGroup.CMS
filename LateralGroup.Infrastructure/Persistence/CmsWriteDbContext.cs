using LateralGroup.Application.Common.Abstractions;
using LateralGroup.Domain.Entities;
using LateralGroup.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;

namespace LateralGroup.Infrastructure.Persistence;
public sealed class CmsWriteDbContext(DbContextOptions<CmsWriteDbContext> options)
    : DbContext(options), ICmsWriteDbContext
{
    public DbSet<CmsContentItem> ContentItems => Set<CmsContentItem>();
    public DbSet<CmsContentVersion> ContentVersions => Set<CmsContentVersion>();
    public DbSet<ProcessedCmsEvent> ProcessedCmsEvents => Set<ProcessedCmsEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CmsContentItemConfiguration).Assembly);
    }
}
