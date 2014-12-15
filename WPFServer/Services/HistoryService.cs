using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using WPFServer.DatabaseContext;

namespace WPFServer
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(UseSynchronizationContext = false, InstanceContextMode = InstanceContextMode.PerCall)]
    public class HistoryService : IHistoryService
    {

        [WebInvoke(UriTemplate = "register/visit/", Method = "POST")]
        public void Create(ServiceEntities.UserVisitsPage move)
        {
            if (move.Reason != ServiceEntities.ActivityType.Visit) return;

            try
            {
                using (MyDBContext con = new MyDBContext())
                {
                    Page page = con.FetchOrCreate(new Page() { AbsoluteURI = move.Page.AbsoluteURI, Title = move.Page.Title });
                    IQueryable<User> users = con.Fetch<User>(u => u.Id == move.User.Id);

                    if (users.Any())
                    {
                        User user = users.First();
                        UserVisitsPage visit = con.FetchOrCreate<UserVisitsPage>(new UserVisitsPage() { Page = page, User = user, VisitedAt = DateTime.Now, Count = 1 }, true);
                    }
                }
                Cache.SearchDatabase.SaveFromCache(move.Page.AbsoluteURI);
            }
            catch (Exception)
            {

            }
        }

        [WebInvoke(UriTemplate = "register/traverse/", Method = "POST")]
        public void CreateEdge(ServiceEntities.UserTraversesLink move)
        {
            if (move.Reason != ServiceEntities.ActivityType.Visit) return;

            try
            {
                using (MyDBContext con = new MyDBContext())
                {
                    Page page = con.FetchOrCreate(new Page() { AbsoluteURI = move.Page.AbsoluteURI, Title = move.Page.Title });
                    IQueryable<User> users = con.Fetch<User>(u => u.Id == move.User.Id);

                    if (!String.IsNullOrWhiteSpace(move.From.AbsoluteURI))
                    {
                        Page pageFrom = con.FetchOrCreate(new Page() { AbsoluteURI = move.From.AbsoluteURI, Title = move.From.Title });
                        Edge edge = con.FetchOrCreate(new Edge() { PageFrom = pageFrom, PageTo = page });

                        if (users.Any())
                        {
                            User user = users.First();
                            UserVisitsPage visitTo = con.FetchOrCreate<UserVisitsPage>(new UserVisitsPage() { Page = page, User = user, VisitedAt = DateTime.Now, Count = 1 }, true);
                            UserVisitsPage visitFrom = con.FetchOrCreate<UserVisitsPage>(new UserVisitsPage() { Page = pageFrom, User = user });
                            UserTraversesEdge traverse = con.FetchOrCreate(new UserTraversesEdge() { Edge = edge, User = user, TimeStamp = DateTime.Now, Frequency = 1 }, true);
                        }
                    }
                }
                Cache.SearchDatabase.SaveFromCache(move.Page.AbsoluteURI);

                if (!String.IsNullOrWhiteSpace(move.From.AbsoluteURI))
                    Cache.SearchDatabase.SaveFromCache(move.From.AbsoluteURI);
            }
            catch (Exception)
            {
            }
        }

        [WebInvoke(UriTemplate = "prefetch/complete/", Method = "POST")]
        public void PrefetchCompleted(ServiceEntities.PageObject page)
        {
            try
            {
                WPFServer.Prefetching.Prefetcher.DownloadCompleted(page.AbsoluteURI);
            }
            catch (Exception)
            {
            } 
        }

        [WebGet(UriTemplate = "predict/user/{usersid}/?page={uri}")]
        public ServiceEntities.PrefetchLinks PredictPersonalized(string usersid, string uri)
        {

            ServiceEntities.PrefetchLinks ret = new ServiceEntities.PrefetchLinks();

            try
            {
                uri = System.Web.HttpUtility.UrlDecode(uri);

                int userid = Convert.ToInt32(usersid);

                ret = Prefetching.PredictionEvaluation.Predict(uri, userid);
            }
            catch (MyDBContextException e)
            {
                throw new WebFaultException<ServiceEntities.ErrorHandler>(new ServiceEntities.ErrorHandler { cause = e.Message, inner = e.InnerException != null ? e.InnerException.Message : "", errorCode = 500 }, System.Net.HttpStatusCode.InternalServerError);
            }
            catch (Exception e)
            {
                throw new WebFaultException<ServiceEntities.ErrorHandler>(new ServiceEntities.ErrorHandler { cause = e.Message, inner = e.InnerException != null ? e.InnerException.Message : "", errorCode = 500 }, System.Net.HttpStatusCode.InternalServerError);
            }
            return ret;
        }

        [WebGet(UriTemplate = "predict/?page={uri}")]
        public ServiceEntities.PrefetchLinks PredictNotPersonalized(string uri)
        {

            ServiceEntities.PrefetchLinks ret = new ServiceEntities.PrefetchLinks();

            try
            {
                uri = System.Web.HttpUtility.UrlDecode(uri);
                ret = Prefetching.PredictionEvaluation.Predict(uri);
            }
            catch (MyDBContextException e)
            {
                throw new WebFaultException<ServiceEntities.ErrorHandler>(new ServiceEntities.ErrorHandler { cause = e.Message, inner = e.InnerException != null ? e.InnerException.Message : "", errorCode = 500 }, System.Net.HttpStatusCode.InternalServerError);
            }
            catch (Exception e)
            {
                throw new WebFaultException<ServiceEntities.ErrorHandler>(new ServiceEntities.ErrorHandler { cause = e.Message, inner = e.InnerException != null ? e.InnerException.Message : "", errorCode = 500 }, System.Net.HttpStatusCode.InternalServerError);
            }
            return ret;
        }

        [WebGet(UriTemplate = "period/{period}/user/{usersid}/page/{part}/")]
        public ServiceEntities.VisitedPages VisitedToday(string period, string usersid, string part)
        {
            ServiceEntities.VisitedPages ret = new ServiceEntities.VisitedPages();
            ServiceEntities.PageObjectWithDateTime powdt;
            IQueryable<User> users;
            User user;

            try
            {
                int pag = Convert.ToInt32(part);
                int userid = Convert.ToInt32(usersid);
                DateTime from = DateTime.Today;
                DateTime to = DateTime.Today;
                switch (period)
                {
                    case "today":
                        from = DateTime.Today;
                        to = DateTime.Today.AddDays(1);
                        break;
                    case "yesterday":
                        from = DateTime.Today.AddDays(-1);
                        to = DateTime.Today;
                        break;
                    case "week":
                        from = DateTime.Today.AddDays(-7);
                        to = DateTime.Today;
                        break;
                    case "month":
                        from = DateTime.Today.AddDays(-30);
                        to = DateTime.Today;
                        break;
                    case "complete":
                        from = new DateTime(2012, 1, 1);
                        to = DateTime.Today.AddDays(1);
                        break;
                    default:
                        return ret;
                }

                using (MyDBContext con = new MyDBContext())
                {
                    try
                    {
                        users = con.Fetch<User>(u => u.Id == userid);

                        if (!users.Any())
                            return ret;
                        user = users.First();
                     
                        IQueryable<UserVisitsPage> visitsQuery = con.Fetch<UserVisitsPage>(uvp => uvp.UserId == user.Id && uvp.VisitedAt != null && uvp.VisitedAt >= from && uvp.VisitedAt < to).OrderByDescending(uvp => uvp.VisitedAt);

                        int currentPage = pag;
                        int totalPages = pag;

                        Helpers.Search.ProcessPages(visitsQuery.Count(), pag, out totalPages, out currentPage);

                        ret.TotalPages = totalPages;
                        ret.CurrentPage = currentPage;

                        Helpers.Search.ExtractPage(ref visitsQuery, currentPage);

                        System.Collections.Generic.List<UserVisitsPage> visits = visitsQuery.ToList();

                        foreach (var visit in visits)
                        {
                            powdt = new ServiceEntities.PageObjectWithDateTime();
                            powdt.AbsoluteURI = visit.Page.AbsoluteURI;
                            powdt.Title = visit.Page.Title;
                            powdt.Id = visit.Page.Id;
                            powdt.AvgRating = visit.Rating ?? 0.0;
                            powdt.VisitTimeStamp = visit.VisitedAt ?? DateTime.Now;
                            ret.Visits.Add(powdt);
                        }
                    }
                    catch (Exception) { }
                }
            }
            catch (Exception e)
            {
                throw new WebFaultException<ServiceEntities.ErrorHandler>(new ServiceEntities.ErrorHandler { cause = e.Message, inner = e.InnerException != null ? e.InnerException.Message : "", errorCode = 500 }, System.Net.HttpStatusCode.InternalServerError);
            }

            return ret;
        }

        [WebInvoke(UriTemplate = "user/{usersid}/page/{pageid}/", Method = "DELETE")]
        public bool Delete(string usersid, string pageid)
        {
            int uid = 0;
            int pid = 0;
            try
            {
                uid = Convert.ToInt32(usersid);
                pid = Convert.ToInt32(pageid);

                using (MyDBContext con = new MyDBContext())
                {
                    try
                    {
                        return con.FindAndRemove<UserVisitsPage>(uvp => uvp.PageId == pid && uvp.UserId == uid);
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            catch (MyDBContextException e)
            {
                throw new WebFaultException<ServiceEntities.ErrorHandler>(new ServiceEntities.ErrorHandler { cause = e.Message, inner = e.InnerException != null ? e.InnerException.Message : "", errorCode = 500 }, System.Net.HttpStatusCode.InternalServerError);
            }
            catch (Exception e)
            {
                throw new WebFaultException<ServiceEntities.ErrorHandler>(new ServiceEntities.ErrorHandler { cause = e.Message, inner = e.InnerException != null ? e.InnerException.Message : "", errorCode = 400 }, System.Net.HttpStatusCode.BadRequest);
            }
            return false;
        }

    }
}