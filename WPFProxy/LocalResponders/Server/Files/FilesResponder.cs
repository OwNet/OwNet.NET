using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using WPFProxy.Proxy;

namespace WPFProxy.LocalResponders.Server.Files
{
    class FilesResponder : ServerDomainResponder
    {
        protected override string ThisUrl { get { return "files/"; } }
        private System.IO.Stream OutStream = null;

        public FilesResponder()
            : base()
        {
        }

        public FilesResponder(System.IO.Stream outStream) : this()
        {
            OutStream = outStream;
        }

        protected override void InitRoutes()
        {
            Routes.RegisterRoute("GET", "*", Download);
            
        }

        // GET: server.ownet/files/{id}/{name}
        private ResponseResult Download(string method, string relativeUrl, RequestParameters parameters)
        {
            
            Match match = new Regex(@"([0-9\-]*)/.*?").Match(relativeUrl);
            if (match.Success == true && OutStream != null)
            {
                try
                {
                    Show(Convert.ToInt32(match.Groups[1].Value), OutStream);
                    return new ResponseResult();
                }
                catch (Exception)
                {
                }
            }
            return null;
        }


        internal static void Show(int id, System.IO.Stream outStream)
        {
            ProxyEntry entry = new ProxyEntry(String.Format(Controller.ServiceBaseUrl + "sharedfiles/SharedFileObjects({0})/$value", id));
            entry.CanCache = (int)ProxyEntry.CanCacheOptions.CantCache;
            entry.UseServer = false;
            SharedProxy.Streams.Input.StreamItem item = null;

            try
            {
                int downloadMethod, readerId;
                item = SharedProxy.Streams.ProxyStreamManager.CreateStreamItem(entry, out downloadMethod, out readerId, false);
                if (item != null)
                {
                    var outputWriter = new OutputStreamWriter(entry, outStream);
                    outputWriter.WriteToOutput(item, readerId);
                }
            }
            catch (Exception ex)
            {
                ClientAndServerShared.LogsController.WriteException("Shared file", ex.Message);
            }
            finally
            {
                if (item != null)
                    item.DisposeAllReaders();
            }
        }
    }
}
