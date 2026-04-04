using LateralGroup.Application.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace LateralGroup.Application.Abstractions.Services;

public interface ICmsContentQueryService
{
    Task<IReadOnlyCollection<CmsContentItemResult>> GetAllAsync(
        bool isAdmin,
        CancellationToken cancellationToken = default);

    Task<CmsContentItemResult?> GetByIdAsync(
        string id,
        bool isAdmin,
        CancellationToken cancellationToken = default);
}
