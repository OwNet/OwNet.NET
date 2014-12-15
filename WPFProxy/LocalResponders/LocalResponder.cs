using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ClientAndServerShared;
using HtmlAgilityPack;
using WPFProxy.Proxy;

namespace WPFProxy.LocalResponders
{

    class RouteTable
    {
        public delegate ResponseResult ResponseAction(string method, string relativeUrl, RequestParameters parameters);
        private Dictionary<string, Response> GetRoutes;
        private Dictionary<string, Response> PostRoutes;

        class Response
        {
            public bool IsAction { get { return true; } }
            public ResponseAction Action { get; private set;  }
            public Response(ResponseAction action)
            {
                Action = action;
            }
        }

        public RouteTable()
        {
            GetRoutes = new Dictionary<string, Response>();
            PostRoutes = new Dictionary<string, Response>();
        }

        private bool Register(string method, string relativeUrl, Response response)
        {
            Dictionary<string, Response> routes = null;
            if (method == "GET")
            {
                routes = GetRoutes;
            }
            else if (method == "POST")
            {
                routes = PostRoutes;
            }
            else
            {
                GetRoutes.Add(relativeUrl, response);
                PostRoutes.Add(relativeUrl, response);
                return true;
            }
            routes.Add(relativeUrl, response);
            return true;
        }

        public RouteTable RegisterRoute(string method, string relativeUrl, ResponseAction responseMethod)
        {
            Register(method, relativeUrl, new Response(responseMethod));
            return this;
        }

        private string RegexSearch(List<string> keys, string relativeUrl)
        {
            System.Text.RegularExpressions.Regex regexp;
            foreach (string key in keys)
            {
                regexp = new System.Text.RegularExpressions.Regex("^"+key+"$");
                if (regexp.IsMatch(relativeUrl))
                {
                    return key;
                }
            }
            return null;
        }

        public ResponseAction RouteResponse(string method, string relativeUrl, bool regex = false)
        {
            Dictionary<string, Response> routes = null;
            Response route = null;
            string key = relativeUrl;

            if (method == "GET")
            {
                routes = GetRoutes;
            }
            else if (method == "POST")
            {
                routes = PostRoutes;
            }


            if (routes != null)
            {
                if (regex)
                {
                    key = RegexSearch(routes.Keys.ToList(), relativeUrl);
                }

                if (key != null && routes.ContainsKey(key))
                {
                    route = routes[key];
                }
            }

            if (route != null)
            {
                if (route.IsAction)
                {
                    return route.Action;
                }
            }

            return null;
        }

    }

    class ResponseResult
    {
        public byte[] Data { get; set; }
        public string Filename { get; set; }
        public System.Net.HttpStatusCode StatusCode { get; set; }
        public ResponseResult()
        {
            Data = null;
            Filename = ".html";
            StatusCode = System.Net.HttpStatusCode.OK;
        }

    }

    class LocalResponder
    {
        protected virtual string ThisUrl { get { return "http://"; } }

        protected RouteTable Routes = null;

        private Stream OutStream = null;

        internal enum LocalLayout
        {
            Frame
        };

        public LocalResponder()
        {
            if (Routes != null) return;
            Routes = new RouteTable();
        }

        protected virtual void InitRoutes()
        {
            Routes.RegisterRoute(null, "my.ownet/.*", new My.MyDomainResponder().Respond)
                .RegisterRoute(null, "inject.ownet/.*", new Inject.InjectDomainResponder().Respond)
                .RegisterRoute(null, "server.ownet/.*", new Server.ServerDomainResponder(OutStream).Respond)
                .RegisterRoute(null, "proxy.ownet/.*", new Proxy.ProxyDomainResponder().Respond);
        }

        protected virtual void InitContentRoutes()
        {
            Routes.RegisterRoute("GET", "js/.*", Content)
                .RegisterRoute("GET", "css/.*", Content)
                .RegisterRoute("GET", "graphics/.*", Content)
                .RegisterRoute("GET", "favicon.ico", Content);
        }

        protected RouteTable.ResponseAction GetAction(string method, string relativeUrl)
        {
            return Routes.RouteResponse(method, relativeUrl, true);
        }

