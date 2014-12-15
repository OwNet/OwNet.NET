using System;
using System.Collections.Generic;
using CentralServiceCore.Test.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace CentralServiceCore.Test
{
    [TestClass]
    public class SyncJournalInjectionTest
    {
        [TestInitialize]
        public void Initialize()
        {
            TestInitializer.Init();
        }

        [TestMethod]
        public void TestInject()
        {
            SyncJournalInjection injection = new SyncJournalInjection();
            injection.WorkspaceId = "WRK1";

            injection.Inject("client_caches",
                new System.Collections.Generic.Dictionary<string, object>() {
                    { "cache_id", 98765 },
                    { "client_id", "adasd" }
                });

            SyncJournalUpdater updater = new SyncJournalUpdater();
            updater.WorkspaceId = "WRK1";
            var updates = updater.GetUpdates(new Dictionary<string,Dictionary<string,int>>());

            Assert.AreEqual(1, updates.Count);
            Assert.AreEqual("adasd", (updates.First()["columns"] as Dictionary<string, string>)["client_id"]);
        }
    }
}
