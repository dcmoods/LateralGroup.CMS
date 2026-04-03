using LateralGroup.Application.Models;
using LateralGroup.Application.Services;
using LateralGroup.Domain.Entities;
using LateralGroup.Domain.Enums;
using LateralGroup.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace LateralGroup.Application.Tests;

public sealed class CmsEventProcessorTests
{
    [Fact]
    public async Task ProcessAsync_PersistsFailedAuditLog_ForUnsupportedEventType()
    {
        await using var fixture = await TestFixture.CreateAsync();

        var result = await fixture.Processor.ProcessAsync(
            [
                new ProcessCmsEventInput
                {
                    Type = "archive",
                    Id = "item-1",
                    Timestamp = new DateTimeOffset(2026, 4, 3, 12, 0, 0, TimeSpan.Zero),
                    OriginalOrder = 0
                }
            ]);

        var processedEvent = await fixture.WriteDbContext.ProcessedCmsEvents.SingleAsync();

        Assert.Equal(1, result.Failed);
        Assert.Equal(ProcessedEventStatus.Failed, processedEvent.Status);
        Assert.Equal(CmsEventType.Unknown, processedEvent.EventType);
        Assert.Contains("Unsupported event type", processedEvent.FailureReason);
    }

    [Fact]
    public async Task ProcessAsync_PersistsIgnoredAuditLog_ForStaleEvent()
    {
        await using var fixture = await TestFixture.CreateAsync();

        fixture.WriteDbContext.ContentItems.Add(new CmsContentItem
        {
            Id = "item-1",
            LatestKnownVersion = 2,
            LatestPublishedVersion = 2,
            LatestPayloadJson = "{\"title\":\"Current\"}",
            IsPublished = true,
            LastEventTimestampUtc = new DateTimeOffset(2026, 4, 3, 12, 5, 0, TimeSpan.Zero),
            LastEventType = CmsEventType.Publish,
            CreatedUtc = fixture.Clock.UtcNow,
            UpdatedUtc = fixture.Clock.UtcNow
        });

        await fixture.WriteDbContext.SaveChangesAsync();

        var result = await fixture.Processor.ProcessAsync(
            [
                new ProcessCmsEventInput
                {
                    Type = "publish",
                    Id = "item-1",
                    Version = 1,
                    PayloadJson = "{\"title\":\"Old\"}",
                    Timestamp = new DateTimeOffset(2026, 4, 3, 12, 0, 0, TimeSpan.Zero),
                    OriginalOrder = 0
                }
            ]);

        var processedEvent = await fixture.WriteDbContext.ProcessedCmsEvents.SingleAsync();

        Assert.Equal(1, result.Ignored);
        Assert.Equal(ProcessedEventStatus.Ignored, processedEvent.Status);
        Assert.Equal(CmsEventType.Publish, processedEvent.EventType);
        Assert.Equal("Ignored stale event.", processedEvent.FailureReason);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(1, 0)]
    public async Task ProcessAsync_MakesDeleteWin_ForSameTimestampPublishAndDelete(
        int publishOriginalOrder,
        int deleteOriginalOrder)
    {
        await using var fixture = await TestFixture.CreateAsync();
        var timestamp = new DateTimeOffset(2026, 4, 3, 12, 0, 0, TimeSpan.Zero);

        var result = await fixture.Processor.ProcessAsync(
            [
                new ProcessCmsEventInput
                {
                    Type = "delete",
                    Id = "item-1",
                    Timestamp = timestamp,
                    OriginalOrder = deleteOriginalOrder
                },
                new ProcessCmsEventInput
                {
                    Type = "publish",
                    Id = "item-1",
                    Version = 1,
                    PayloadJson = "{\"title\":\"Hello\"}",
                    Timestamp = timestamp,
                    OriginalOrder = publishOriginalOrder
                }
            ]);

        var contentItem = await fixture.WriteDbContext.ContentItems.SingleOrDefaultAsync(x => x.Id == "item-1");
        var processedEvents = await fixture.WriteDbContext.ProcessedCmsEvents
            .OrderBy(x => x.Id)
            .ToListAsync();

        Assert.Equal(2, result.Processed);
        Assert.Null(contentItem);
        Assert.Collection(
            processedEvents,
            entry => Assert.Equal(CmsEventType.Publish, entry.EventType),
            entry => Assert.Equal(CmsEventType.Delete, entry.EventType));
    }

    private sealed class TestFixture : IAsyncDisposable
    {
        private readonly SqliteConnection _connection;

        private TestFixture(SqliteConnection connection, CmsWriteDbContext writeDbContext, TestClock clock)
        {
            _connection = connection;
            WriteDbContext = writeDbContext;
            Clock = clock;
            Processor = new CmsEventProcessor(writeDbContext, clock, NullLogger<CmsEventProcessor>.Instance);
        }

        public TestClock Clock { get; }

        public CmsEventProcessor Processor { get; }

        public CmsWriteDbContext WriteDbContext { get; }

        public static async Task<TestFixture> CreateAsync()
        {
            var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();

            var options = new DbContextOptionsBuilder<CmsWriteDbContext>()
                .UseSqlite(connection)
                .Options;

            var writeDbContext = new CmsWriteDbContext(options);
            await writeDbContext.Database.EnsureCreatedAsync();

            return new TestFixture(
                connection,
                writeDbContext,
                new TestClock(new DateTimeOffset(2026, 4, 3, 12, 0, 0, TimeSpan.Zero)));
        }

        public async ValueTask DisposeAsync()
        {
            await WriteDbContext.DisposeAsync();
            await _connection.DisposeAsync();
        }
    }
}
