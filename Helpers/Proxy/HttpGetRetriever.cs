using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Net.Security;

namespace Helpers.Proxy
{
    public class HttpGetRetriever
    {
        public static Stream RetrieveFromWeb(ProxyEntry entry)
        {
            HttpWebRequest webReq = entry.WebRequest;
            HttpWebResponse response = null;
            Stream responseStream = null;
            //MemoryStream cacheStream = null;

            try
            {
                response = (HttpWebResponse)webReq.GetResponse();
            }
            catch (WebException webEx)
            {
                Messages.WriteException("respond()", webEx.Message);
                response = webEx.Response as HttpWebResponse;

                if (response == null && OfflineCheck.IsOffline)
                {
                    entry.IsOffline = true;
                    return null;
                }
            }

            if (response != null)
            {
                List<Tuple<String, String>> responseHeaders = ProxyServer.ProcessResponse(response);

                try
                {
                    responseStream = response.GetResponseStream();

                    entry.Update(response);

                    entry.ShouldSave = true;

                    OfflineCheck.IsOffline = false;
                }
                catch (Exception ex)
                {
                    Messages.WriteException("respond() [second]", ex.Message);
                }
            }

            return responseStream;
        }

        public static void WritePostDataToWebRequest(char[] postBuffer, HttpWebRequest webReq)
        {
            StreamWriter sw = null;
            if (webReq != null)
                sw = new StreamWriter(webReq.GetRequestStream());

            sw.Write(postBuffer, 0, postBuffer.Length);

            if (sw != null)
                sw.Close();
        }
    }
}
