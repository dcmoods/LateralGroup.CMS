namespace LateralGroup.Domain.Entities;

public class ProcessedCmsEvent
{
    public long Id { get; set; }
    public string ContentItemId { get; set; } = default!;
    public string EventType { get; set; } = default!;
    public int? Version { get; set; }
    public DateTimeOffset TimestampUtc { get; set; }

    public string RawEventJson { get; set; } = default!;
    public string Status { get; set; } = default!; 
    public string? FailureReason { get; set; }

    public DateTimeOffset CreatedUtc { get; set; }
}
