using LateralGroup.Application.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Json;

namespace LateralGroup.API.Tests
{
    public class CmsEventsControllerTests
    {

        WebApplicationFactory<Program> _factory;

        public CmsEventsControllerTests()
        {
            _factory = new WebApplicationFactory<Program>();
        }

        [Fact]
        public async Task Post_WithoutAuthenication_ReturnsUnauthorized()
        {
            var client = _factory.CreateClient();
            var response = await client.PostAsJsonAsync("/cms/events", new[]
            {
                new
                {
                    type = "delete",
                    id = "X",
                    timestamp = "2026-01-01T00:00:00Z"
                }
            });
            Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);

        }

        [Fact]
        public async Task Post_WithValidCmsCredentials_ReturnsOk()
        {
            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri("https://localhost")
            });

            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Basic",
                Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("cmsingest01:1d9d2745-37d8-4836-84ec-b45f57c2a95d")));
            var response = await client.PostAsJsonAsync("/cms/events", new[]
            {
                new
                {
                    type = "delete",
                    id = "X",
                    timestamp = "2026-01-01T00:00:00Z"
                }
            });
            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Post_WithValidConsumerCredentials_IsRejected()
        {
            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri("https://localhost")
            });
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Basic",
                Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("consumerapi1:5233c2da-c875-4920-a1a7-fca8c44902c4")));
            var response = await client.PostAsJsonAsync("/cms/events", new[]
            {
                new
                {
                    type = "delete",
                    id = "X",
                    timestamp = "2026-01-01T00:00:00Z"
                }
            });
            Assert.Equal(System.Net.HttpStatusCode.Forbidden, response.StatusCode);

        }

        [Fact]
        public async Task Post_WithValidPublishEvent_ReturnsProcessedBatchResult()
        {
            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri("https://localhost")
            });
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Basic",
                Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("cmsingest01:1d9d2745-37d8-4836-84ec-b45f57c2a95d")));
            var response = await client.PostAsJsonAsync("/cms/events", new[]
            {
                new
                {
                    type = "publish",
                    id = "X",
                    payload = new { title = "Hello" },
                    version = 1,
                    timestamp = "2026-01-01T00:00:00Z"
                }
            });

            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);


            var result = await response.Content.ReadFromJsonAsync<BatchProcessResult>();

            Assert.NotNull(result);
            Assert.Equal(1, result.Received);
            Assert.Equal(1, result.Processed);
            Assert.Equal(0, result.Ignored);
            Assert.Equal(0, result.Failed);
        }

        [Fact]
        public async Task Post_WithPublishMissingVersion_ReturnsFailedBatchResult()
        {
            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri("https://localhost")
            });
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Basic",
                Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("cmsingest01:1d9d2745-37d8-4836-84ec-b45f57c2a95d")));
            var response = await client.PostAsJsonAsync("/cms/events", new[]
            {
                new
                {
                    type = "publish",
                    id = "X",
                    payload = new { title = "Hello" },
                    timestamp = "2026-01-01T00:00:00Z"
                }
            });

            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);

            var result = await response.Content.ReadFromJsonAsync<BatchProcessResult>();

            Assert.NotNull(result);
            Assert.Equal(1, result.Received);
            Assert.Equal(0, result.Processed);
            Assert.Equal(0, result.Ignored);
            Assert.Equal(1, result.Failed);
        }

        [Fact]
        public async Task Post_InvalidCMSCreds_ReturnsUnauthorized()
        {
            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri("https://localhost")
            });

            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Basic",
                Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("invalid:credentials")));

            var response = await client.PostAsJsonAsync("/cms/events", new[]
            {
                new
                {
                    type = "publish",
                    id = "X",
                    payload = new { title = "Hello" },
                    version = 1,
                    timestamp = "2026-01-01T00:00:00Z"
                }
            });

            Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
        }


        [Fact]
        public async Task Post_MissingAuthHeader_ReturnsUnauthorized()
        {
            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri("https://localhost")
            });
            var response = await client.PostAsJsonAsync("/cms/events", new[]
            {
                new
                {
                    type = "publish",
                    id = "X",
                    payload = new { title = "Hello" },
                    version = 1,
                    timestamp = "2026-01-01T00:00:00Z"
                }
            });
            Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);

        }


        [Fact]
        public async Task Post_StalePublishEvent_ReturnsIgnoredBatchResult()
        {
            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri("https://localhost")
            });
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Basic",
                Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("cmsingest01:1d9d2745-37d8-4836-84ec-b45f57c2a95d")));

            var initialResponse = await client.PostAsJsonAsync("/cms/events", new[]
            {
                new
                {
                    type = "publish",
                    id = "X",
                    payload = new { title = "Hello" },
                    version = 2,
                    timestamp = "2026-01-01T00:00:00Z"
                }
            });

            Assert.Equal(System.Net.HttpStatusCode.OK, initialResponse.StatusCode);

            var response = await client.PostAsJsonAsync("/cms/events", new[]
            {
                new
                {
                    type = "publish",
                    id = "X",
                    payload = new { title = "Old title" },
                    version = 1,
                    timestamp = "2000-01-01T00:00:00Z"
                }
            });
            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<BatchProcessResult>();
            Assert.NotNull(result);
            Assert.Equal(1, result.Received);
            Assert.Equal(0, result.Processed);
            Assert.Equal(1, result.Ignored);
            Assert.Equal(0, result.Failed);
        }


        [Fact]
        public async Task Post_UnpublishEvent_ReturnsProcessedBatchResult()
        {
            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri("https://localhost")
            });
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Basic",
                Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("cmsingest01:1d9d2745-37d8-4836-84ec-b45f57c2a95d")));
            var response = await client.PostAsJsonAsync("/cms/events", new[]
            {
                new
                {
                    type = "unpublish",
                    id = "X",
                    payload = new { title = "Hello" },
                    version = 2,
                    timestamp = "2026-01-01T00:00:00Z"
                }
            });
            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<BatchProcessResult>();
            Assert.NotNull(result);
            Assert.Equal(1, result.Received);
            Assert.Equal(1, result.Processed);
            Assert.Equal(0, result.Ignored);
            Assert.Equal(0, result.Failed);

        }


        [Fact]
        public async Task Post_DeleteEvent_ReturnsProcessedBatchResult()
        {
            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri("https://localhost")
            });
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Basic",
                Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("cmsingest01:1d9d2745-37d8-4836-84ec-b45f57c2a95d")));
            var response = await client.PostAsJsonAsync("/cms/events", new[]
            {
                new
                {
                    type = "delete",
                    id = "X",
                    version = 3,
                    timestamp = "2026-01-01T00:00:00Z"
                }
            });
            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<BatchProcessResult>();
            Assert.NotNull(result);
            Assert.Equal(1, result.Received);
            Assert.Equal(1, result.Processed);
            Assert.Equal(0, result.Ignored);
            Assert.Equal(0, result.Failed);
        }
    }
}
