using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading;
using System.IO;
using System.Text.RegularExpressions;

namespace Helpers.Proxy
{
    public class ProxyCache
    {
        private static Regex _youtubePlaybackRegex = new Regex(@"^http://.*c\.youtube\.com/videoplayback.*", RegexOptions.Compiled);
        private static Regex _youtubeGetInfoRegex = new Regex(@"^http://www\.youtube\.com/get_video_info.*", RegexOptions.Compiled);

        public static int GetUriHash(string url)
        {
            if (_youtubePlaybackRegex.IsMatch(url) || _youtubeGetInfoRegex.IsMatch(url))
            {
                string[] baseAndArgs = url.Split('?');
                string[] args = baseAndArgs.Last().Split('&');
                Array.Sort<string>(args);
                url = baseAndArgs.First() + "?" + string.Join("&", args);
            }
            return url.TrimEnd(new char[] { '/' }).GetHashCode();
        }

        public static Boolean CanCache(WebHeaderCollection headers, ref DateTime? expires)
        {
            foreach (String s in headers.AllKeys)
            {
                String value = headers[s].ToLower();
                switch (s.ToLower())
                {
                    case "cache-control":
                        if (value.Contains("max-age"))
                        {
                            int seconds;
                            if (int.TryParse(value, out seconds))
                            {
                                if (seconds == 0)
                                    return false;
                                DateTime d = DateTime.Now.AddSeconds(seconds);
                                if (!expires.HasValue || expires.Value < d)
                                    expires = d;

                            }
                        }

                        if (value.Contains("private") || value.Contains("no-cache"))
                            return false;
                        else if (value.Contains("public") || value.Contains("no-store"))
                            return true;

                        break;

                    case "pragma":
                        if (value == "no-cache")
                            return false;

                        break;

                    case "expires":
                        DateTime dExpire;
                        if (DateTime.TryParse(value, out dExpire))
                        {
                            if (!expires.HasValue || expires.Value < dExpire)
                                expires = dExpire;
                        }
                        break;
                }
            }
            return true;
        }
    }
}
