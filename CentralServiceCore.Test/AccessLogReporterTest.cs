using System;
using System.Collections.Generic;
using CentralServiceCore.Data;
using CentralServiceCore.Test.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace CentralServiceCore.Test
{
    [TestClass]
    public class AccessLogReporterTest
    {
        [TestInitialize]
        public void Initialize()
        {
            TestInitializer.Init();
        }

        [TestMethod]
        public void TestSaveReport()
        {
            var report = new List<Dictionary<string, object>>();
            report.Add(new Dictionary<string, object>()
            {
                { "cache_id", 4000000000 },
                { "absolute_uri", "http://sme.sk" },
                { "accessed_at", "2013-02-25T20:36:00" }
            });
            report.Add(new Dictionary<string, object>()
            {
                { "cache_id", 1000000000 },
                { "absolute_uri", "http://apple.com" },
                { "accessed_at", "2013-02-25T20:36:00" }
            });
            report.Add(new Dictionary<string, object>()
            {
                { "cache_id", 47474747 },
                { "absolute_uri", "http://www.zones.sk" },
                { "accessed_at", "2013-02-25T20:36:00" }
            });


            var reporter = new AccessLogReporter();
            reporter.WorkspaceId = "workpace_id";
            reporter.SaveReport(report);

            Assert.AreEqual(3, DataController.Container.WebObjects.Count());
            Assert.AreEqual(3, DataController.Container.AccessLogs.Count());

            var obj = DataController.Container.WebObjects.Where(o => o.AbsoluteUri == "http://sme.sk").FirstOrDefault();
            Assert.AreEqual(4000000000, obj.CacheId);
            Assert.AreEqual(2, obj.AccessLogs.FirstOrDefault().AccessedAt.Month);

            obj = DataController.Container.WebObjects.Where(o => o.AbsoluteUri == "http://apple.com").FirstOrDefault();
            Assert.AreEqual((long)1000000000, obj.CacheId);
            Assert.AreEqual(36, obj.AccessLogs.FirstOrDefault().AccessedAt.Minute);

            obj = DataController.Container.WebObjects.Where(o => o.AbsoluteUri == "http://www.zones.sk").FirstOrDefault();
            Assert.AreEqual((long)47474747, obj.CacheId);
        }
    }
}
