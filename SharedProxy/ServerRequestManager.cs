using System;
using System.Net;
using System.Runtime.Serialization;
using Microsoft.Http;

namespace SharedProxy
{
    public class ServerRequestManager
    {
        static ServerRequestManager()
        {
            ServicePointManager.DefaultConnectionLimit = 10;        // 10 maximum connections per ServicePoint, default is 2
            ServicePointManager.MaxServicePointIdleTime = 5000;     // idle 5 seconds after completed request, then disposed
            ServicePointManager.MaxServicePoints = 0;               // no limit on number of ServicePoints
        }


        private static HttpClient CreateClient(string ip = "") {
            if (ip == "::1")
                ip = "localhost";
            HttpClient ret = new HttpClient(Controller.ServiceBaseUrl(ip));
            ret.TransportSettings.ConnectionTimeout = new TimeSpan(0, 1, 0);   // 10 seconds timeout for Client
            return ret;
        }

        public static T Post<P, T>(string url, P data, string ip = "")
        {
            T ret = default(T);
            using (HttpClient http = CreateClient(ip))
            {
                HttpContent content = HttpContentExtensions.CreateDataContract<P>(data);
                using (HttpResponseMessage resp = http.Post(url, content))
                {
                    resp.EnsureStatusIsSuccessful();
                    ret = ReadContentAsDataContract<T>(resp.Content);
                }
            }
            return ret;
        }
        public static void PostWithoutResponse<P>(string url, P data)
        {
            using (HttpClient http = CreateClient())
            {
                HttpContent content = HttpContentExtensions.CreateDataContract<P>(data);
                using (HttpResponseMessage resp = http.Post(url, content))
                {
                    resp.EnsureStatusIsSuccessful();
                }
            }
        }

        public static T Put<P, T>(string url, P data)
        {
            T ret = default(T);
            using (HttpClient http = CreateClient())
            {
                HttpContent content = HttpContentExtensions.CreateDataContract<P>(data);
                using (HttpResponseMessage resp = http.Put(url, content))
                {
                    resp.EnsureStatusIsSuccessful();
                    ret = ReadContentAsDataContract<T>(resp.Content);
                }
            }
            return ret;
        }

        private static T ReadContentAsDataContract<T>(HttpContent content)
        {
            return content.ReadAsDataContract<T>();
        }

        public static T Get<T>(Uri uri, HttpQueryString vars = null, string ip = "")
        {
            T ret = default(T);
            using (HttpClient http = CreateClient(ip))
            {
                if (vars == null)
                {
                    using (HttpResponseMessage resp = http.Get(uri))
                    {
                        resp.EnsureStatusIsSuccessful();
                        ret = ReadContentAsDataContract<T>(resp.Content);
                    }
                }
                else
                {
                    using (HttpResponseMessage resp = http.Get(uri, vars))
                    {
                        resp.EnsureStatusIsSuccessful();
                        ret = ReadContentAsDataContract<T>(resp.Content);
                    }
                }

            }
            return ret;
        }

        public static T Delete<T>(Uri uri)
        {
            T ret = default(T);
            using (HttpClient http = CreateClient())
            {
                using (HttpResponseMessage resp = http.Delete(uri))
                {
                    resp.EnsureStatusIsSuccessful();
                    ret = ReadContentAsDataContract<T>(resp.Content);
                }
            }
            return ret;
        }
        public static void DeleteWithoutResponse(Uri uri)
        {
            using (HttpClient http = CreateClient())
            {
                using (HttpResponseMessage resp = http.Delete(uri))
                {
                    resp.EnsureStatusIsSuccessful();
                }
            }
        }
    }
}
