
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

    }
}
