using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System.Net;
using System.Threading;
using Microsoft.Win32;

namespace Helpers.Proxy
{
    public class ProxyServer
    {
        private static List<string> _nonHtmlFileExtensions = new List<string>()
        {
            ".css", "text/css", ".csv", ".js", ".json", ".xml", ".avi", ".bmp", ".png", ".gif", ".jpg", ".jpeg"
        };

        public static readonly int BUFFER_SIZE = 8192;
        public static readonly char[] semiSplit = new char[] { ';' };
        public static readonly char[] equalSplit = new char[] { '=' };
        public static readonly String[] colonSpaceSplit = new string[] { ": " };
        public static readonly char[] commaSplit = new char[] { ',' };
        public static readonly Regex cookieSplitRegEx = new Regex(@",(?! )");
        
        public static List<Tuple<String, String>> ProcessResponse(HttpWebResponse response)
        {
            String value = null;
            String header = null;
            List<Tuple<String, String>> returnHeaders = new List<Tuple<String, String>>();
            foreach (String s in response.Headers.Keys)
            {
                if (s.ToLower() == "set-cookie")
                {
                    header = s;
                    value = response.Headers[s];
                }
                else
                    returnHeaders.Add(new Tuple<String, String>(s, response.Headers[s]));
            }

            if (!String.IsNullOrWhiteSpace(value))
            {
                response.Headers.Remove(header);
                String[] cookies = cookieSplitRegEx.Split(value);
                foreach (String cookie in cookies)
                    returnHeaders.Add(new Tuple<String, String>("Set-Cookie", cookie));

            }
            returnHeaders.Add(new Tuple<String, String>("X-Proxied-By", "OwNet"));
            return returnHeaders;
        }

        public static string GetContentType(string fileName)
        {
            string contentType = "";
            string extension = "";

            try
            {
                extension = Path.GetExtension(fileName);

                switch (extension)
                {
                    case ".htm":
                    case ".html":
                        contentType = "text/html";
                        break;

                    case ".css":
                        contentType = "text/css";
                        break;

                    case ".csv":
                        contentType = "text/csv";
                        break;

                    case ".js":
                        contentType = "application/javascript";
                        break;

                    case ".json":
                        contentType = "application/json";
                        break;

                    case ".xml":
                        contentType = "text/xml";
                        break;

                    case ".avi":
                        contentType = "video/x-msvideo";
                        break;

                    case ".bmp":
                        contentType = "image/bmp";
                        break;

                    case ".png":
                        contentType = "image/png";
                        break;

                    case ".gif":
                        contentType = "image/gif";
                        break;

                    case ".jpg":
                    case ".jpeg":
                        contentType = "image/jpeg";
                        break;

                    default:
                        break;
                }

                if (contentType == "")
                {
                    RegistryKey key = Registry.ClassesRoot.OpenSubKey(extension);
                    object obj = key.GetValue("Content Type");
                    if (obj != null)
                        contentType = key.GetValue("Content Type").ToString();
                }
            }
            catch (Exception ex)
            {
                Messages.WriteException("Get content type", ex.Message);
            }
            finally
            {
                if (contentType == "")
                    contentType = "application/octet-stream";
            }

            return contentType;
        }

        public static bool HasNonHtmlExtensions(string fileName)
        {
            try
            {
                return _nonHtmlFileExtensions.Contains(Path.GetExtension(fileName));
            }
            catch (Exception ex)
            {
                Messages.WriteException("HasNonHtmlExtensions", ex.Message);
            }
            return false;
        }
    }
}
