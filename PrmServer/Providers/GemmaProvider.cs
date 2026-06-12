using System.Text;
using System.Text.Json;
using PrmServer.Services.Interfaces;

namespace PrmServer.Providers
{
    /// <summary>
    /// AI provider adapter for a self-hosted Ollama endpoint running the Gemma model.
    /// Request format:  POST {GemmaBaseUrl}/api/generate
    ///                  Header: apikey: {key}
    ///                  Body:   { "model": "gemma3:12b-it-q8_0", "prompt": "...", "stream": false }
    /// Response field:  .response
    ///
    /// GemmaBaseUrl is read at call-time from ISystemConfigService so it can be changed
    /// by an Admin without restarting the server (Open/Closed Principle — the IAiProvider
    /// contract stays stable while runtime behaviour is configurable).
    /// </summary>
    public class GemmaProvider : IAiProvider
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ISystemConfigService _config;

        private const string DefaultModel = "gemma3:12b-it-q8_0";

        public string ProviderName => "Gemma";

        public GemmaProvider(IHttpClientFactory httpClientFactory, ISystemConfigService config)
        {
            _httpClientFactory = httpClientFactory;
            _config = config;
        }

        public async Task<string> GenerateContentAsync(string prompt, string apiKey)
        {
            var baseUrl = _config.Get("GemmaBaseUrl");
            if (string.IsNullOrWhiteSpace(baseUrl))
                throw new InvalidOperationException(
                    "GemmaBaseUrl is not configured in System Configuration. " +
                    "Set it via POST /api/system-config.");

            var model = _config.Get("GemmaModel");
            if (string.IsNullOrWhiteSpace(model))
                model = DefaultModel;

            var endpoint = $"{baseUrl.TrimEnd('/')}/api/generate";

            var client = _httpClientFactory.CreateClient();

            // The Ollama endpoint uses a custom header for the API key
            if (!string.IsNullOrWhiteSpace(apiKey))
                client.DefaultRequestHeaders.TryAddWithoutValidation("apikey", apiKey);

            var requestBody = new
            {
                model,
                prompt,
                stream = false
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(endpoint, content);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseJson);

            // Ollama non-streaming response returns { "response": "..." }
            return doc.RootElement
                .GetProperty("response")
                .GetString() ?? string.Empty;
        }
    }
}
