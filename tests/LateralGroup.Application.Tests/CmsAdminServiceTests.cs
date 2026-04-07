using LateralGroup.Application.Services;
using LateralGroup.Application.Abstractions.Services;
using LateralGroup.Domain.Entities;
using LateralGroup.Domain.Enums;
using LateralGroup.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace LateralGroup.Application.Tests;

public sealed class CmsAdminServiceTests
{
    [Fact]
    public async Task DisableAsync_SetsAdminFlag_AndUpdatesTimestamp()
    {
        await using var fixture = await AdminFixture.CreateAsync();
        var item = CreateItem("item-1");
        fixture.WriteDbContext.ContentItems.Add(item);
        await fixture.WriteDbContext.SaveChangesAsync();

        var service = new CmsAdminService(fixture.WriteDbContext, fixture.Clock, NullLogger<CmsAdminService>.Instance);
        fixture.Clock.UtcNow = new DateTimeOffset(2026, 4, 5, 14, 0, 0, TimeSpan.Zero);

        var result = await service.DisableAsync("item-1");

        var stored = await fixture.WriteDbContext.ContentItems.SingleAsync(x => x.Id == "item-1");
        Assert.Equal(CmsAdminActionResult.Updated, result);
        Assert.True(stored.IsDisabledByAdmin);
        Assert.Equal(fixture.Clock.UtcNow, stored.UpdatedUtc);
        Assert.Equal(CmsEventType.Publish, stored.LastEventType);
    }

    [Fact]
    public async Task EnableAsync_ClearsAdminFlag_AndUpdatesTimestamp()
    {
        await using var fixture = await AdminFixture.CreateAsync();
        var item = CreateItem("item-1");
        item.DisableByAdmin(new DateTimeOffset(2026, 4, 5, 13, 0, 0, TimeSpan.Zero));
        fixture.WriteDbContext.ContentItems.Add(item);
        await fixture.WriteDbContext.SaveChangesAsync();

        var service = new CmsAdminService(fixture.WriteDbContext, fixture.Clock, NullLogger<CmsAdminService>.Instance);
        fixture.Clock.UtcNow = new DateTimeOffset(2026, 4, 5, 14, 0, 0, TimeSpan.Zero);

        var result = await service.EnableAsync("item-1");

        var stored = await fixture.WriteDbContext.ContentItems.SingleAsync(x => x.Id == "item-1");
        Assert.Equal(CmsAdminActionResult.Updated, result);
        Assert.False(stored.IsDisabledByAdmin);
        Assert.Equal(fixture.Clock.UtcNow, stored.UpdatedUtc);
    }

    [Fact]
    public async Task EnableAsync_ForMissingItem_ReturnsNotFound()
    {
        await using var fixture = await AdminFixture.CreateAsync();
        var service = new CmsAdminService(fixture.WriteDbContext, fixture.Clock, NullLogger<CmsAdminService>.Instance);

        var result = await service.EnableAsync("missing-item");

        Assert.Equal(CmsAdminActionResult.NotFound, result);
    }

    [Fact]
    public async Task DisableAsync_WhenAlreadyDisabled_ReturnsNoChange()
    {
        await using var fixture = await AdminFixture.CreateAsync();
        var item = CreateItem("item-1");
        item.DisableByAdmin(new DateTimeOffset(2026, 4, 5, 13, 0, 0, TimeSpan.Zero));
        fixture.WriteDbContext.ContentItems.Add(item);
        await fixture.WriteDbContext.SaveChangesAsync();

        var service = new CmsAdminService(fixture.WriteDbContext, fixture.Clock, NullLogger<CmsAdminService>.Instance);

        var result = await service.DisableAsync("item-1");

        Assert.Equal(CmsAdminActionResult.NoChange, result);
    }

    [Fact]
    public async Task EnableAsync_WhenAlreadyEnabled_ReturnsNoChange()
    {
        await using var fixture = await AdminFixture.CreateAsync();
        var item = CreateItem("item-1");
        fixture.WriteDbContext.ContentItems.Add(item);
        await fixture.WriteDbContext.SaveChangesAsync();

        var service = new CmsAdminService(fixture.WriteDbContext, fixture.Clock, NullLogger<CmsAdminService>.Instance);

        var result = await service.EnableAsync("item-1");

        Assert.Equal(CmsAdminActionResult.NoChange, result);
    }

    private static CmsContentItem CreateItem(string id)
    {
        return new CmsContentItem
        {
            Id = id,
            LatestKnownVersion = 1,
            LatestPublishedVersion = 1,
            LatestPayloadJson = "{\"title\":\"Test\"}",
            IsPublished = true,
            IsDisabledByCms = false,
            IsDisabledByAdmin = false,
            LastEventTimestampUtc = new DateTimeOffset(2026, 4, 5, 12, 0, 0, TimeSpan.Zero),
            LastEventType = CmsEventType.Publish,
            CreatedUtc = new DateTimeOffset(2026, 4, 5, 12, 0, 0, TimeSpan.Zero),
            UpdatedUtc = new DateTimeOffset(2026, 4, 5, 12, 0, 0, TimeSpan.Zero)
        };
    }

    private sealed class AdminFixture : IAsyncDisposable
    {
        private readonly SqliteConnection _connection;

        private AdminFixture(SqliteConnection connection, CmsWriteDbContext writeDbContext, TestClock clock)
        {
            _connection = connection;
            WriteDbContext = writeDbContext;
            Clock = clock;
        }

        public CmsWriteDbContext WriteDbContext { get; }
        public TestClock Clock { get; }

        public static async Task<AdminFixture> CreateAsync()
        {
            var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();

            var options = new DbContextOptionsBuilder<CmsWriteDbContext>()
                .UseSqlite(connection)
                .Options;

            var writeDbContext = new CmsWriteDbContext(options);
            await writeDbContext.Database.EnsureCreatedAsync();
            var clock = new TestClock(new DateTimeOffset(2026, 4, 5, 12, 0, 0, TimeSpan.Zero));

            return new AdminFixture(connection, writeDbContext, clock);
        }

        public async ValueTask DisposeAsync()
        {
            await WriteDbContext.DisposeAsync();
            await _connection.DisposeAsync();
        }
    }
}
