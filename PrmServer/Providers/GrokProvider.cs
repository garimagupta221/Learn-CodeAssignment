using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace PrmServer.Providers
{
    public class GrokProvider : IAiProvider
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private const string ApiUrl = "https://api.x.ai/v1/chat/completions";

        public string ProviderName => "Grok";

        public GrokProvider(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<string> GenerateContentAsync(string prompt, string apiKey)
        {
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            var requestBody = new
            {
                model = "grok-beta",
                messages = new[]
                {
                    new { role = "user", content = prompt }
                }
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(ApiUrl, content);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseJson);

            return doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString() ?? string.Empty;
        }
    }
}
