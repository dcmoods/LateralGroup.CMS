using LateralGroup.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace LateralGroup.Application.Abstractions.Persistence;

public interface ICmsReadDbContext
{
    IQueryable<CmsContentItem> ContentItems { get; }
    IQueryable<CmsContentVersion> ContentVersions { get; }
    IQueryable<ProcessedCmsEvent> ProcessedCmsEvents { get; }
}
