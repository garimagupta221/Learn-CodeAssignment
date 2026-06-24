using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace PrmClient.Services
{
    public class ApiClient
    {
        private readonly HttpClient _httpClient;
        private string? _jwtToken;

        public ApiClient(string baseUrl)
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(baseUrl)
            };
        }

        public void SetToken(string token)
        {
            _jwtToken = token;
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }

        public void ClearToken()
        {
            _jwtToken = null;
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }

        public bool IsAuthenticated => !string.IsNullOrEmpty(_jwtToken);

        /// <summary>
        /// Reads the response body when the status is an error and tries to extract
        /// a "message" property from JSON, falling back to the raw body or HTTP reason.
        /// </summary>
        private static async Task EnsureSuccessAsync(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
                return;

            string body = await response.Content.ReadAsStringAsync();
            string errorMessage;

            try
            {
                using var doc = JsonDocument.Parse(body);
                if (doc.RootElement.TryGetProperty("message", out var msgProp))
                {
                    errorMessage = msgProp.GetString() ?? body;
                }
                else if (doc.RootElement.TryGetProperty("error", out var errProp))
                {
                    errorMessage = errProp.GetString() ?? body;
                }
                else
                {
                    errorMessage = body;
                }
            }
            catch
            {
                errorMessage = string.IsNullOrWhiteSpace(body)
                    ? response.ReasonPhrase ?? response.StatusCode.ToString()
                    : body;
            }

            throw new HttpRequestException(errorMessage, null, response.StatusCode);
        }

        public async Task<T?> GetAsync<T>(string endpoint)
        {
            var response = await _httpClient.GetAsync(endpoint);
            await EnsureSuccessAsync(response);
            return await response.Content.ReadFromJsonAsync<T>();
        }

        public async Task<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, TRequest body)
        {
            var response = await _httpClient.PostAsJsonAsync(endpoint, body);
            await EnsureSuccessAsync(response);
            return await response.Content.ReadFromJsonAsync<TResponse>();
        }

        public async Task PostAsync<TRequest>(string endpoint, TRequest body)
        {
            var response = await _httpClient.PostAsJsonAsync(endpoint, body);
            await EnsureSuccessAsync(response);
        }

        public async Task<TResponse?> PutAsync<TRequest, TResponse>(string endpoint, TRequest body)
        {
            var response = await _httpClient.PutAsJsonAsync(endpoint, body);
            await EnsureSuccessAsync(response);
            return await response.Content.ReadFromJsonAsync<TResponse>();
        }

        public async Task PutAsync(string endpoint)
        {
            var response = await _httpClient.PutAsync(endpoint, null);
            await EnsureSuccessAsync(response);
        }

        public async Task PutAsync<TRequest>(string endpoint, TRequest body)
        {
            var response = await _httpClient.PutAsJsonAsync(endpoint, body);
            await EnsureSuccessAsync(response);
        }

        public async Task PostAsync(string endpoint)
        {
            var response = await _httpClient.PostAsync(endpoint, null);
            await EnsureSuccessAsync(response);
        }

        public async Task DeleteAsync(string endpoint)
        {
            var response = await _httpClient.DeleteAsync(endpoint);
            await EnsureSuccessAsync(response);
        }
    }
}
