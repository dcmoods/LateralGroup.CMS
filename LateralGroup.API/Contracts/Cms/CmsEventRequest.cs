using LateralGroup.Application.Models;
using System.Text.Json;

namespace LateralGroup.API.Contracts.Cms;

public class CmsEventRequest
{
    public string Type { get; init; } = default!;
    public string Id { get; init; } = default!;
    public JsonElement? Payload { get; init; }
    public int? Version { get; init; }
    public DateTimeOffset Timestamp { get; init; }
    //public int OriginalOrder { get; init; }
}

public static class CmsEventRequestExtensions
{
    public static ProcessCmsEventInput ToProcessInput(this CmsEventRequest request, int index)
    {
        return new ProcessCmsEventInput
        {
            Type = request.Type,
            Id = request.Id,
            PayloadJson = request.Payload.HasValue ? request.Payload.Value.GetRawText() : null,
            Version = request.Version,
            Timestamp = request.Timestamp,
            OriginalOrder = index
        };
    }

}
