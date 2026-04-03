using LateralGroup.Application.Models;

namespace LateralGroup.Application.Abstractions.Services;

public interface ICmsEventProcessor
{
    Task<BatchProcessResult> ProcessAsync(
        IReadOnlyCollection<ProcessCmsEventInput> events,
        CancellationToken cancellationToken = default);
}