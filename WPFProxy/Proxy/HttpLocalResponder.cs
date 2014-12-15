using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Web;

namespace WPFProxy.Proxy
{ 
    class RequestParameters : List<Tuple<string, string>>
    {
        public RequestParameters(string uriParameters) : base()
        {
            if (!String.IsNullOrWhiteSpace(uriParameters))
            {
                string[] pars = uriParameters.Split("&".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                this.Capacity = pars.Length;
                foreach (string par in pars)
                {
                    string[] parsep = par.Split("=".ToCharArray(), StringSplitOptions.None);
                    this.Add(parsep[0], parsep[1]);
                }
            }
        }
        private RequestParameters() : base() { }
        private RequestParameters(int capacity) : base(capacity) { }
        public void Add(string name, string value)
        {
            base.Add(new Tuple<string, string>(name, value));
        }

        public string GetValue(string name, bool erase = false)
        {
            string val = "";
            int i;
            for (i = 0; i < this.Count; ++i)
            {
                if (this[i].Item1.Equals(name))
                {
                    val = this[i].Item2;
                    if (erase) this.RemoveAt(i);
                    break;
                }
            }
            return val;
        }

        public static bool IsNullOrEmpty(RequestParameters parameters)
        {
            return parameters == null || parameters.Count == 0;
        }
    }

    class HttpLocalResponder : HttpResponder
    {
        public static string BaseUrl = "http://my.ownet/";
        private string _subdomain;
        private string _relativeUri;
        private static Regex _urlMatchRegex = new Regex(@"http://(?:www\.)?([A-Za-z0-9\-_]*)\.ownet/([A-Za-z0-9\?=%\-\._/&\(\)\+]*)", RegexOptions.Compiled);

        public static string RegisterUrl
        {
            get { return "http://my.ownet/user/register.html"; }
        }

        public static string GetSharedFilePath(ServiceEntities.SharedFile file)
        {
            return String.Format("http://server.ownet/files/{0}/{1}", file.FileObjectId, System.Web.HttpUtility.UrlEncode(file.FileName));
        }

        public static HttpLocalResponder CreateIfMatches(string url, string method)
        {
            try
            {
                Match match = _urlMatchRegex.Match(url);
                if (match.Success == true)
                {
                    return new HttpLocalResponder(url, method, "http://"+ match.Groups[1].Value + ".ownet/", match.Groups[2].Value);
                }
            }
            catch (Exception)
            {
            }
            return null;
        }

        protected HttpLocalResponder(string remoteUri, string method, string subdomain, string relativeUrl)
            : base(remoteUri, method)
        {
            _subdomain = subdomain;
            _relativeUri = relativeUrl;//.TrimEnd(new char[] { '/' }); ;
        }

        public override void Respond(System.IO.Stream clientStream, System.IO.Stream outStream, System.IO.StreamReader clientStreamReader)
        {
            RequestParameters parameters = null;
            string uriParameters = null;

            if (Method.ToUpper() == "GET" && _relativeUri.IndexOf("?") != -1)
            {
                uriParameters = _relativeUri.Substring(_relativeUri.IndexOf("?") + 1);
                _relativeUri = System.Text.RegularExpressions.Regex.Replace(_relativeUri, "\\?.*$", "");
            }
            else if (Method.ToUpper() == "POST")
            {
                int contentLen = ProxyServer.GetContentLengthFromHeaders(ProxyServer.ReadRequestHeaders(clientStreamReader));
                char[] data = ReadPostData(contentLen, clientStreamReader);
                uriParameters = new String(data);
            }
            if (uriParameters != null)
            {
                parameters = new RequestParameters(uriParameters);
            }

            new LocalResponders.LocalResponder().Respond(outStream, Method.ToUpper(), _subdomain + _relativeUri, parameters);
        }

        public static string GetPageTitle(string uri)
        {
            if (!String.IsNullOrWhiteSpace(uri))
            {
                uri = HttpUtility.UrlDecode(uri);

                return HtmlResponder.GetWebsiteTitleAndDescription(uri).Item1;
            }
            return "Webpage";
        }

        public static void OutputOfflineError(System.IO.Stream clientStream, System.IO.Stream outStream, System.IO.StreamReader clientStreamReader, string page, string referrer = null)
        {
            HttpResponder responder = HttpLocalResponder.CreateIfMatches("http://proxy.ownet/errors/offline.html?page=" + HttpUtility.UrlEncode(page) + "&ref="+ HttpUtility.UrlEncode(referrer ?? ""), "GET");

            if (responder != null)
                responder.Respond(clientStream, outStream, clientStreamReader);
        }
    }
}
