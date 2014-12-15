using CentralServiceCore.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CentralServiceCore
{
    public class WorkspaceAction
    {
        string _workspaceId = "";
        string _workspaceName = "";

        public string WorkspaceId
        {
            get { return _workspaceId; }
            set { _workspaceId = value; }
        }

        public string WorkspaceName
        {
            get { return _workspaceName; }
            set { _workspaceName = value; }
        }

        public Workspace GetWorkspace()
        {
            return GetWorkspace(DataController.Container);
        }

        public Workspace GetWorkspace(ICentralDataModelContainer container)
        {
            Workspace workspace = container.Workspaces.Where(w => w.WorkspaceId == WorkspaceId).FirstOrDefault();
            if (workspace == null)
            {
                workspace = new Workspace()
                {
                    WorkspaceId = WorkspaceId,
                    Name = WorkspaceName,
                    DateCreated = DateTime.Now
                };
                container.Workspaces.Add(workspace);
            }
            else if (workspace.Name != WorkspaceName)
            {
                workspace.Name = WorkspaceName;
            }
            return workspace;
        }
    }
}
