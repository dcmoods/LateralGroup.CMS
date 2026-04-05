using LateralGroup.Application.Abstractions.Persistence;
using LateralGroup.Application.Abstractions.Services;
using LateralGroup.Application.Models;
using LateralGroup.Domain.Entities;
using LateralGroup.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace LateralGroup.Application.Services;
public sealed class CmsEventProcessor : ICmsEventProcessor
{
    private readonly ICmsWriteDbContext _writeDbContext;
    private readonly IClock _clock;
    private readonly ILogger<CmsEventProcessor> _logger;

    public CmsEventProcessor(
        ICmsWriteDbContext dbContext,
        IClock clock,
        ILogger<CmsEventProcessor> logger)
    {
        _writeDbContext = dbContext;
        _clock = clock;
        _logger = logger;
    }

    public async Task<BatchProcessResult> ProcessAsync(
        IReadOnlyCollection<ProcessCmsEventInput> events,
        CancellationToken cancellationToken = default)
    {
        if (events is null)
        {
            throw new ArgumentNullException(nameof(events));
        }

        var orderedEvents = events
            .OrderBy(x => x.Timestamp)
            .ThenBy(x => x.Version ?? int.MaxValue)
            .ThenBy(GetEventPriority)
            .ThenBy(x => x.OriginalOrder)
            .ToList();

        _logger.LogInformation("Processing CMS batch with {Count} events.", orderedEvents.Count);

        var processed = 0;
        var ignored = 0;
        var failed = 0;

        foreach (var input in orderedEvents)
        {
            try
            {
                var validationError = Validate(input);
                if (validationError is not null)
                {
                    failed++;

                    await AddProcessedEventLogAsync(
                        input,
                        status: ProcessedEventStatus.Failed,
                        failureReason: validationError,
                        cancellationToken: cancellationToken);

                    await _writeDbContext.SaveChangesAsync(cancellationToken);

                    _logger.LogWarning(
                        "Failed CMS event for content item {ContentItemId}. Reason: {Reason}",
                        input.Id,
                        validationError);

                    continue;
                }

                var eventType = ParseEventType(input.Type);

                var entity = await _writeDbContext.ContentItems
                    .Include(x => x.Versions)
                    .FirstOrDefaultAsync(x => x.Id == input.Id, cancellationToken);

                if (IsStale(entity, input))
                {
                    ignored++;

                    await AddProcessedEventLogAsync(
                        input,
                        status: ProcessedEventStatus.Ignored,
                        failureReason: "Ignored stale event.",
                        cancellationToken: cancellationToken);

                    await _writeDbContext.SaveChangesAsync(cancellationToken);

                    _logger.LogInformation(
                        "Ignored stale {EventType} event for content item {ContentItemId}.",
                        eventType,
                        input.Id);

                    continue;
                }

                switch (eventType)
                {
                    case CmsEventType.Publish:
                        await HandlePublishAsync(entity, input, cancellationToken);
                        break;

                    case CmsEventType.Unpublish:
                        await HandleUnpublishAsync(entity, input, cancellationToken);
                        break;

                    case CmsEventType.Delete:
                        await HandleDeleteAsync(entity, input, cancellationToken);
                        break;

                    default:
                        throw new InvalidOperationException($"Unsupported event type: {eventType}");
                }

                await AddProcessedEventLogAsync(
                    input,
                    status: ProcessedEventStatus.Processed,
                    failureReason: null,
                    cancellationToken: cancellationToken);

                await _writeDbContext.SaveChangesAsync(cancellationToken);

                processed++;

                _logger.LogInformation(
                    "Processed {EventType} event for content item {ContentItemId}.",
                    eventType,
                    input.Id);
            }
            catch (Exception ex)
            {
                failed++;

                _logger.LogError(
                    ex,
                    "Unhandled exception while processing CMS event for content item {ContentItemId}.",
                    input.Id);

                await AddProcessedEventLogAsync(
                    input,
                    status: ProcessedEventStatus.Failed,
                    failureReason: ex.Message,
                    cancellationToken: cancellationToken);

                await _writeDbContext.SaveChangesAsync(cancellationToken);
            }
        }

        return new BatchProcessResult
        {
            Received = orderedEvents.Count,
            Processed = processed,
            Ignored = ignored,
            Failed = failed
        };
    }

    private async Task HandlePublishAsync(
        CmsContentItem? entity,
        ProcessCmsEventInput input,
        CancellationToken cancellationToken)
    {
        var payloadJson = NormalizeJson(input.PayloadJson!);
        var version = input.Version!.Value;

        entity ??= new CmsContentItem
        {
            Id = input.Id,
            CreatedUtc = _clock.UtcNow
        };

        entity.Publish(version, payloadJson, input.Timestamp, _clock.UtcNow);

        if (_writeDbContext.Entry(entity).State == EntityState.Detached)
        {
            _writeDbContext.ContentItems.Add(entity);
        }

        await Task.CompletedTask;
    }

