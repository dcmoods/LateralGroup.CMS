using LateralGroup.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace LateralGroup.Application.Common.Abstractions
{
    public interface ICmsWriteDbContext
    {
        DbSet<CmsContentItem> ContentItems { get; }
        DbSet<CmsContentVersion> ContentVersions { get; }
        DbSet<ProcessedCmsEvent> ProcessedCmsEvents { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        EntityEntry Entry(object entity);
    }
}
