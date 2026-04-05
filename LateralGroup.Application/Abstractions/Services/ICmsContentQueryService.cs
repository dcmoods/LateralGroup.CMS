using LateralGroup.Application.Models;

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
