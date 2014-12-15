using System.Collections.Generic;
using System.IO;
using System.ServiceModel.Activation;
using System.ServiceModel.Channels;
using System.ServiceModel.Web;
using CentralServiceCore;
using System.Linq;
using System;
using Newtonsoft.Json;

namespace CentralRestService
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class FeedbackService : IFeedbackService
    {
        [WebInvoke(Method = "POST", UriTemplate = "/Send")]
        public void Send(Stream stream)
        {
            var dict = JsonHelper.Deserialize<Dictionary<string, string>>(stream);
            FeedbackReporter reporter = new FeedbackReporter();
            reporter.WorkspaceId = dict["workspace_id"].ToString();
            reporter.Save(dict["feedback"].ToString(), dict["output"].ToString(), dict["client_id"].ToString());
        }
    }
}