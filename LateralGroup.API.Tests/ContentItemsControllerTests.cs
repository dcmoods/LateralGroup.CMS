using LateralGroup.API.Contracts.Cms;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;

namespace LateralGroup.API.Tests;

public class ContentItemsControllerTests
{
    private readonly WebApplicationFactory<Program> _factory;

    public ContentItemsControllerTests()
    {
        _factory = new WebApplicationFactory<Program>();
    }

    [Fact]
    public async Task Get_ConsumerUser_ReturnsVisiblePublishedItems()
    {
        var itemId = $"visible-{Guid.NewGuid():N}";
        await PublishAsync(itemId, "Hello");

        var consumerClient = CreateAuthenticatedClient("consumerapi1", "5233c2da-c875-4920-a1a7-fca8c44902c4");

        var response = await consumerClient.GetAsync("/api/content-items");

        response.EnsureSuccessStatusCode();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadFromJsonAsync<List<CmsContentItemResponse>>();

        Assert.NotNull(content);
        Assert.Contains(content, item => item.Id == itemId);
    }

    [Fact]
    public async Task Get_ConsumerUser_HidesUnpublishedItems()
    {
        var hiddenItemId = $"hidden-{Guid.NewGuid():N}";
        await UnpublishAsync(hiddenItemId, "Hidden");

        var consumerClient = CreateAuthenticatedClient("consumerapi1", "5233c2da-c875-4920-a1a7-fca8c44902c4");

        var response = await consumerClient.GetAsync("/api/content-items");

        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadFromJsonAsync<List<CmsContentItemResponse>>();

        Assert.NotNull(content);
        Assert.DoesNotContain(content, item => item.Id == hiddenItemId);
    }

    [Fact]
    public async Task Get_AdminUser_ReturnsUnpublishedItems()
    {
        var hiddenItemId = $"admin-visible-{Guid.NewGuid():N}";
        await UnpublishAsync(hiddenItemId, "Hidden for consumer");

        var adminClient = CreateAuthenticatedClient("adminviewer1", "3ca5e864-6bcf-4cd0-a2f1-2f65dc5fbcab");

        var response = await adminClient.GetAsync("/api/content-items");

        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadFromJsonAsync<List<CmsContentItemResponse>>();

        Assert.NotNull(content);
        var item = Assert.Single(content, x => x.Id == hiddenItemId);
        Assert.False(item.IsPublished);
        Assert.True(item.IsDisabledByCms);
    }

