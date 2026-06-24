namespace PrmServer.Providers
{
    public interface IAiProvider
    {
        string ProviderName { get; }
        Task<string> GenerateContentAsync(string prompt, string apiKey);
    }
}
