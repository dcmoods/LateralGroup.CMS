using LateralGroup.Application.Services;
using LateralGroup.Domain.Entities;
using LateralGroup.Domain.Enums;
using LateralGroup.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace LateralGroup.Application.Tests;

public sealed class CmsContentQueryServiceTests
{
    [Fact]
    public async Task GetAllAsync_ForConsumer_ReturnsOnlyVisibleItems()
    {
        await using var fixture = await QueryFixture.CreateAsync();
        fixture.WriteDbContext.ContentItems.AddRange(
            CreateItem("visible", isPublished: true, isDisabledByCms: false, isDisabledByAdmin: false),
            CreateItem("cms-disabled", isPublished: false, isDisabledByCms: true, isDisabledByAdmin: false),
            CreateItem("admin-disabled", isPublished: true, isDisabledByCms: false, isDisabledByAdmin: true));
        await fixture.WriteDbContext.SaveChangesAsync();

        var service = new CmsContentQueryService(fixture.ReadDbContext);

        var result = await service.GetAllAsync(isAdmin: false);

        var item = Assert.Single(result);
        Assert.Equal("visible", item.Id);
    }

    [Fact]
    public async Task GetAllAsync_ForAdmin_ReturnsAllItems()
    {
        await using var fixture = await QueryFixture.CreateAsync();
        fixture.WriteDbContext.ContentItems.AddRange(
            CreateItem("a-visible", isPublished: true, isDisabledByCms: false, isDisabledByAdmin: false),
            CreateItem("b-cms-disabled", isPublished: false, isDisabledByCms: true, isDisabledByAdmin: false),
            CreateItem("c-admin-disabled", isPublished: true, isDisabledByCms: false, isDisabledByAdmin: true));
        await fixture.WriteDbContext.SaveChangesAsync();

        var service = new CmsContentQueryService(fixture.ReadDbContext);

        var result = await service.GetAllAsync(isAdmin: true);

        Assert.Equal(3, result.Count);
        Assert.Equal(["a-visible", "b-cms-disabled", "c-admin-disabled"], result.Select(x => x.Id).ToArray());
    }

    [Fact]
    public async Task GetByIdAsync_ForConsumer_ReturnsNull_WhenItemIsFilteredOut()
    {
        await using var fixture = await QueryFixture.CreateAsync();
        fixture.WriteDbContext.ContentItems.Add(CreateItem("hidden", isPublished: false, isDisabledByCms: true, isDisabledByAdmin: false));
        await fixture.WriteDbContext.SaveChangesAsync();

        var service = new CmsContentQueryService(fixture.ReadDbContext);

        var result = await service.GetByIdAsync("hidden", isAdmin: false);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdAsync_ForAdmin_ReturnsHiddenItem()
    {
        await using var fixture = await QueryFixture.CreateAsync();
        fixture.WriteDbContext.ContentItems.Add(CreateItem("hidden", isPublished: false, isDisabledByCms: true, isDisabledByAdmin: false));
        await fixture.WriteDbContext.SaveChangesAsync();

        var service = new CmsContentQueryService(fixture.ReadDbContext);

        var result = await service.GetByIdAsync("hidden", isAdmin: true);

        Assert.NotNull(result);
        Assert.Equal("hidden", result.Id);
        Assert.False(result.IsPublished);
        Assert.True(result.IsDisabledByCms);
    }

    private static CmsContentItem CreateItem(
        string id,
        bool isPublished,
        bool isDisabledByCms,
        bool isDisabledByAdmin)
    {
        return new CmsContentItem
        {
            Id = id,
            LatestKnownVersion = 1,
            LatestPublishedVersion = isPublished ? 1 : null,
            LatestPayloadJson = "{\"title\":\"Test\"}",
            IsPublished = isPublished,
            IsDisabledByCms = isDisabledByCms,
            IsDisabledByAdmin = isDisabledByAdmin,
            LastEventTimestampUtc = new DateTimeOffset(2026, 4, 5, 12, 0, 0, TimeSpan.Zero),
            LastEventType = isPublished ? CmsEventType.Publish : CmsEventType.Unpublish,
            CreatedUtc = new DateTimeOffset(2026, 4, 5, 12, 0, 0, TimeSpan.Zero),
            UpdatedUtc = new DateTimeOffset(2026, 4, 5, 12, 0, 0, TimeSpan.Zero)
        };
    }

    private sealed class QueryFixture : IAsyncDisposable
    {
        private readonly SqliteConnection _connection;

        private QueryFixture(SqliteConnection connection, CmsWriteDbContext writeDbContext, CmsReadDbContext readDbContext)
        {
            _connection = connection;
            WriteDbContext = writeDbContext;
            ReadDbContext = readDbContext;
        }

        public CmsWriteDbContext WriteDbContext { get; }
        public CmsReadDbContext ReadDbContext { get; }

        public static async Task<QueryFixture> CreateAsync()
        {
            var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();

            var writeOptions = new DbContextOptionsBuilder<CmsWriteDbContext>()
                .UseSqlite(connection)
                .Options;

            var readOptions = new DbContextOptionsBuilder<CmsReadDbContext>()
                .UseSqlite(connection)
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                .Options;

            var writeDbContext = new CmsWriteDbContext(writeOptions);
            await writeDbContext.Database.EnsureCreatedAsync();
            var readDbContext = new CmsReadDbContext(readOptions);

            return new QueryFixture(connection, writeDbContext, readDbContext);
        }

        public async ValueTask DisposeAsync()
        {
            await ReadDbContext.DisposeAsync();
            await WriteDbContext.DisposeAsync();
            await _connection.DisposeAsync();
        }
    }
}
