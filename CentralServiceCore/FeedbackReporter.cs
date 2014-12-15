using CentralServiceCore.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CentralServiceCore
{
    public class FeedbackReporter : WorkspaceAction
    {
        public void Save(string type, string output, string clientId)
        {
            var container = DataController.Container;
            var workspace = GetWorkspace(container);
            workspace.Feedbacks.Add(container.Feedbacks.Add(new Feedback()
            {
                Type = type,
                Output = output,
                ClientId = clientId,
                DateCreated = DateTime.Now,
                Workspace = workspace
            }));
            container.SaveChanges();
        }
    }
}
