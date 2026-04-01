using LateralGroup.Domain.Enums;

namespace LateralGroup.Domain.Entities;

public class CmsContentItem
{
    public string Id { get; set; } = default!;
    public int? LatestKnownVersion { get; set; }
    public int? LatestPublishedVersion { get; set; }
    public string? LatestPayloadJson { get; set; }

    public bool IsPublished { get; set; }
    public bool IsDisabledByCms { get; set; }
    public bool IsDisabledByAdmin { get; set; }

    public DateTimeOffset LastEventTimestampUtc { get; set; }
    public CmsEventType LastEventType { get; set; }

    public DateTimeOffset CreatedUtc { get; set; }
    public DateTimeOffset UpdatedUtc { get; set; }

    public ICollection<CmsContentVersion> Versions { get; set; } = new List<CmsContentVersion>();
}


