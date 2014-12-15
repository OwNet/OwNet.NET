using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CentralServiceCore.Data;

namespace CentralServiceCore
{
    public class AccessLogReporter : WorkspaceAction
    {
        public void SaveReport(IEnumerable<Dictionary<string, object>> items)
        {
            var container = DataController.Container;
            var workspace = GetWorkspace(container);
            foreach (var item in items)
            {
                long cacheId = long.Parse(item["cache_id"].ToString());

                var webObject = container.WebObjects.Where(o => o.CacheId == cacheId).FirstOrDefault();
                if (webObject == null)
                {
                    webObject = new WebObject()
                    {
                        AbsoluteUri = item["absolute_uri"].ToString(),
                        CacheId = cacheId,
                        IsTracked = false,
                        DateCreated = DateTime.Now,
                        DateUpdated = DateTime.Now
                    };
                    container.WebObjects.Add(webObject);
                }
                webObject.AbsoluteUri = item["absolute_uri"].ToString();
                webObject.AccessLogs.Add(container.AccessLogs.Add(new AccessLog()
                {
                    WebObject = webObject,
                    AccessedAt = DateTime.Parse(item["accessed_at"].ToString()),
                    Workspace = workspace
                }));
            }
            container.SaveChanges();
        }
    }
}
