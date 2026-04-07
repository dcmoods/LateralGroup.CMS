using LateralGroup.Application.Abstractions.Persistence;
using LateralGroup.Application.Abstractions.Services;
using LateralGroup.Application.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LateralGroup.Application.Services;

public class CmsAdminService : ICmsAdminService
{
    private readonly ICmsWriteDbContext _writeDbContext;
    private readonly IClock _clock;
    private readonly ILogger<CmsAdminService> _logger;

    public CmsAdminService(ICmsWriteDbContext writeDbContext, IClock clock, ILogger<CmsAdminService> logger)
    {
        _writeDbContext = writeDbContext;
        _clock = clock;
        _logger = logger;
    }

    public async Task<CmsAdminActionResult> DisableAsync(string id, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ValidationException("Id cannot be null or empty.");

        var contentItem = await _writeDbContext.ContentItems
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (contentItem == null)
        {
            _logger.LogWarning("Attempted to disable non-existent content item with ID {ContentItemId}", id);
            return CmsAdminActionResult.NotFound;
        }
        if (contentItem.IsDisabledByAdmin)
        {
            _logger.LogInformation("Content item with ID {ContentItemId} is already disabled by admin", id);
            return CmsAdminActionResult.NoChange;
        }
        contentItem.DisableByAdmin(_clock.UtcNow);
        await _writeDbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Content item with ID {ContentItemId} has been disabled by admin", id);
        return CmsAdminActionResult.Updated;
    }

    public async Task<CmsAdminActionResult> EnableAsync(string id, CancellationToken cancellationToken = default)
    {
        if(string.IsNullOrWhiteSpace(id))
            throw new ValidationException("Id cannot be null or empty.");

        var contentItem = await _writeDbContext.ContentItems
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (contentItem == null)
        {
            _logger.LogWarning("Attempted to enable non-existent content item with ID {ContentItemId}", id);
            return CmsAdminActionResult.NotFound;
        }

        if (!contentItem.IsDisabledByAdmin)
        {
            _logger.LogInformation("Content item with ID {ContentItemId} is not currently disabled by admin", id);
            return CmsAdminActionResult.NoChange;
        }
        contentItem.EnableByAdmin(_clock.UtcNow);
        await _writeDbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Content item with ID {ContentItemId} has been enabled by admin", id);
        return CmsAdminActionResult.Updated;
    }
}
