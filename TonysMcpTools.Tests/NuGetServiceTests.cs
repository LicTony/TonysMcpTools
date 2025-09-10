using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace TonysMcpTools.Tests
{
    public class NuGetServiceTests
    {
        // Clase para simular el HttpMessageHandler y controlar las respuestas HTTP
        public class MockHttpMessageHandler(HttpResponseMessage response) : HttpMessageHandler
        {
            private readonly HttpResponseMessage _response = response;

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return Task.FromResult(_response);
            }
        }

        [Fact]
        public async Task GetNugetInfoAsync_ShouldReturnPackageInfo_WhenApiSucceeds()
        {
            // Arrange
            var packageName = "newtonsoft.json";
            var fakeJsonResponse = """{"Versions": ["13.0.1", "13.0.2"]}""";
            
            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(fakeJsonResponse)
            };

            var mockHandler = new MockHttpMessageHandler(responseMessage);
            var httpClient = new HttpClient(mockHandler);
            
            var nugetService = new NuGetService(httpClient);

            // Act
            var result = await nugetService.GetNugetInfoAsync(packageName);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Versions.Length);
            Assert.Equal("13.0.1", result.Versions[0]);
            Assert.Equal("13.0.2", result.Versions[1]);
        }

        [Fact]
        public async Task GetNugetInfoAsync_ShouldThrowException_WhenApiFails()
        {
            // Arrange
            var packageName = "unexisting.package";
            
            var responseMessage = new HttpResponseMessage(HttpStatusCode.NotFound);

            var mockHandler = new MockHttpMessageHandler(responseMessage);
            var httpClient = new HttpClient(mockHandler);

            var nugetService = new NuGetService(httpClient);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => nugetService.GetNugetInfoAsync(packageName));
            Assert.Contains($"Error al consultar NuGet para {packageName}", exception.Message);
        }
    }
}