        public virtual ResponseResult Respond(string method, string relativeUrl, RequestParameters parameters = null)
        {
            InitRoutes();
          
            ResponseResult result = null;
            RouteTable.ResponseAction response = null;
            relativeUrl = new System.Text.RegularExpressions.Regex("^"+this.ThisUrl).Replace(relativeUrl, "");
            if (method == "GET" || method == "POST")
            {
                response = Routes.RouteResponse(method, relativeUrl, true);
            }
            if (response != null)
            {
                result = response(method, relativeUrl, parameters);
            }
            else
            {
                result = LoadProxyLocalError404();
            }
            return result;
        }

        public void Respond(System.IO.Stream outStream, string method, string relativeUrl, RequestParameters parameters = null)
        {
            OutStream = outStream;
            ResponseResult result = null;
            result = Respond(method, relativeUrl, parameters);
            if (result == null)
            {
                result = LoadProxyLocalError404();
            }
            WriteResponse(outStream, result);
        }

        protected static string GetSubdirectory(ref string relativeUrl, bool remove = false)
        {
            string dir = "";
            if (relativeUrl.IndexOf("/") != -1)
            {
                dir = relativeUrl.Substring(0, relativeUrl.IndexOf("/") - 1);
                if (remove)
                {
                    relativeUrl = relativeUrl.Substring(relativeUrl.IndexOf("/") + 1);
                }
            }
            return dir;
        }

        //protected string GetRoutedUrl(string relativeUrl)
        //{
        //    return BaseUrl + relativeUrl;
        //}
        protected static ResponseResult LoadProxyLocalError404()
        {
            FileStream fs = null;
            byte[] returnBytes = null;

            try
            {
                fs = File.OpenRead(Controller.GetAppResourcePath("Html/404.html"));
                returnBytes = new byte[fs.Length];
                fs.Read(returnBytes, 0, Convert.ToInt32(fs.Length));
            }
            catch (Exception e)
            {
                LogsController.WriteException("LoadError404Page()", e.Message);
                returnBytes = System.Text.Encoding.UTF8.GetBytes("Error 404");
            }
            finally
            {
                if (fs != null)
                {
                    if (fs != null)
                    {
                        fs.Close();
                        fs.Dispose();
                    }
                }
            }
            return new ResponseResult() { Data = returnBytes, StatusCode = System.Net.HttpStatusCode.NotFound }; // TODO 
        }

        protected static ResponseResult LoadProxyLocalFile(string relativeUrl)
        {
            FileStream fs = null;
            byte[] returnBytes = null;

            try
            {
                fs = File.OpenRead(Controller.GetAppResourcePath("Html/" + relativeUrl));
                returnBytes = new byte[fs.Length];
                fs.Read(returnBytes, 0, Convert.ToInt32(fs.Length));
            }
            catch (Exception e)
            {
                LogsController.WriteException("Respond()", e.Message);
                return LoadProxyLocalError404();
            }
            finally
            {
                if (fs != null)
                {
                    fs.Close();
                    fs.Dispose();
                }
            }
            return new ResponseResult() { Data = returnBytes, Filename = relativeUrl }; // TODO RelativeUrl
        }

        protected static void WriteResponse(System.IO.Stream outStream, ResponseResult result)
        {// TODO byte[] -> ResponseResult
            if (result == null || result.Data == null) return;
            StreamWriter myResponseWriter = new StreamWriter(outStream);
            ProxyServer.WriteResponseStatus(System.Net.HttpStatusCode.OK, "OK", myResponseWriter);

            List<Tuple<string, string>> headers = new List<Tuple<string, string>>();
            headers.Add(new Tuple<string, string>("Content-Length", Convert.ToString(result.Data.Length)));
            headers.Add(new Tuple<string, string>("Connection", "close"));

            int argsIndex = result.Filename.IndexOf('?');        // TODO treba to tu vobec?
            if (argsIndex <= 0)
                argsIndex = result.Filename.Length;

            headers.Add(new Tuple<string, string>("Content-Type", ProxyServer.GetContentType(result.Filename.Substring(0, argsIndex))));

            ProxyServer.WriteResponseHeaders(myResponseWriter, headers);

            myResponseWriter.Flush();
            outStream.Write(result.Data, 0, result.Data.Length);
            outStream.Flush();
        }

