namespace LateralGroup.Domain.Entities;

public class CmsContentVersion
{
    public long Id { get; set; }
    public string ContentItemId { get; set; } = default!;
    public int Version { get; set; }
    public string PayloadJson { get; set; } = default!;

    public bool WasPublished { get; set; }
    public bool WasUnpublished { get; set; }

    public DateTimeOffset ObservedAtUtc { get; set; }

    public CmsContentItem ContentItem { get; set; } = default!;
}


