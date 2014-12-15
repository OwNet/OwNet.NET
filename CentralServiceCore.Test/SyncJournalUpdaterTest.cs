using System;
using System.Collections.Generic;
using CentralServiceCore.Data;
using CentralServiceCore.Test.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CentralServiceCore.Test
{
    [TestClass]
    public class SyncJournalUpdaterTest
    {
        [TestInitialize]
        public void Initialize()
        {
            TestInitializer.Init();
        }

        [TestMethod]
        public void TestGetUpdates()
        {
            var workspace = new Workspace()
            {
                Name = "Workspace",
                WorkspaceId = "WorkspaceId"
            };
            DataController.Container.Workspaces.Add(workspace);

            SyncJournalReporter reporter = new SyncJournalReporter();
            reporter.WorkspaceId = workspace.WorkspaceId;
            reporter.WorkspaceName = workspace.Name;

            var report = SyncJournalReporterTest.GetTestReport();
            reporter.SaveReport(report);

            SyncJournalUpdater updater = new SyncJournalUpdater();
            updater.WorkspaceId = workspace.WorkspaceId;

            var updates = updater.GetUpdates(reporter.GetSyncState(), true);
            Assert.AreEqual(0, updates.Count);

            updates = updater.GetUpdates(new Dictionary<string, Dictionary<string, int>>(), false);
            Assert.AreEqual(4, updates.Count);

            updates = updater.GetUpdates(new Dictionary<string, Dictionary<string, int>>(), true);
            Assert.AreEqual(5, updates.Count);


            updates = updater.GetUpdates(new Dictionary<string, Dictionary<string, int>>()
            {
                { "", new Dictionary<string, int>() { { "3", 13 } } }
            }, true);
            Assert.AreEqual(4, updates.Count);
        }
    }
}
