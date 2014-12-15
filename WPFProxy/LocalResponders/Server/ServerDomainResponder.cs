using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WPFProxy.Proxy;

namespace WPFProxy.LocalResponders.Server
{
    class ServerDomainResponder : LocalResponder
    {
        protected override string ThisUrl  { get { return "server.ownet/"; } }


        public ServerDomainResponder() : base()
        {
        }

        private System.IO.Stream OutStream = null;

        public ServerDomainResponder(System.IO.Stream outStream)
            : this()
        {
            OutStream = outStream;
        }

        protected override void InitRoutes()
        {
            Routes
                .RegisterRoute(null, "files/.*", new Files.FilesResponder(OutStream).Respond)
                .RegisterRoute(null, "prefetch/.*", new Prefetch.PrefetchResponder().Respond);
        }

        private static ResponseResult Index(string method, string relativeUrl, RequestParameters parameters)
        {
            return new ResponseResult() { Filename = "index.html", Data = LocalHelpers.MenuHelper.ReplyToPageWithMenu("index.html") };
        }



       
    }
}
