using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CentralServiceCore.Test.Helpers;
using CentralServiceCore.Data;
using System.Linq;

namespace CentralServiceCore.Test
{
    [TestClass]
    public class WorkspaceActionTest
    {
        [TestInitialize]
        public void Initialize()
        {
            TestInitializer.Init();
        }

        [TestMethod]
        public void TestGetWorkspace()
        {
            WorkspaceAction action = new WorkspaceAction();
            action.WorkspaceId = "Workspace id";
            action.WorkspaceName = "Workspace name";

            var workspace = action.GetWorkspace();
            Assert.AreEqual("Workspace id", workspace.WorkspaceId);
            Assert.AreEqual("Workspace name", workspace.Name);
        }
    }
}
