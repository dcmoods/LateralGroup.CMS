namespace LateralGroup.Application.Models;
public sealed class ProcessCmsEventInput
{
    public string Type { get; init; } = default!;
    public string Id { get; init; } = default!;
    public string? PayloadJson { get; init; }
    public int? Version { get; init; }
    public DateTimeOffset Timestamp { get; init; }
    public int OriginalOrder { get; init; }
}
