using LateralGroup.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace LateralGroup.Application.Models
{
    public sealed class CmsContentItemResult
    {
        public string Id { get; init; } = default!;
        public int? LatestKnownVersion { get; init; }
        public int? LatestPublishedVersion { get; init; }
        public bool IsPublished { get; init; }
        public bool IsDisabledByCms { get; init; }
        public bool IsDisabledByAdmin { get; init; }
        public string? PayloadJson { get; init; }
        public DateTimeOffset LastEventTimestampUtc { get; init; }
        public CmsEventType LastEventType { get; init; }
    }
}
