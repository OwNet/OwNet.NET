using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace CentralServerShared
{
    public class HttpGetRetriever
    {
        public static List<Regex> _blacklist = new List<Regex>();

        static HttpGetRetriever()
        {
            _blacklist.Add(new Regex(@"^http://pagead2\.googlesynsdication\.com/.*", RegexOptions.Compiled));
        }

        public static bool MatchesBlacklist(string url)
        {
            foreach (Regex regex in _blacklist)
            {
                if (regex.IsMatch(url))
                    return true;
            }
            return false;
        }

        public static string DownloadNewHash(string url, out long length)
        {
            Stream readStream = Helpers.Proxy.HttpGetRetriever.RetrieveFromWeb(new CacheEntry(url));
            length = 0;

            if (readStream == null)
                return "";

            try
            {
                MemoryStream memoryStream = new MemoryStream();
                byte[] buffer = new byte[Helpers.Proxy.ProxyServer.BUFFER_SIZE];

                int bytesRead;
                while ((bytesRead = readStream.Read(buffer, 0, buffer.Length)) > 0)
                    memoryStream.Write(buffer, 0, bytesRead);

                byte[] bytes = memoryStream.ToArray();
                length = bytes.LongLength;
                return Helpers.Cache.GetMD5HashFromFile(bytes);
            }
            catch (Exception ex)
            {
                Controller.WriteException("Get MD5 hash", ex.Message);
            }
            return "";
        }


        public static byte[] DownloadFile(string url)
        {
            Stream readStream = Helpers.Proxy.HttpGetRetriever.RetrieveFromWeb(new CacheEntry(url));

            if (readStream == null)
                return null;

            try
            {
                MemoryStream memoryStream = new MemoryStream();
                byte[] buffer = new byte[Helpers.Proxy.ProxyServer.BUFFER_SIZE];

                int bytesRead;
                while ((bytesRead = readStream.Read(buffer, 0, buffer.Length)) > 0)
                    memoryStream.Write(buffer, 0, bytesRead);

                byte[] bytes = memoryStream.ToArray();
                return bytes;
            }
            catch (Exception ex)
            {
                Controller.WriteException("DownloadFile", ex.Message);
            }
            return null;
        }
    }
}