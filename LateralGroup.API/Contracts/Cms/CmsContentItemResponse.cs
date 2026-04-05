using LateralGroup.Domain.Enums;

namespace LateralGroup.API.Contracts.Cms;

public sealed class CmsContentItemResponse
{
    public string Id { get; init; } = default!;
    public int? LatestKnownVersion { get; init; }
    public int? LatestPublishedVersion { get; init; }
    public bool IsPublished { get; init; }
    public bool IsDisabledByCms { get; init; }
    public bool IsDisabledByAdmin { get; init; }
    public string? PayloadJson { get; init; }
    public DateTimeOffset LastEventTimestampUtc { get; init; }
    public string LastEventType { get; init; } = default!;
}

public static class CmsContentItemResponseExtensions
{
    public static CmsContentItemResponse ToResponse(this Application.Models.CmsContentItemResult result)
    {
        return new CmsContentItemResponse
        {
            Id = result.Id,
            LatestKnownVersion = result.LatestKnownVersion,
            LatestPublishedVersion = result.LatestPublishedVersion,
            IsPublished = result.IsPublished,
            IsDisabledByCms = result.IsDisabledByCms,
            IsDisabledByAdmin = result.IsDisabledByAdmin,
            PayloadJson = result.PayloadJson,
            LastEventTimestampUtc = result.LastEventTimestampUtc,
            LastEventType = result.LastEventType.ToString()
        };
    }
}


