using LateralGroup.Domain.Enums;

namespace LateralGroup.Domain.Entities;

public class ProcessedCmsEvent
{
    public long Id { get; set; }
    public string ContentItemId { get; set; } = default!;
    public CmsEventType EventType { get; set; } = CmsEventType.Unpublish;
    public int? Version { get; set; }
    public DateTimeOffset TimestampUtc { get; set; }

    public string RawEventJson { get; set; } = default!;
    public ProcessedEventStatus Status { get; set; }  
    public string? FailureReason { get; set; }
    
    public DateTimeOffset CreatedUtc { get; set; }
}
