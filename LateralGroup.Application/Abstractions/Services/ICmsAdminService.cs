namespace LateralGroup.Application.Abstractions.Services;

public interface ICmsAdminService
{
    Task DisableAsync(
        string id,
        CancellationToken cancellationToken = default);

    Task EnableAsync(
        string id,
        CancellationToken cancellationToken = default);
}