    private async Task HandleUnpublishAsync(
        CmsContentItem? entity,
        ProcessCmsEventInput input,
        CancellationToken cancellationToken)
    {
        var payloadJson = NormalizeJson(input.PayloadJson!);
        var version = input.Version!.Value;

        entity ??= new CmsContentItem
        {
            Id = input.Id,
            CreatedUtc = _clock.UtcNow
        };


        entity.Unpublish(version, payloadJson, input.Timestamp, _clock.UtcNow);

        if (_writeDbContext.Entry(entity).State == EntityState.Detached)
        {
            _writeDbContext.ContentItems.Add(entity);
        }

        await Task.CompletedTask;
    }

    private async Task HandleDeleteAsync(
        CmsContentItem? entity,
        ProcessCmsEventInput input,
        CancellationToken cancellationToken)
    {
        if (entity is null)
        {
            return;
        }

        _writeDbContext.ContentVersions.RemoveRange(entity.Versions);
        _writeDbContext.ContentItems.Remove(entity);

        await Task.CompletedTask;
    }

    private async Task AddProcessedEventLogAsync(
        ProcessCmsEventInput input,
        ProcessedEventStatus status,
        string? failureReason,
        CancellationToken cancellationToken)
    {
        var parsed = TryParseEventType(input.Type, out var eventType);

        var log = new ProcessedCmsEvent
        {
            ContentItemId = input.Id,
            EventType = parsed ? eventType : CmsEventType.Unknown,
            Version = input.Version,
            TimestampUtc = input.Timestamp,
            RawEventJson = SerializeRawEvent(input),
            Status = status,
            FailureReason = failureReason,
            CreatedUtc = _clock.UtcNow
        };

        _writeDbContext.ProcessedCmsEvents.Add(log);
        await Task.CompletedTask;
    }

    private static bool IsStale(CmsContentItem? entity, ProcessCmsEventInput input)
    {
        if (entity is null)
        {
            return false;
        }

        if (input.Timestamp < entity.LastEventTimestampUtc)
        {
            return true;
        }

        if (input.Timestamp > entity.LastEventTimestampUtc)
        {
            return false;
        }

        // same timestamp
        if (!input.Version.HasValue || !entity.LatestKnownVersion.HasValue)
        {
            return false;
        }

        return input.Version.Value < entity.LatestKnownVersion.Value;
    }

    private static string? Validate(ProcessCmsEventInput input)
    {
        if (input is null)
        {
            return "Event is required.";
        }

        if (string.IsNullOrWhiteSpace(input.Type))
        {
            return "Event type is required.";
        }

        if (string.IsNullOrWhiteSpace(input.Id))
        {
            return "Entity id is required.";
        }

        if (!TryParseEventType(input.Type, out var eventType))
        {
            return $"Unsupported event type '{input.Type}'.";
        }

        if (input.Timestamp == default)
        {
            return "Timestamp is required.";
        }

        if (eventType is CmsEventType.Publish or CmsEventType.Unpublish)
        {
            if (!input.Version.HasValue || input.Version.Value <= 0)
            {
                return $"Version is required for {eventType} events.";
            }

            if (string.IsNullOrWhiteSpace(input.PayloadJson))
            {
                return $"Payload is required for {eventType} events.";
            }

            try
            {
                JsonDocument.Parse(input.PayloadJson);
            }
            catch
            {
                return "Payload must be valid JSON.";
            }
        }

        return null;
    }

    private static CmsEventType ParseEventType(string type)
    {
        if (!TryParseEventType(type, out var eventType))
        {
            throw new InvalidOperationException($"Unsupported event type '{type}'.");
        }

        return eventType;
    }

    private static bool TryParseEventType(string type, out CmsEventType eventType)
    {
        return Enum.TryParse(type?.Trim(), ignoreCase: true, out eventType);
    }

    private static int GetEventPriority(ProcessCmsEventInput input)
    {
        return TryParseEventType(input.Type, out var eventType)
            ? eventType switch
            {
                CmsEventType.Publish => 0,
                CmsEventType.Unpublish => 1,
                CmsEventType.Delete => 2,
                _ => 3
            }
            : 4;
    }

    private static string NormalizeJson(string payloadJson)
    {
        using var document = JsonDocument.Parse(payloadJson);
        return JsonSerializer.Serialize(document.RootElement);
    }

    private static string SerializeRawEvent(ProcessCmsEventInput input)
    {
        return JsonSerializer.Serialize(new
        {
            input.Type,
            input.Id,
            input.PayloadJson,
            input.Version,
            input.Timestamp,
            input.OriginalOrder
        });
    }
}
