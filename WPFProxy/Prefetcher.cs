using System;
using System.Collections.Generic;
using System.Linq;
using ClientAndServerShared;
using Prefetching;
using WPFProxy.Database;

namespace WPFProxy
{
    
    public static class Prefetcher
    {
        public static string ToMessage(this Prefetcher.Status status)
        {
            switch (status)
            {
                case Prefetcher.Status.Cancel: return "Cancelled";
                case Prefetcher.Status.Complete: return "Completed";
                case Prefetcher.Status.Disabled: return "Disabled";
                case Prefetcher.Status.Fail: return "Failed";
                case Prefetcher.Status.Timeout: return "Timed out";
            }
            return "Ordered";
        }

        public class PrefetchOrdersList : ServiceEntities.CollectionWithPaging
        {
            public List<PrefetchOrders> Orders { get; set; }

            public PrefetchOrdersList()
                : base()
            {
                Orders = new List<PrefetchOrders>();
            }
        }

        private static object _ordersLock = null;

        public static PrefetchOrdersList GetPrefetchOrders(int page, bool scheduled)
        {
            PrefetchOrdersList ret = new PrefetchOrdersList();
            try
            {
                lock (_ordersLock)
                {
              
                    using (DatabaseEntities db = Controller.GetDatabase())
                    {
                        IQueryable<PrefetchOrders> orders = db.PrefetchOrders.Where(o => o.IsScheduled == scheduled).OrderByDescending(o => o.Completed).ThenByDescending(o => o.DateCompleted).ThenByDescending(o => o.DateCreated);

                        int currentPage = page;
                        int totalPages = page;

                        Helpers.Search.ProcessPages(orders.Count(), page, out totalPages, out currentPage);

                        ret.TotalPages = totalPages;
                        ret.CurrentPage = currentPage;
                        Helpers.Search.ExtractPage(ref orders, currentPage);
                        ret.Orders = orders.ToList(); // Skip(Helpers.Search.SkipBeforePage(currentPage)).Take(Helpers.Search.ItemsPerPage)
                    }       
                }
            }
            catch (Exception e)
            {
                LogsController.WriteException("GetPrefetchOrders()", e.Message);
            }
            

            return ret;
        }
        public static void RegisterPredictions(ServiceEntities.PageObject from, List<ServiceEntities.PageObjectWithRating> toVarious)
        {
            if (toVarious == null || toVarious.Count <= 0 || String.IsNullOrWhiteSpace(from.AbsoluteURI))
                return;
           
                try
                {
                    lock (_ordersLock)
                    {
                        using (DatabaseEntities db = Controller.GetDatabase())
                        {
                            IQueryable<PrefetchOrders> oldOrders = db.PrefetchOrders.Where(t => t.IsScheduled == false && t.Enabled == true && t.Completed == false && t.Priority > 0);

                            bool changes = oldOrders.Any();

                            foreach (PrefetchOrders oldOrder in oldOrders)
                            {
                                oldOrder.Priority -= 1;
                            }

                            oldOrders = db.PrefetchOrders
                                .Where(t => t.IsScheduled == false && t.Enabled == false && t.Completed == false)
                                .OrderByDescending(t => t.DateCreated)
                                .ThenByDescending(o => o.Priority)
                                .Skip(30);

                            changes = changes ? true : oldOrders.Any();

                            foreach (PrefetchOrders oldOrder in oldOrders)
                            {
                                db.PrefetchOrders.DeleteObject(oldOrder);
                            }

                            if (changes)
                            {
                                Controller.SubmitDatabaseChanges(db);
                            }

                            Dictionary<int, ServiceEntities.PageObjectWithRating> pages = toVarious.Where(t => !String.IsNullOrWhiteSpace(t.AbsoluteURI)).ToDictionary(t => WPFProxy.Cache.ProxyCache.GetUriHash(t.AbsoluteURI), t => t);
                            List<int> hashes = null;
                            if (pages.Any())
                            {
                                hashes = pages.Keys.ToList();
                                List<int> cached = db.Caches.Where(c => hashes.Contains(c.ID)).Select(c => c.ID).ToList();

                                hashes = hashes.Except(cached).ToList();
                            }

                            if (hashes != null)
                            {
                                foreach (int hash in hashes)
                                {
                                    ServiceEntities.PageObjectWithRating to = pages[hash];

                                    PrefetchOrders order = null;

                                    IQueryable<PrefetchOrders> orders = db.PrefetchOrders.Where(o => o.ToAbsoluteUri == to.AbsoluteURI);

                                    if (orders.Any())
                                    {
                                        order = orders.First();
                                        if (order.IsScheduled == false 
                                            && order.Completed == false 
                                            && (order.Enabled == true || order.Status == (byte)Status.Disabled) 
                                            && order.Priority < 50)
                                        {
                                            order.FromAbsoluteUri = from.AbsoluteURI;
                                            order.FromTitle = from.Title;
                                            order.DateCompleted = null;
                                            order.Enabled = true;
                                            order.Status = (byte)Status.None;
                                            order.DateCreated = DateTime.Now;
                                            order.Priority += 1;
                                        }
                                    }
                                    else
                                    {
                                        db.PrefetchOrders.AddObject(
                                            new PrefetchOrders()
                                            {
                                                FromAbsoluteUri = from.AbsoluteURI,
                                                FromTitle = from.Title,
                                                ToAbsoluteUri = to.AbsoluteURI,
                                                ToTitle = to.Title,
                                                DateCreated = DateTime.Now,
                                                IsScheduled = false,
                                                Enabled = true,
                                                Completed = false,
                                                DateCompleted = null,
                                                Priority = 30,
                                                Status = 0
                                            }
                                        );
                                    }
                                }
                                Controller.SubmitDatabaseChanges(db);
                            }
                        }
                        
                    }
                }
                catch (Exception e)
                {
                    LogsController.WriteException("RegisterPredictions(): " + e.Message);
                }
            
        }

