using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using ClientAndServerShared;
using SharedProxy.Proxy;

namespace SharedProxy.Cache
{
    public class CacheFile
    {
        public static string CacheFilePathAsOnePart(string absoluteUri)
        {
            ProxyEntry entry = Controller.ProxyInstance.CreateCacheEntry(absoluteUri);
            int hash = Helpers.Proxy.ProxyCache.GetUriHash(absoluteUri);
            if (entry.UpdateFromDatabase())
            {
                if (entry.NumFileParts <= 1)
                    return Controller.GetCacheFilePath(hash, 0);

                string tempFilePath = Path.GetTempFileName();
                try
                {
                    using (var stream = File.OpenWrite(tempFilePath))
                    {
                        byte[] buffer = new byte[Helpers.Proxy.ProxyServer.BUFFER_SIZE];

                        for (int i = 0; i < entry.NumFileParts; ++i)
                        {
                            using (var readStream = File.OpenRead(Controller.GetCacheFilePath(hash, i)))
                            {
                                int read;
                                while ((read = readStream.Read(buffer, 0, buffer.Length)) > 0)
                                    stream.Write(buffer, 0, read);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogsController.WriteException("CacheFilePathAsOnePart", ex.Message);
                }
                return tempFilePath;
            }
            return "";
        }
    }
}
