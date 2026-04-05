using LateralGroup.Application.Abstractions.Persistence;
using LateralGroup.Domain.Entities;
using LateralGroup.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;

namespace LateralGroup.Infrastructure.Persistence
{
    public sealed class CmsReadDbContext(DbContextOptions options) : DbContext(options), ICmsReadDbContext
    {
        public DbSet<CmsContentItem> ContentItems => Set<CmsContentItem>();
        public DbSet<CmsContentVersion> ContentVersions => Set<CmsContentVersion>();
        public DbSet<ProcessedCmsEvent> ProcessedCmsEvents => Set<ProcessedCmsEvent>();

        IQueryable<CmsContentItem> ICmsReadDbContext.ContentItems => ContentItems;
        IQueryable<CmsContentVersion> ICmsReadDbContext.ContentVersions => ContentVersions;
        IQueryable<ProcessedCmsEvent> ICmsReadDbContext.ProcessedCmsEvents => ProcessedCmsEvents;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(CmsContentItemConfiguration).Assembly);
        }
    }
}