        protected static ResponseResult SimpleOKResponse(string content = null, string url = null, string type = null)
        {
            ResponseResult result = new ResponseResult();
            result.Filename = (url ?? "response") + "." + (type ?? "json");
            result.Data = System.Text.Encoding.UTF8.GetBytes(content ?? "{ \"status\" : \"OK\" }");
            return result;
        }
        public static string UrlDecode(string text)
        {
            text = text.Replace("+", " ");
            return System.Uri.UnescapeDataString(text);
        }
        public static string ConvertListToJsonArray(List<string> list, bool encode = true, bool quote = true)
        {
            StringBuilder sb = new StringBuilder("[");
            bool first = true;

            foreach (string item in list)
            {
                if (first == false)
                    sb.Append(",");
                else
                    first = false;
                if (quote) sb.Append("\"");
                if (encode) sb.Append(System.Web.HttpUtility.UrlEncode(item));
                else sb.Append(item);
                if (quote) sb.Append("\"");
            }
            sb.Append("]");
            return sb.ToString();
        }

        protected static bool IncludeNoResultsMessage(HtmlDocument htmlDoc, string message = null)
        {
            string msg;
            if (message == null)
            {
                msg = "<div class=\"message_status\"><div class=\"message_info\">There are no results.</div></div>";
            }
            else
            {
                msg = "<div class=\"message_status\"><div class=\"message_info\">" + message + "</div></div>";
            }
            htmlDoc.LoadHtml(msg);
            return true;
        }

        protected static void IncludePaging(HtmlDocument htmlDoc, int totalPages, int currentPage, string div, string uri, string args = "")
        {
            HtmlNode root = htmlDoc.DocumentNode.SelectSingleNode("//div[@id=\"include\"]");
            if (root == null) return;
            HtmlNode resultsNode = htmlDoc.CreateElement("div");
            resultsNode.SetAttributeValue("class", "paging");
            root.AppendChild(resultsNode);
            if (totalPages > 1)
            {
                int bigPageBegin = currentPage - (currentPage % 10);
                if ((currentPage % 10) == 0 && bigPageBegin > 0) bigPageBegin -= 10;
                int bigPageEnd = bigPageBegin + 10;
                if (bigPageEnd > totalPages) bigPageEnd -= (bigPageEnd - totalPages);
                //                int bigPageEnd = (totalPages - bigPage) % 11; //(totalPages - bigPage);
                //               bigPageEnd = bigPage + bigPageEnd;
                StringBuilder sb = new StringBuilder();
                string link = String.Format(" <a href=\"javascript:void(0);\" onclick=\"TabLoad('{0}', '{1}?page={{0}}&{2}');return false;\">{{1}}</a> ", div, uri, args);
                string currLink = String.Format(" <span class=\"act_page\">{0}</span> |", currentPage);
                if (currentPage > 1)
                {
                    sb.Append(String.Format(link, currentPage - 1, "Previous"));
                    if (bigPageBegin >= 10) sb.Append(String.Format(link, bigPageBegin, "&hellip;"));
                }

                int i;
                for (i = bigPageBegin + 1; i < currentPage; ++i)
                {
                    sb.Append(String.Format(link, i, i));
                    sb.Append("|");
                }
                sb.Append(currLink);
                for (i = currentPage + 1; i <= bigPageEnd; ++i)
                {
                    sb.Append(String.Format(link, i, i));
                    sb.Append("|");
                }
                sb.Remove(sb.Length - 1, 1);

                if (currentPage < totalPages)
                {
                    if (bigPageEnd < totalPages)
                    {
                        sb.Append(String.Format(link, bigPageEnd + 1, "&hellip;"));
                    }
                    sb.Append(String.Format(link, currentPage + 1, "Next"));
                }
                resultsNode.InnerHtml = sb.ToString();
            }
        }

        protected static ResponseResult ReplyToRedirect(string url, string parameters = null)
        {
            HtmlDocument htmlDoc = new HtmlAgilityPack.HtmlDocument();
            htmlDoc.LoadHtml(String.Format("<html><head><meta http-equiv=\"refresh\" content=\"0; url={0}{1}\"></head></html>", url, (parameters != null ? "?" + parameters : "")));
            return new ResponseResult() { Data = Encoding.GetEncoding("utf-8").GetBytes(htmlDoc.DocumentNode.OuterHtml) };
        }

        protected static ResponseResult ReplyToRedirect(RouteTable.ResponseAction action, string method, string relativeUrl, RequestParameters parameters)
        {   // TODO
            return action(method, relativeUrl, parameters);
        }

        protected static ResponseResult Content(string method, string relativeUrl, RequestParameters parameters)
        {
            return LoadProxyLocalFile(relativeUrl);
        }

    }
}
