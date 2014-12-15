
namespace Helpers
{
    public interface IAppSettings
    {
        long MaximumCacheSize();
        string CentralServerUsername();
        string CentralServerPassword();
        string AppDataFolder();
        string SearchDatabaseVersion();
        string ServerName();
        string ServerIP();
        string ClientName();
        bool DownloadEverythingThroughServer();
    }
}
