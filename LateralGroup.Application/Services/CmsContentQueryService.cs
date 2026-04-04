using LateralGroup.Application.Abstractions.Persistence;
using LateralGroup.Application.Abstractions.Services;
using LateralGroup.Application.Models;
using LateralGroup.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LateralGroup.Application.Services;

public class CmsContentQueryService : ICmsContentQueryService
{
    private readonly ICmsReadDbContext _readContext;

    public CmsContentQueryService(ICmsReadDbContext readContext)
    {
        _readContext = readContext;
    }

    public async Task<IReadOnlyCollection<CmsContentItemResult>> GetAllAsync(bool isAdmin,
        CancellationToken cancellationToken = default)
    {
        var query = _readContext.ContentItems;

        var filteredQuery = ApplyFiltering(query, isAdmin);

        return await filteredQuery.Select(item => new CmsContentItemResult
            {
                Id = item.Id,
                LatestKnownVersion = item.LatestKnownVersion,
                LatestPublishedVersion = item.LatestPublishedVersion,
                IsPublished = item.IsPublished,
                IsDisabledByCms = item.IsDisabledByCms,
                IsDisabledByAdmin = item.IsDisabledByAdmin,
                PayloadJson = item.LatestPayloadJson,
                LastEventTimestampUtc = item.LastEventTimestampUtc,
                LastEventType = item.LastEventType
            }).ToListAsync(cancellationToken);
    }

    public async Task<CmsContentItemResult?> GetByIdAsync(string id, 
        bool isAdmin, 
        CancellationToken cancellationToken = default)
    {
        var query = _readContext.ContentItems.Where(item => item.Id == id);
        var filteredQuery = ApplyFiltering(query, isAdmin);

        return await filteredQuery.Select(item => new CmsContentItemResult
            {
                Id = item.Id,
                LatestKnownVersion = item.LatestKnownVersion,
                LatestPublishedVersion = item.LatestPublishedVersion,
                IsPublished = item.IsPublished,
                IsDisabledByCms = item.IsDisabledByCms,
                IsDisabledByAdmin = item.IsDisabledByAdmin,
                PayloadJson = item.LatestPayloadJson,
                LastEventTimestampUtc = item.LastEventTimestampUtc,
                LastEventType = item.LastEventType
            }).FirstOrDefaultAsync(cancellationToken);
    }

    private static IQueryable<CmsContentItem> ApplyFiltering(
        IQueryable<CmsContentItem> query, 
        bool isAdmin)
    {

        if (isAdmin)
        {
            return query;
        }

        return query.Where(item => 
            item.IsPublished && 
            !item.IsDisabledByCms && 
            !item.IsDisabledByAdmin);
    }
}
