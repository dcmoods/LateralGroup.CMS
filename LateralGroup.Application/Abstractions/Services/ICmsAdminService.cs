namespace LateralGroup.Application.Abstractions.Services;

public interface ICmsAdminService
{
    Task<bool> DisableAsync(
        string id,
        CancellationToken cancellationToken = default);

    Task<bool> EnableAsync(
        string id,
        CancellationToken cancellationToken = default);
}
