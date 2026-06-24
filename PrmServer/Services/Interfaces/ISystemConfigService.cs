namespace PrmServer.Services.Interfaces
{
    public interface ISystemConfigService
    {
        string Get(string key);
        void Set(string key, string value);
        Dictionary<string, string> GetAll();
    }
}
