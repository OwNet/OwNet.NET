using System;
using System.Collections.Generic;
using System.Linq;
using Prefetching;
using ClientAndServerShared;
using WPFServer.DatabaseContext;

namespace WPFServer.Prefetching
{
    public class Prefetcher
    {
        private static bool running = false;        // if there is being anything prefetched right now
        private static object _runningLock = new object();

        private static BrowserWorker browser = new BrowserWorker();
        private static object _ordersLock = null;

        public static void RegisterPredictions(List<ServiceEntities.CentralService.v2.LinkToUpdate> linksToPrefetch)
        {
            if (linksToPrefetch == null || linksToPrefetch.Count <= 0)
                return;
            else
            {  
                try
                {
                    lock (_ordersLock)
                    {
                        using (MyDBContext db = new MyDBContext())
                        {
                            IQueryable<Prefetch> oldOrders = db.Fetch<Prefetch>(o => o.Enabled == true && o.Completed == false);
                            foreach (Prefetch oldOrder in oldOrders)
                            {
                                if (oldOrder.Priority > 0) oldOrder.Priority -= 1;
                            }
                            db.SaveChanges();

                            foreach (ServiceEntities.CentralService.v2.LinkToUpdate link in linksToPrefetch)
                            {
                                if (System.Text.RegularExpressions.Regex.IsMatch(link.AbsoluteUri, "^http://.*"))
                                {
                                    if (db.Fetch<Prefetch>(t => t.AbsoluteUri.Equals(link.AbsoluteUri)).Any())
                                        continue;
                                    db.FetchSet<Prefetch>().Add(
                                        new Prefetch()
                                        {
                                            AbsoluteUri = link.AbsoluteUri,
                                            Priority = (link.Priority < 0) ? (byte) ((link.Priority * -1) % 256) : (byte) (link.Priority % 256),
                                            DateCreated = DateTime.Now,
                                            Attempts = 0,
                                            Completed = false,
                                            Enabled = true
                                        }
                                    );
                                }
                            }
                            db.SaveChanges();
                        }
                    }
                }
                catch (Exception e)
                {
                    LogsController.WriteException("RegisterPredictions(): " + e.Message);
                }
            }
        }

        static Prefetcher()
        {
            browser.Completed += new BrowserWorker.CompletedEventHandler(browser_Completed);
            browser.Cancelled += new BrowserWorker.CancelledEventHandler(browser_Cancelled);
            browser.TimedOut += new BrowserWorker.TimedOutEventHandler(browser_TimedOut);
            _ordersLock = new object();
        }

        public enum Status : byte
        {
            None = 0,
            Complete = 1,
            Disabled = 2,
            Fail = 4,
            Cancel = 8,
            Timeout = 16
        }

        static void browser_TimedOut(object sender, BrowserWorkerEventArgs e)
        {
            Stop(e.Uri, Status.Timeout); // should be false, but browser don't fire more documentcompleted events
        }

        static void browser_Cancelled(object sender, BrowserWorkerEventArgs e)
        {
            LogsController.WriteException("BrowserThread()", e.Message);
            Stop(e.Uri, Status.Cancel);
        }

        static void browser_Completed(object sender, BrowserWorkerEventArgs e)
        {
            Stop(e.Uri, Status.Complete);
        }

        public static bool Start() 
        {
            lock (_runningLock)
            {
                if (running) 
                    return false;
                running = true;
            }
            bool any = false;
            if (SharedProxy.Proxy.ProxyTraffic.LastTraffic() < 2)
            {
                try
                {
                    using (MyDBContext db = new MyDBContext())
                    {
                        IQueryable<Prefetch> orders = db
                            .Fetch<Prefetch>(o => o.Enabled == true && o.Attempts < 3)
                            .OrderByDescending(o => o.Priority)
                            .ThenBy(o => o.Attempts)
                            .ThenByDescending(o => o.DateCreated)
                            .ThenBy(o => o.Status);

                        if (orders.Any())
                        {
                            foreach (Prefetch order in orders)
                            {
                                order.Attempts += 1;
                                if (!String.IsNullOrWhiteSpace(order.AbsoluteUri) && browser.Start("http://server.ownet/prefetch/load?page=" + System.Web.HttpUtility.UrlEncode(order.AbsoluteUri), order.AbsoluteUri, "127.0.0.1", "8081"))
                                {
                                    any = true;
                                    break;
                                }
                                else
                                {
                                    order.Enabled = false;
                                    order.Status = (byte)Status.Fail;
                                    LogsController.WriteException("Prefetching failed for", order.AbsoluteUri);
                                }
                            }
                            db.SaveChanges();
                        }
                        orders = db.Fetch<Prefetch>(o => o.Enabled && o.Attempts >= 3);
                        if (orders.Any())
                        {
                            foreach (Prefetch order in orders)
                            {
                                order.Enabled = false;
                                order.Status = (byte)Status.Cancel;
                            }
                            db.SaveChanges();
                        }
                    }

                
                }
                catch (Exception e)
                {
                    LogsController.WriteException("PrefetchingStart()", e.Message);
                }
            }

            running = any;

            return any;
        }

        public static void DownloadCompleted(string url)
        {
            SetStatus(url, Status.Complete);
        }


        private static void SetStatus(string url, Status status)
        {
            try
            {
                lock (_ordersLock)
                {
                    using (MyDBContext db = new MyDBContext())
                    {
                        IQueryable<Prefetch> orders = db.Fetch<Prefetch>(o =>
                            ((status == Status.Complete && o.Enabled == false) || (o.Enabled == true && o.Completed == false))
                            && o.AbsoluteUri == url);
                        foreach (Prefetch order in orders)
                        {
                            order.Status = (byte)status;
                            if (status == Status.Complete)
                            {
                                order.Completed = true;
                                order.Enabled = false;
                                order.DateCompleted = DateTime.Now;
                            }
                            else if (status == Status.Cancel)
                            {
                                order.Enabled = false;
                            }
                        }
                        db.SaveChanges();
                    }
                }
            }
            catch (Exception e)
            {
                LogsController.WriteException("PrefetchingStop()", e.Message);
            }
        }


        private static void Stop(string url, Status status)
        {
            bool resetTimer = false;
            lock (_runningLock)
            {
                if (running)
                {
                    running = false;
                    resetTimer = true;
                }
            }

            SetStatus(url, status);

            if (resetTimer)
                Jobs.ResetTimerPrefetchLinks();
        }    
    }
}