        public static bool RegisterSchedule(string fromTitle, string fromUri, string toTitle, string toUri)
        {
            if (String.IsNullOrWhiteSpace(toUri)) 
                return false;
            PrefetchOrders order;
            try
            {
                lock (_ordersLock)
                {
                    using (DatabaseEntities db = Controller.GetDatabase())
                    {
                        if (db.PrefetchOrders.Any(t => t.ToAbsoluteUri == toUri))
                        {   // update
                            order = db.PrefetchOrders.First(t => t.ToAbsoluteUri == toUri);
                            order.Enabled = true;
                            order.Completed = false;
                            order.DateCompleted = null;
                            order.DateCreated = DateTime.Now;
                            order.FromAbsoluteUri = fromUri;
                            order.FromTitle = fromTitle;
                            order.ToAbsoluteUri = toUri;
                            order.ToTitle = toTitle;
                            order.IsScheduled = true;
                            order.Priority = 50;
                            order.Status = 0;
                        }
                        else
                        {
                            db.PrefetchOrders.AddObject(
                                new PrefetchOrders() {
                                    FromAbsoluteUri = fromUri,
                                    Enabled = true,
                                    Completed = false,
                                    FromTitle = fromTitle,
                                    ToAbsoluteUri = toUri,
                                    ToTitle = toTitle,
                                    DateCreated = DateTime.Now,
                                    IsScheduled = true,
                                    DateCompleted = null,
                                    Priority = 50,
                                    Status = 0
                                });
                        }
                        Controller.SubmitDatabaseChanges(db);
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                LogsController.WriteException("RegisterSchedule(): " + e.Message);
                return false;
            }
        }

        public static void DisablePredictions(string fromUri)
        {
            if (String.IsNullOrWhiteSpace(fromUri)) 
                return;
            try
            {
                lock (_ordersLock)
                {
                    using (DatabaseEntities db = Controller.GetDatabase())
                    {
                        IQueryable<PrefetchOrders> orders = db.PrefetchOrders.Where(o => o.FromAbsoluteUri == fromUri && o.IsScheduled == false && o.Completed == false);

                        bool changes = orders.Any();
                        foreach (PrefetchOrders order in orders)
                        {
                            order.Status = (byte)Status.Disabled;
                            order.Enabled = false;
                        }

                        if (changes) 
                            Controller.SubmitDatabaseChanges(db);
                    }
                }
            }
            catch (Exception e)
            {
                LogsController.WriteException("DisablePredictions()", e.Message);
            }

        }

        public static void EnablePredictions(string fromUri)
        {
            if (String.IsNullOrWhiteSpace(fromUri))
                return;
            try
            {
                lock (_ordersLock)
                {
                    using (DatabaseEntities db = Controller.GetDatabase())
                    {
                        IQueryable<PrefetchOrders> orders = db.PrefetchOrders.Where(o => o.FromAbsoluteUri == fromUri && o.IsScheduled == false && o.Completed == false && o.Status == (byte)Status.Disabled);
                        bool changes = orders.Any();
                        foreach (PrefetchOrders order in orders)
                        {
                            order.Status = (byte)Status.None;
                            order.Enabled = true;
                        }
                        if (changes) 
                           Controller.SubmitDatabaseChanges(db);
                    }
                }
            }
            catch (Exception e)
            {
                LogsController.WriteException("EnablePredictions()", e.Message);
            }

        }
       
        public static bool RemoveOrder(int id)
        {
            bool deleted = false;
            try
            {
                lock (_ordersLock)
                {
                    using (DatabaseEntities db = Controller.GetDatabase())
                    {
                        if (db.PrefetchOrders.Any(o => o.Id == id))
                        {
                            db.PrefetchOrders.DeleteObject(db.PrefetchOrders.First(o => o.Id == id));
                            Controller.SubmitDatabaseChanges(db);
                            deleted = true;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                LogsController.WriteException("RemoveOrder(): " + e.Message);
            }
            return deleted;
        }


        private static bool running = false;        // if there is being anything prefetched right now
        private static object _runningLock = new object();

        private static BrowserWorker browser = new BrowserWorker();

        static Prefetcher()
        {
            browser.Completed += new BrowserWorker.CompletedEventHandler(browser_Completed);
            browser.Cancelled += new BrowserWorker.CancelledEventHandler(browser_Cancelled);
            browser.TimedOut += new BrowserWorker.TimedOutEventHandler(browser_TimedOut);
            _ordersLock = new object();

            try
            {
                lock (_ordersLock)
                {
                    using (DatabaseEntities db = Controller.GetDatabase())
                    {
                        IQueryable<PrefetchOrders> oldOrders = db.PrefetchOrders.Where(o => o.IsScheduled == false);
                        foreach (PrefetchOrders order in oldOrders)
                        {
                            db.PrefetchOrders.DeleteObject(order);
                        }
                        Controller.SubmitDatabaseChanges(db);
                    }
                }
            }
            catch (Exception e)
            {
                LogsController.WriteException("Prefetcher", e.Message);
            }

        }

        
        public enum Status : byte
        {
            None = 0,              
            Complete = 1,           // successfully prefetched 
            Disabled = 2,           // user left page
            Fail = 4,               // used all attempts without success
            Cancel = 8,             // uri is not valid
            Timeout = 16            // prefetching timed out 
        }

        static void browser_TimedOut(object sender, BrowserWorkerEventArgs e)
        {
            Stop(e.Uri, Status.Timeout); // browser don't fire more documentcompleted events
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
            if (Helpers.Proxy.OfflineCheck.IsOffline == false && SharedProxy.Proxy.ProxyTraffic.LastTraffic() < 40)
            {
                try
                {
                    lock (_ordersLock)
                    {
                        using (DatabaseEntities db = Controller.GetDatabase())
                        {
                            IQueryable<PrefetchOrders> orders = db.PrefetchOrders
                                .Where(o => o.Enabled == true && o.Attempts < 3)
                                .OrderByDescending(o => o.IsScheduled)
                                .ThenByDescending(o => o.Priority)
                                .ThenBy(o => o.Attempts)
                                .ThenByDescending(o => o.DateCreated)
                                .ThenBy(o => o.Status);

                            if (orders.Any())
                            {
                                foreach (PrefetchOrders order in orders)
                                {
                                    order.Attempts += 1;
                                    if (!String.IsNullOrWhiteSpace(order.ToAbsoluteUri) && browser.Start("http://proxy.ownet/prefetch/load?page=" + System.Web.HttpUtility.UrlEncode(order.ToAbsoluteUri), order.ToAbsoluteUri, "127.0.0.1", "8081")) 
                                    {
                                        any = true;
                                        break;
                                    }
                                    else
                                    {
                                        order.Enabled = false;
                                        order.Status = (byte)Status.Fail;
                                        LogsController.WriteException("Prefetching failed for", order.ToAbsoluteUri);
                                    }
                                }
                                Controller.SubmitDatabaseChanges(db);
                            }
                            orders = db.PrefetchOrders.Where(o => o.Enabled && o.Attempts >= 3);
                            if (orders.Any())
                            {
                                foreach (PrefetchOrders order in orders) 
                                {
                                    order.Enabled = false;
                                    order.Status = (byte)Status.Cancel;
                                }
                                Controller.SubmitDatabaseChanges(db);
                            }
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

        internal static void DownloadCompleted(string url)
        {
            SetStatus(url, Status.Complete);
        }

        private static void SetStatus(string url, Status status)
        {
            try
            {
                lock (_ordersLock)
                {
                    using (DatabaseEntities db = Controller.GetDatabase())
                    {
                        IQueryable<PrefetchOrders> orders = db.PrefetchOrders.Where(o => ((status == Status.Complete && o.Enabled == false) || (o.Enabled == true && o.Completed == false)) && o.ToAbsoluteUri == url);
                        foreach (PrefetchOrders order in orders)
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
                        Controller.SubmitDatabaseChanges(db);
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
