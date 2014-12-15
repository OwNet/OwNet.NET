using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Activation;
using System.ServiceModel.Channels;
using System.ServiceModel.Web;
using System.Text;
using System.Web;

namespace CentralRestService
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class PingService : IPingService
    {
        [WebGet(UriTemplate = "{workspaceId}")]
        public Message Hey(string workspaceId)
        {
            return WebOperationContext.Current.CreateTextResponse("OK",
                        "text/plain; charset=utf-8",
                        Encoding.UTF8);
        }
    }
}