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

    public void Publish(int versionNumber, 
        string payloadJson, 
        DateTimeOffset eventTimestampUtc,
        DateTimeOffset updatedUtc)
    {
        LatestKnownVersion = versionNumber;
        LatestPublishedVersion = versionNumber;
        LatestPayloadJson = payloadJson;
        IsPublished = true;
        IsDisabledByCms = false;
        LastEventTimestampUtc = eventTimestampUtc;
        UpdatedUtc = updatedUtc;
        LastEventType = CmsEventType.Publish;

        UpsertVersion(versionNumber, payloadJson, wasPublished: true, wasUnpublished: false, observedAtUtc: updatedUtc);
    }

    public void Unpublish(int versionNumber,
        string payloadJson,
        DateTimeOffset eventTimestampUtc,
        DateTimeOffset updatedUtc)
    {
        LatestKnownVersion = versionNumber;
        LatestPayloadJson = payloadJson;
        IsPublished = false;
        IsDisabledByCms = true;
        LastEventTimestampUtc = eventTimestampUtc;
        UpdatedUtc = updatedUtc;
        LastEventType = CmsEventType.Unpublish;

        UpsertVersion(versionNumber, payloadJson, wasPublished: false, wasUnpublished: true, observedAtUtc: updatedUtc);
    }

    public void DisableByAdmin(DateTimeOffset updatedUtc)
    {
        IsDisabledByAdmin = true;
        UpdatedUtc = updatedUtc;
    }

    public void EnableByAdmin(DateTimeOffset updatedUtc)
    {
        IsDisabledByAdmin = false;
        UpdatedUtc = updatedUtc;
    }

    private void UpsertVersion(
        int versionNumber,
        string payloadJson,
        bool wasPublished,
        bool wasUnpublished,
        DateTimeOffset observedAtUtc)
    {
        var existingVersion = Versions.FirstOrDefault(v => v.Version == versionNumber);
        if (existingVersion != null)
        {
            existingVersion.PayloadJson = payloadJson;
            existingVersion.ObservedAtUtc = observedAtUtc;
            existingVersion.WasPublished = existingVersion.WasPublished || wasPublished;
            existingVersion.WasUnpublished = existingVersion.WasUnpublished || wasUnpublished;
        }
        else
        {
            Versions.Add(new CmsContentVersion
            {
                Version = versionNumber,
                PayloadJson = payloadJson,
                WasPublished = wasPublished,
                WasUnpublished = wasUnpublished,
                ObservedAtUtc = observedAtUtc
            });
        }

    }

}


