using System;
using System.Collections.Generic;
using CentralServiceCore.Data;
using CentralServiceCore.Test.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using CentralServiceCore.Helpers;

namespace CentralServiceCore.Test
{
    [TestClass]
    public class SyncJournalReporterTest
    {
        [TestInitialize]
        public void Initialize()
        {
            TestInitializer.Init();
        }

        [TestMethod]
        public void TestGetSyncState()
        {
            var workspace = new Workspace()
            {
                Name = "Workspace",
                WorkspaceId = "WorkspaceId"
            };
            DataController.Container.Workspaces.Add(workspace);
            workspace.SyncJournalStates.Add(DataController.Container.SyncJournalStates.Add(new SyncJournalState()
            {
                ClientId = "1",
                Workspace = workspace,
                LastClientRecordNumber = 100,
                DateCreated = DateTime.Now
            }));
            workspace.SyncJournalStates.Add(DataController.Container.SyncJournalStates.Add(new SyncJournalState()
            {
                ClientId = "1",
                Workspace = workspace,
                GroupId = 2,
                LastClientRecordNumber = 200,
                DateCreated = DateTime.Now
            }));
            workspace.SyncJournalStates.Add(DataController.Container.SyncJournalStates.Add(new SyncJournalState()
            {
                ClientId = "2",
                Workspace = workspace,
                GroupId = 2,
                LastClientRecordNumber = 300,
                DateCreated = DateTime.Now
            }));

            SyncJournalReporter reporter = new SyncJournalReporter()
            {
                WorkspaceId = workspace.WorkspaceId,
                WorkspaceName = workspace.Name
            };
            var syncState = reporter.GetSyncState();
            Assert.AreEqual(2, syncState.Count);
            Assert.AreEqual(100, syncState[""]["1"]);
            Assert.AreEqual(200, syncState["2"]["1"]);
            Assert.AreEqual(300, syncState["2"]["2"]);
        }

        [TestMethod]
        public void TestSaveReport()
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

            var report = GetTestReport();
            reporter.SaveReport(report);

            Assert.AreEqual(5, DataController.Container.SyncJournalItems.Count());
            int i = 0;
            foreach (var item in DataController.Container.SyncJournalItems)
            {
                if (report[i]["group_id"] as string == "")
                    Assert.AreEqual(null, item.GroupId);
                else
                    Assert.AreEqual(Convert.ToInt32(report[i]["group_id"]), item.GroupId);
                Assert.AreEqual(report[i]["client_id"], item.ClientId);
                Assert.AreEqual(Convert.ToInt32(report[i]["client_rec_num"]), item.ClientRecordNumber);
                Assert.AreEqual((SyncJournalItem.OperationTypes)Convert.ToInt32(report[i]["operation_type"]),
                    item.OperationType);
                Assert.AreEqual(report[i]["uid"], item.Uid);
                Assert.AreEqual(report[i]["sync_with"], item.SyncWith);
                Assert.AreEqual(report[i]["table_name"], item.TableName);
                if (report[i].ContainsKey("columns"))
                    Assert.AreEqual(report[i]["columns"], item.Columns);

                i++;
            }

            Assert.AreEqual(3, workspace.SyncJournalStates.Count());
            Assert.IsNotNull(workspace.SyncJournalStates.Where(s =>
                s.GroupId == null && s.ClientId == "2" && s.LastClientRecordNumber == 10).FirstOrDefault());
            Assert.IsNotNull(workspace.SyncJournalStates.Where(s =>
                s.GroupId == 1 && s.ClientId == "2" && s.LastClientRecordNumber == 11).FirstOrDefault());
            Assert.IsNotNull(workspace.SyncJournalStates.Where(s =>
                s.GroupId == null && s.ClientId == "3" && s.LastClientRecordNumber == 15).FirstOrDefault());
        }

        internal static List<Dictionary<string, object>> GetTestReport()
        {
            var report = new List<Dictionary<string, object>>();
            report.Add(new Dictionary<string, object>()
            {
                { "group_id", "" },
                { "client_id", "2" },
                { "client_rec_num", "10" },
                { "operation_type", "1" },
                { "uid", "S145" },
                { "sync_with", "" },
                { "table_name", "users" },
                { "columns", "{ \"name\": \"Peter\", \"surname\": \"Brown\" }" },
                { "date_created", "2013-02-24T20:36:00" }
            });
            report.Add(new Dictionary<string, object>()
            {
                { "group_id", "" },
                { "client_id", "3" },
                { "client_rec_num", "13" },
                { "operation_type", "1" },
                { "uid", "S146" },
                { "sync_with", "" },
                { "table_name", "users" },
                { "columns", "{ \"name\": \"Edward\", \"surname\": \"Therence\" }" },
                { "date_created", "2013-02-25T20:36:00" }
            });
            report.Add(new Dictionary<string, object>()
            {
                { "group_id", "1" },
                { "client_id", "2" },
                { "client_rec_num", "11" },
                { "operation_type", "2" },
                { "uid", "S146" },
                { "sync_with", "21" },
                { "table_name", "users" },
                { "date_created", "2013-02-23T21:36:00" }
            });
            report.Add(new Dictionary<string, object>()
            {
                { "group_id", "" },
                { "client_id", "3" },
                { "client_rec_num", "14" },
                { "operation_type", "1" },
                { "uid", "S146" },
                { "sync_with", "" },
                { "table_name", "client_caches" },
                { "columns", "{ \"client_id\": \"CLIENT_ID\", \"cache_id\": \"1000\", \"date_created\": \"2013-01-25T20:36:00\" }" },
                { "date_created", "2013-02-25T20:36:00" }
            });
            report.Add(new Dictionary<string, object>()
            {
                { "group_id", "" },
                { "client_id", "3" },
                { "client_rec_num", "15" },
                { "operation_type", "2" },
                { "uid", "S146" },
                { "sync_with", "" },
                { "table_name", "client_caches" },
                { "date_created", "2013-02-25T20:36:00" }
            });
            return report;
        }

        [TestMethod]
        public void TestGetSyncStateAfterReport()
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

            var report = GetTestReport();
            reporter.SaveReport(report);

            var syncState = reporter.GetSyncState();
            Assert.AreEqual(2, syncState.Count);
            Assert.AreEqual(10, syncState[""]["2"]);
            Assert.AreEqual(11, syncState["1"]["2"]);
            Assert.AreEqual(15, syncState[""]["3"]);
        }

        [TestMethod]
        public void TestReportingClientCaches()
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

            var report = GetTestReport();
            reporter.SaveReport(report.Take(4));

            Assert.AreEqual(1, DataController.Container.WebObjects.Count());
            Assert.AreEqual(1, DataController.Container.ClientCaches.Count());
            var cache = DataController.Container.ClientCaches.First();
            Assert.AreEqual(1000, cache.WebObject.CacheId);
            Assert.AreEqual("CLIENT_ID", cache.ClientId);
            Assert.AreEqual(1, cache.DateCreated.Month);
        }
    }
}
