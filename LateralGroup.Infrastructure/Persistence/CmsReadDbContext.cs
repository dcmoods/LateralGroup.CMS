using LateralGroup.Application.Common.Abstractions;
using LateralGroup.Domain.Entities;
using LateralGroup.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;

namespace LateralGroup.Infrastructure.Persistence
{
    public sealed class CmsReadDbContext(DbContextOptions options) : DbContext(options), ICmsReadDbContext
    {
        public DbSet<CmsContentItem> CmsContentItems => Set<CmsContentItem>();
        public DbSet<CmsContentVersion> CmsContentVersions => Set<CmsContentVersion>();
        public DbSet<ProcessedCmsEvent> CmsProcessedCmsEvents => Set<ProcessedCmsEvent>();

        IQueryable<CmsContentItem> ICmsReadDbContext.ContentItems => CmsContentItems;
        IQueryable<CmsContentVersion> ICmsReadDbContext.ContentVersions => CmsContentVersions;
        IQueryable<ProcessedCmsEvent> ICmsReadDbContext.ProcessedCmsEvents => CmsProcessedCmsEvents;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(CmsContentItemConfiguration).Assembly);
        }
    }
}
