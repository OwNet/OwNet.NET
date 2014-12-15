
namespace CentralServerShared
{
    public class CacheEntry : Helpers.Proxy.ProxyEntry
    {
        public CacheEntry(string absoluteUri)
        {
            AbsoluteUri = absoluteUri;
            RequestHeaders = Helpers.Proxy.ProxyHelper.CreateArtificialRequestHeaders();
            Method = "GET";

            Init();
        }
    }
}
