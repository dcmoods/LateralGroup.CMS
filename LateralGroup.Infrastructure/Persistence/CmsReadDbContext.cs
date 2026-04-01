using LateralGroup.Application.Common.Abstractions;
using LateralGroup.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LateralGroup.Infrastructure.Persistence
{
    public sealed class CmsReadDbContext(DbContextOptions options) : DbContext(options), ICmsReadDbContext
    {
        public IQueryable<CmsContentItem> ContentItems => Set<CmsContentItem>();

        public IQueryable<CmsContentVersion> ContentVersions => Set<CmsContentVersion>();

        public IQueryable<ProcessedCmsEvent> ProcessedCmsEvents => Set<ProcessedCmsEvent>();

    }
}