    [Fact]
    public async Task GetById_ConsumerUser_ReturnsNotFound_ForUnpublishedItem()
    {
        var hiddenItemId = $"byid-hidden-{Guid.NewGuid():N}";
        await UnpublishAsync(hiddenItemId, "Hidden by id");

        var consumerClient = CreateAuthenticatedClient("consumerapi1", "5233c2da-c875-4920-a1a7-fca8c44902c4");

        var response = await consumerClient.GetAsync($"/api/content-items/{hiddenItemId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetById_AdminUser_ReturnsItem_ForUnpublishedItem()
    {
        var hiddenItemId = $"byid-admin-{Guid.NewGuid():N}";
        await UnpublishAsync(hiddenItemId, "Visible to admin");

        var adminClient = CreateAuthenticatedClient("adminviewer1", "3ca5e864-6bcf-4cd0-a2f1-2f65dc5fbcab");

        var response = await adminClient.GetAsync($"/api/content-items/{hiddenItemId}");

        response.EnsureSuccessStatusCode();

        var item = await response.Content.ReadFromJsonAsync<CmsContentItemResponse>();

        Assert.NotNull(item);
        Assert.Equal(hiddenItemId, item.Id);
        Assert.False(item.IsPublished);
        Assert.True(item.IsDisabledByCms);
    }

    [Fact]
    public async Task Disable_AdminUser_ReturnsNoContent_AndHidesItemFromConsumer()
    {
        var itemId = $"admin-disable-{Guid.NewGuid():N}";
        await PublishAsync(itemId, "Disable me");

        var adminClient = CreateAuthenticatedClient("adminviewer1", "3ca5e864-6bcf-4cd0-a2f1-2f65dc5fbcab");

        var disableResponse = await adminClient.PostAsync($"/api/content-items/{itemId}/disable", content: null);

        Assert.Equal(HttpStatusCode.NoContent, disableResponse.StatusCode);

        var consumerClient = CreateAuthenticatedClient("consumerapi1", "5233c2da-c875-4920-a1a7-fca8c44902c4");
        var consumerListResponse = await consumerClient.GetAsync("/api/content-items");
        consumerListResponse.EnsureSuccessStatusCode();
        var consumerItems = await consumerListResponse.Content.ReadFromJsonAsync<List<CmsContentItemResponse>>();

        Assert.NotNull(consumerItems);
        Assert.DoesNotContain(consumerItems, item => item.Id == itemId);

        var adminListResponse = await adminClient.GetAsync("/api/content-items");
        adminListResponse.EnsureSuccessStatusCode();
        var adminItems = await adminListResponse.Content.ReadFromJsonAsync<List<AdminContentItemResponse>>();

        Assert.NotNull(adminItems);
        var adminItem = Assert.Single(adminItems, item => item.Id == itemId);
        Assert.True(adminItem.IsDisabledByAdmin);
    }

    [Fact]
    public async Task Disable_ConsumerUser_ReturnsForbidden()
    {
        var itemId = $"consumer-disable-{Guid.NewGuid():N}";
        await PublishAsync(itemId, "Not allowed");

        var consumerClient = CreateAuthenticatedClient("consumerapi1", "5233c2da-c875-4920-a1a7-fca8c44902c4");

        var response = await consumerClient.PostAsync($"/api/content-items/{itemId}/disable", content: null);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Enable_AdminUser_ReturnsNoContent_AndRestoresItemToConsumer()
    {
        var itemId = $"admin-enable-{Guid.NewGuid():N}";
        await PublishAsync(itemId, "Enable me");

        var adminClient = CreateAuthenticatedClient("adminviewer1", "3ca5e864-6bcf-4cd0-a2f1-2f65dc5fbcab");

        var disableResponse = await adminClient.PostAsync($"/api/content-items/{itemId}/disable", content: null);
        Assert.Equal(HttpStatusCode.NoContent, disableResponse.StatusCode);

        var enableResponse = await adminClient.PostAsync($"/api/content-items/{itemId}/enable", content: null);
        Assert.Equal(HttpStatusCode.NoContent, enableResponse.StatusCode);

        var consumerClient = CreateAuthenticatedClient("consumerapi1", "5233c2da-c875-4920-a1a7-fca8c44902c4");
        var consumerListResponse = await consumerClient.GetAsync("/api/content-items");
        consumerListResponse.EnsureSuccessStatusCode();
        var consumerItems = await consumerListResponse.Content.ReadFromJsonAsync<List<CmsContentItemResponse>>();

        Assert.NotNull(consumerItems);
        Assert.Contains(consumerItems, item => item.Id == itemId);
    }

    [Fact]
    public async Task Disable_MissingItem_ReturnsNotFound()
    {
        var adminClient = CreateAuthenticatedClient("adminviewer1", "3ca5e864-6bcf-4cd0-a2f1-2f65dc5fbcab");

        var response = await adminClient.PostAsync($"/api/content-items/{Guid.NewGuid():N}/disable", content: null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private HttpClient CreateAuthenticatedClient(string username, string password)
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Basic",
            Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}")));

        return client;
    }

    private async Task PublishAsync(string id, string title)
    {
        var cmsClient = CreateAuthenticatedClient("cmsingest01", "1d9d2745-37d8-4836-84ec-b45f57c2a95d");

        var response = await cmsClient.PostAsJsonAsync("/cms/events", new[]
        {
            new
            {
                type = "publish",
                id,
                payload = new { title },
                version = 1,
                timestamp = "2026-01-01T00:00:00Z"
            }
        });

        response.EnsureSuccessStatusCode();
    }

    private async Task UnpublishAsync(string id, string title)
    {
        var cmsClient = CreateAuthenticatedClient("cmsingest01", "1d9d2745-37d8-4836-84ec-b45f57c2a95d");

        var response = await cmsClient.PostAsJsonAsync("/cms/events", new[]
        {
            new
            {
                type = "unpublish",
                id,
                payload = new { title },
                version = 1,
                timestamp = "2026-01-01T00:00:00Z"
            }
        });

        response.EnsureSuccessStatusCode();
    }
}
