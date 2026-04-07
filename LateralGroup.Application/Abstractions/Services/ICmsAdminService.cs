namespace LateralGroup.Application.Abstractions.Services;

public interface ICmsAdminService
{
    Task<CmsAdminActionResult> DisableAsync(
        string id,
        CancellationToken cancellationToken = default);

    Task<CmsAdminActionResult> EnableAsync(
        string id,
        CancellationToken cancellationToken = default);
}
