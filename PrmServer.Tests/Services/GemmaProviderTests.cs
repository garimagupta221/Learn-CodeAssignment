using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.Protected;
using PrmServer.Providers;
using PrmServer.Services.Interfaces;
using Xunit;

namespace PrmServer.Tests.Services
{
    /// <summary>
    /// Unit tests for GemmaProvider.
    /// Uses a Moq HttpMessageHandler to intercept HTTP calls — no real network required.
    /// </summary>
    public class GemmaProviderTests
    {
        private readonly Mock<ISystemConfigService> _configMock;

        public GemmaProviderTests()
        {
            _configMock = new Mock<ISystemConfigService>();
            _configMock.Setup(c => c.Get("GemmaBaseUrl")).Returns("http://localhost:11434");
            _configMock.Setup(c => c.Get("GemmaModel")).Returns(string.Empty); // use default
        }

        // ── Helpers ──────────────────────────────────────────────────────────────────

        private static IHttpClientFactory BuildHttpClientFactory(
            HttpStatusCode statusCode, string responseJson)
        {
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = statusCode,
                    Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
                });

            var client = new HttpClient(handlerMock.Object);
            var factoryMock = new Mock<IHttpClientFactory>();
            factoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(client);
            return factoryMock.Object;
        }

        private GemmaProvider BuildProvider(IHttpClientFactory factory) =>
            new GemmaProvider(factory, _configMock.Object);

        // ── Tests ─────────────────────────────────────────────────────────────────────

        [Fact]
        public void ProviderName_ShouldBeGemma()
        {
            var provider = BuildProvider(BuildHttpClientFactory(HttpStatusCode.OK, "{}"));
            Assert.Equal("Gemma", provider.ProviderName);
        }

        [Fact]
        public async Task GenerateContentAsync_ShouldReturnResponseField_OnSuccess()
        {
            var responseJson = JsonSerializer.Serialize(new
            {
                model = "gemma3:12b-it-q8_0",
                response = "Hello from Gemma!",
                done = true
            });

            var provider = BuildProvider(BuildHttpClientFactory(HttpStatusCode.OK, responseJson));
            var result = await provider.GenerateContentAsync("Hello", "test-key");

            Assert.Equal("Hello from Gemma!", result);
        }

        [Fact]
        public async Task GenerateContentAsync_ShouldUseDefaultModel_WhenGemmaModelNotConfigured()
        {
            string? sentJson = null;
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .Callback<HttpRequestMessage, CancellationToken>(async (req, _) =>
                {
                    sentJson = await req.Content!.ReadAsStringAsync();
                })
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(
                        JsonSerializer.Serialize(new { response = "ok" }),
                        Encoding.UTF8, "application/json")
                });

            var client = new HttpClient(handlerMock.Object);
            var factoryMock = new Mock<IHttpClientFactory>();
            factoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(client);

            var provider = new GemmaProvider(factoryMock.Object, _configMock.Object);
            await provider.GenerateContentAsync("Test prompt", "key");

            Assert.NotNull(sentJson);
            Assert.Contains("gemma3:12b-it-q8_0", sentJson);
        }

        [Fact]
        public async Task GenerateContentAsync_ShouldUseConfiguredModel_WhenSet()
        {
            _configMock.Setup(c => c.Get("GemmaModel")).Returns("gemma3:4b");

            string? sentJson = null;
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .Callback<HttpRequestMessage, CancellationToken>(async (req, _) =>
                {
                    sentJson = await req.Content!.ReadAsStringAsync();
                })
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(
                        JsonSerializer.Serialize(new { response = "ok" }),
                        Encoding.UTF8, "application/json")
                });

            var client = new HttpClient(handlerMock.Object);
            var factoryMock = new Mock<IHttpClientFactory>();
            factoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(client);

            var provider = new GemmaProvider(factoryMock.Object, _configMock.Object);
            await provider.GenerateContentAsync("Test", "key");

            Assert.Contains("gemma3:4b", sentJson);
        }

        [Fact]
        public async Task GenerateContentAsync_ShouldThrow_WhenHttpReturnsNonSuccess()
        {
            var provider = BuildProvider(
                BuildHttpClientFactory(HttpStatusCode.InternalServerError, "error"));

            await Assert.ThrowsAsync<HttpRequestException>(
                () => provider.GenerateContentAsync("Hello", "key"));
        }

        [Fact]
        public async Task GenerateContentAsync_ShouldThrow_WhenGemmaBaseUrlNotConfigured()
        {
            _configMock.Setup(c => c.Get("GemmaBaseUrl")).Returns(string.Empty);

            var provider = BuildProvider(
                BuildHttpClientFactory(HttpStatusCode.OK, "{}"));

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => provider.GenerateContentAsync("Hello", "key"));
        }

        [Fact]
        public async Task GenerateContentAsync_ShouldSendApikeyHeader_WhenKeyProvided()
        {
            string? apiKeyHeader = null;
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .Callback<HttpRequestMessage, CancellationToken>((req, _) =>
                {
                    if (req.Headers.TryGetValues("apikey", out var vals))
                        apiKeyHeader = System.Linq.Enumerable.FirstOrDefault(vals);
                })
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(
                        JsonSerializer.Serialize(new { response = "ok" }),
                        Encoding.UTF8, "application/json")
                });

            var client = new HttpClient(handlerMock.Object);
            var factoryMock = new Mock<IHttpClientFactory>();
            factoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(client);

            var provider = new GemmaProvider(factoryMock.Object, _configMock.Object);
            await provider.GenerateContentAsync("Hi", "my-secret-key");

            Assert.Equal("my-secret-key", apiKeyHeader);
        }

        [Fact]
        public async Task GenerateContentAsync_ShouldSendStreamFalse()
        {
            string? sentJson = null;
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .Callback<HttpRequestMessage, CancellationToken>(async (req, _) =>
                {
                    sentJson = await req.Content!.ReadAsStringAsync();
                })
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(
                        JsonSerializer.Serialize(new { response = "ok" }),
                        Encoding.UTF8, "application/json")
                });

            var client = new HttpClient(handlerMock.Object);
            var factoryMock = new Mock<IHttpClientFactory>();
            factoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(client);

            var provider = new GemmaProvider(factoryMock.Object, _configMock.Object);
            await provider.GenerateContentAsync("Prompt", "key");

            Assert.Contains("\"stream\":false", sentJson);
        }
    }
}
