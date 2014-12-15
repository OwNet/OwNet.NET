using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Web;
using WPFServer.DatabaseContext;
using WPFServer.SharedFiles;
using ClientAndServerShared;

namespace WPFServer
{

    /*
    Routes:
    GET:
    rating/user/{id}/?page={url}      // vracia rating stranky userom
    POST: 
    rating/                           // nastavi/update rating stranky userom
     */

    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(UseSynchronizationContext = false, InstanceContextMode = InstanceContextMode.PerCall)]
    public class ActivityService : IActivityService
    {

        private static int itemsPerPage = 10;

        [WebGet(UriTemplate = "rating/user/{usersid}/?page={uri}")]
        public ServiceEntities.UserRatedPage GetRating(string usersid, string uri)
        {
            uri = HttpUtility.UrlDecode(uri);
            ServiceEntities.UserRatedPage ret = new ServiceEntities.UserRatedPage();
            ret.Page.AbsoluteURI = uri;

            User user;
            UserVisitsPage visit;

            IQueryable<User> users;
            IQueryable<UserVisitsPage> visits;
            try {
                using (MyDBContext con = new MyDBContext())
                {
                    try
                    {
                        int userid = Convert.ToInt32(usersid);
                        users = con.Fetch<User>(u => u.Id == userid);
                        var pages = con.Fetch<Page>(new Page { AbsoluteURI = uri }).Select(n => new { n, rating = n.Visitors.Average(v => (int?)v.Rating) });


                        if (!users.Any() || !pages.Any())
                            return ret;
                        user = users.First();
                        var page = pages.First();

                        visits = con.Fetch<UserVisitsPage>(uvp => uvp.UserId == user.Id && uvp.PageId == page.n.Id);
                        ret.Page.AvgRating = page.rating ?? 0.0;
                        ret.Page.Id = page.n.Id;
                        if (!visits.Any())
                            return ret;
                        visit = visits.First();
                        ret.Rating = visit.Rating ?? 0;
                        ret.User.Username = user.Username;
                        ret.User.Id = user.Id;

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
                throw new WebFaultException<ServiceEntities.ErrorHandler>(new ServiceEntities.ErrorHandler { cause = e.Message, inner = e.InnerException != null ? e.InnerException.Message : "", errorCode = 500 }, System.Net.HttpStatusCode.InternalServerError);           
            }
            return ret;
        }

        [WebInvoke(UriTemplate = "rating/", Method = "POST")]
        public void SetRating(ServiceEntities.UserRatesPage rating)
        {
            try {
                using (MyDBContext con = new MyDBContext())
                {
                    IQueryable<User> users = con.Fetch<User>(u => u.Id == rating.User.Id);
                    if (!users.Any())
                        return;
                    User user = users.First();
                    Page page = con.FetchOrCreate<Page>(new Page
                    {
                        AbsoluteURI = rating.Page.AbsoluteURI,
                        Title = rating.Page.Title
                    }, true);
                    UserVisitsPage vis = new UserVisitsPage ()
                    {
                        User = user,
                        Page = page,
                        Rating = rating.Rating
                    };
                    UserVisitsPage visit = con.FetchOrCreate<UserVisitsPage>(vis, true);


                    con.SaveChanges();
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
        }

        [WebInvoke(UriTemplate = "usage/", Method = "POST")]
        public void SetUsage(ServiceEntities.UserVisitsPage move)
        {
            try {
                WPFServer.DatabaseContext.ActivityType type;
                switch (move.Reason)
                {
                    case ServiceEntities.ActivityType.Rating: type = WPFServer.DatabaseContext.ActivityType.Rating; break;
                    case ServiceEntities.ActivityType.Recommend: type = WPFServer.DatabaseContext.ActivityType.Recommend; break;
                    case ServiceEntities.ActivityType.Share: type = WPFServer.DatabaseContext.ActivityType.Share; break;
                    case ServiceEntities.ActivityType.Search: type = WPFServer.DatabaseContext.ActivityType.Search; break;
                    case ServiceEntities.ActivityType.Visit: type = WPFServer.DatabaseContext.ActivityType.Visit; break;
                    default: return;
                }
                
                using (MyDBContext con = new MyDBContext())
                {
                    IQueryable<User> users = con.Fetch<User>(u => u.Id == move.User.Id);
                    if (!users.Any())
                        return;

                    Page page = null;
                    if (type != WPFServer.DatabaseContext.ActivityType.Share)   // takze je to stranka
                    {
                        IQueryable<Page> pages = null;
                        if (move.Page.Id != 0)
                        {
                            pages = con.Fetch<Page>(p => p.Id == move.Page.Id);
                        }
                        else if ((move.Page.Id == 0 || (pages != null && !pages.Any())) && !String.IsNullOrWhiteSpace(move.Page.AbsoluteURI)) 
                        {
                            pages = con.Fetch<Page>(new Page() { AbsoluteURI = move.Page.AbsoluteURI });
                        }

                        if (pages == null || !pages.Any())
                            return;
                        page = pages.First();
                    }

                    Activity act = con.CreateActivityItem(type, WPFServer.DatabaseContext.ActivityAction.Read, "" + ((page == null) ? move.Page.AbsoluteURI : ""), users.First(), false, page);
                    con.RegisterActivity(act);
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
        }

        /* adresa sluzby */
        [WebInvoke(UriTemplate = "recommend/", Method = "POST")]
        public int RecommendPage(ServiceEntities.UserRecommendsPage urp)
        {

            int ret = 0;

            try
            {
                // pripojenie sa k databaze 
                using (MyDBContext con = new MyDBContext())
                {
                    // Vyber pouzivatela vykonavajuceho odporucanie z databazy 
                    IQueryable<User> users = con.Fetch<User>(u => u.Id == urp.User.Id);
                    if (!users.Any())
                        return 0;
                    User user = users.First();

                    Page page = con.FetchOrCreate<Page>(new Page() { AbsoluteURI = urp.Page.AbsoluteURI, Title = urp.Page.Title }, true);

                    IQueryable<Group> groups = con.Fetch<Group>(new Group() { Name = urp.GroupName });
                    if (groups.Any())
                    {
                        Group group = groups.First();

                        // kontrola ci dana stranka uz nebola odporucana 
                        IQueryable<GroupRecommendation> r = con.Fetch<GroupRecommendation>(p => p.Page.Id == page.Id && p.Group.Id == group.Id && p.UserId == user.Id);
                        if (!r.Any())
                        {
                            // vytvorenie odporucania v databaze
                            con.FetchOrCreate<GroupRecommendation>(new GroupRecommendation()
                            {

                                Group = group,
                                User = user,
                                Page = page,
                                //dekodovanie  popisu a titulku
                                Description = HttpUtility.UrlDecode(urp.Recommendation.Description),
                                Title = HttpUtility.UrlDecode(urp.Recommendation.Title),
                                DateTime = DateTime.Now
                            }, true);
                            ret = 1;

                        }
                        else
                        {
                            var recommendation = r.First();
                            recommendation.Title = HttpUtility.UrlDecode(urp.Recommendation.Title);
                            recommendation.Description = HttpUtility.UrlDecode(urp.Recommendation.Description);
                            recommendation.DateTime = DateTime.Now;
                            ret = 2;
                        }
                        // zaznamenanie, ze pouzivatel pouzil skupinu
                        var ug = con.Fetch<UserGroup>(g => g.UserId == user.Id && g.GroupId == group.Id).First();
                        ug.LastUsed = DateTime.Now;

                        con.SaveChanges();
                        return ret;
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
            return 0;
        }

        [WebInvoke(UriTemplate = "recommend/group/{gid}/page/{pid}/", Method = "DELETE")]
        public bool DeleteRecommendedPage(string gid, string pid)
        {
            try
            {
                int id = Convert.ToInt32(pid);
                int groupid = Convert.ToInt32(gid);
                using (MyDBContext con = new MyDBContext())
                {
                    try
                    {
                        IQueryable<GroupRecommendation> recomms = con.Fetch<GroupRecommendation>(urp => urp.PageId == id && urp.GroupId == groupid);
                        if (recomms.Any())
                        {
                            GroupRecommendation recomm = recomms.First();
                            Page page = recomm.Page;
                            User user = recomm.User;
                            Group group = recomm.Group;
                            con.FetchSet<GroupRecommendation>().Remove(recomm);
                            con.SaveChanges();
                            int acttype = (int)ActivityType.Recommend;
                            IQueryable<Activity> acts  = con.Fetch<Activity>(a => a.TypeValue == acttype && a.UserId == user.Id && a.PageId == page.Id && a.GroupId == group.Id);
                            foreach (Activity act in acts)
                            {
                                act.Visible = false;
                            }
                            con.SaveChanges();
                            con.RegisterActivity(con.CreateActivityItem(ActivityType.Recommend, ActivityAction.Delete, "", user, false, page, null, group));

                            return true;
                        }
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



        [WebGet(UriTemplate = "recommend/group/{group}/page/{page}/user/{usersid}/")]
        public ServiceEntities.UserRecommendations GetRecommendedInGroupWithVisitedInformation(string group, string page, string usersid)
        {
            ServiceEntities.UserRecommendations ret = new ServiceEntities.UserRecommendations();
            try
            {
                using (MyDBContext con = new MyDBContext())
                {
                    int pag = Convert.ToInt32(page);
                    int userid = Convert.ToInt32(usersid);
                    ServiceEntities.UserRecommendedPageWithRating urpwr = null;
                    

                    try
                    {
                        IQueryable<User> users = con.Fetch<User>(u => u.Id == userid);
                        User user = null;
                        IQueryable<Group> groups = con.Fetch<Group>(new Group() { Id = Convert.ToInt32(group) } );
                        if (groups.Any() && users.Any())
                        {

                            user = users.First();
                            Group gr = groups.First();

                            IQueryable<GroupRecommendation> recs = con.Fetch<GroupRecommendation>(r => r.Group == gr && r.User == user);
                            GroupRecommendation rec = recs.First();

                            var allRecomms = (from p in recs
                                              select new { p, rating = p.Page.Visitors.Average(v => (int?)v.Rating) } into g
                                              orderby g.p.DateTime descending
                                              select new { g.p, g.rating });

                            int currentPage = pag;
                            int totalPages = pag;

                            Helpers.Search.ProcessPages(allRecomms.Count(), pag, out totalPages, out currentPage);

                            ret.TotalPages = totalPages;
                            ret.CurrentPage = currentPage;

                            var recomms = allRecomms.Skip(Helpers.Search.SkipBeforePage(currentPage)).Take(Helpers.Search.ItemsPerPage);


                            foreach (var urpig in recomms)
                            {
                                urpwr = new ServiceEntities.UserRecommendedPageWithRating();
                                urpwr.User.Username = urpig.p.User.Username;
                                urpwr.User.Id = urpig.p.User.Id;
                                urpwr.User.Firstname = urpig.p.User.Firstname;
                                urpwr.User.Surname = urpig.p.User.Surname;
                                urpwr.User.IsTeacher = urpig.p.User.IsTeacher;
                                urpwr.Page.AbsoluteURI = urpig.p.Page.AbsoluteURI;
                                urpwr.Page.Title = urpig.p.Page.Title;
                                urpwr.Page.Id = urpig.p.Page.Id;
                                urpwr.Recommendation.Group.Name = gr.Name;
                                urpwr.Recommendation.Group.Id = gr.Id;
                                urpwr.Recommendation.Title = urpig.p.Title;
                                urpwr.Recommendation.Description = urpig.p.Description;
                                urpwr.Recommendation.IsSet = true;
                                urpwr.Page.AvgRating = urpig.rating ?? 0.0;
                                urpwr.RecommendationTimeStamp = urpig.p.DateTime;
                                if (urpig.p.Page.Visitors.Where(p => p.UserId == user.Id).Any())
                                {
                                    urpwr.Visited = true;
                                }
                                ret.Recommendations.Add(urpwr);

                            }
                        }
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
                throw new WebFaultException<ServiceEntities.ErrorHandler>(new ServiceEntities.ErrorHandler { cause = e.Message, inner = e.InnerException != null ? e.InnerException.Message : "", errorCode = 500 }, System.Net.HttpStatusCode.InternalServerError);
            }

            return ret;
        }

        [WebGet(UriTemplate = "top/all/page/{part}/")]
        public ServiceEntities.RatedPages GetAllTopRated(string part)
        {
            ServiceEntities.RatedPages ret = new ServiceEntities.RatedPages();
            try
            {
                using (MyDBContext con = new MyDBContext())
                {

                    int pag = Convert.ToInt32(part);
                    ServiceEntities.PageObjectWithRating urpwr = null;
                    try
                    {
                        var allRatings = con.Fetch<Page>(y => y.Visitors.Any()).Select(y => new { y, rating = y.Visitors.Average(v => (int?)v.Rating) }).Where(y => y.rating > 0).OrderByDescending(v => v.rating);

                        int currentPage = pag;
                        int totalPages = pag;

                        Helpers.Search.ProcessPages(allRatings.Count(), pag, out totalPages, out currentPage);

                        ret.TotalPages = totalPages;
                        ret.CurrentPage = currentPage;

                        var ratings = allRatings.Skip(Helpers.Search.SkipBeforePage(currentPage)).Take(Helpers.Search.ItemsPerPage);

                        foreach (var page in ratings)
                        {
                            urpwr = new ServiceEntities.PageObjectWithRating();
                            urpwr.AbsoluteURI = page.y.AbsoluteURI;
                            urpwr.Title = page.y.Title;
                            urpwr.Id = page.y.Id;
                            urpwr.AvgRating = page.rating ?? 0.0;

                            ret.Ratings.Add(urpwr);
                        }
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
                throw new WebFaultException<ServiceEntities.ErrorHandler>(new ServiceEntities.ErrorHandler { cause = e.Message, inner = e.InnerException != null ? e.InnerException.Message : "", errorCode = 500 }, System.Net.HttpStatusCode.InternalServerError);
            }

            return ret;
        }

        [WebGet(UriTemplate = "top/user/{usersid}/page/{part}/")]
        public ServiceEntities.RatedPages GetUserTopRated(string usersid, string part)
        {
            ServiceEntities.RatedPages ret = new ServiceEntities.RatedPages();

            try
            {
                using (MyDBContext con = new MyDBContext())
                {
                    int pag = Convert.ToInt32(part);
                    ServiceEntities.PageObjectWithRating urpwr;
                    IQueryable<User> users;
                    User user;
                    try
                    {

                        int userid = Convert.ToInt32(usersid);
                        users = con.Fetch<User>(u => u.Id == userid);


                        if (!users.Any())
                            return ret;
                        user = users.First();

                        IQueryable<UserVisitsPage> ratings = con.Fetch<UserVisitsPage>(uvp => uvp.UserId == user.Id && uvp.Rating != null).OrderByDescending(uvp => uvp.Rating);

                        int currentPage = pag;
                        int totalPages = pag;

                        Helpers.Search.ProcessPages(ratings.Count(), pag, out totalPages, out currentPage);

                        ret.TotalPages = totalPages;
                        ret.CurrentPage = currentPage;

                        Helpers.Search.ExtractPage(ref ratings, currentPage);

                        foreach (var rating in ratings)
                        {
                            urpwr = new ServiceEntities.PageObjectWithRating();
                            urpwr.AbsoluteURI = rating.Page.AbsoluteURI;
                            urpwr.Title = rating.Page.Title;
                            urpwr.AvgRating = rating.Rating ?? 0.0;
                            urpwr.Id = rating.Page.Id;

                            ret.Ratings.Add(urpwr);
                        }
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
                throw new WebFaultException<ServiceEntities.ErrorHandler>(new ServiceEntities.ErrorHandler { cause = e.Message, inner = e.InnerException != null ? e.InnerException.Message : "", errorCode = 500 }, System.Net.HttpStatusCode.InternalServerError);
            }
            return ret;
        }

        [WebGet(UriTemplate = "recommend/not_displayed/{usersid}/")]
        public int GetCountNotDisplayedRecomms(string usersid)
        {
            int ret = 0;
            try
            {
                using (MyDBContext con = new MyDBContext())
                {
                    try
                    {
                        int userid = Convert.ToInt32(usersid);
                        IQueryable<User> users = con.Fetch<User>(u => u.Id == userid);
                        if (users.Any())
                        {
                            User user = users.First();

                            var recommendationObjects = from ug in user.UserGroups
                                                        let gr = ug.Group
                                                        from r in gr.Recommendations
                                                        where r.DateTime.CompareTo(user.LastDisplayedRecom) == 1 && r.UserId != user.Id/*> user.LastDisplayedRecom*/  /*filter*/
                                                        orderby r.DateTime descending

                                                        select new
                                                        {
                                                            Recommendation = r,
                                                        };

                            ret = recommendationObjects.Count();
                        }
                    }
                    catch (Exception ex)
                    {
                        LogsController.WriteException("GetCountNotDisplayedRecomms", ex.Message);
                    }
                }
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

        [WebGet(UriTemplate = "recommend/recent/user/{usersid}/")]
        public ServiceEntities.UserRecommendations GetRecommendedMostRecentWithVisitedInformation(string usersid)
        {
            ServiceEntities.UserRecommendations ret = new ServiceEntities.UserRecommendations() { TotalPages = 1, CurrentPage = 1, Recommendations = new List<ServiceEntities.UserRecommendedPageWithRating>() };
            try
            {
                using (MyDBContext con = new MyDBContext())
                {
                    ServiceEntities.UserRecommendedPageWithRating urpwr = null;
                    try
                    {
                        int userid = Convert.ToInt32(usersid);
                        IQueryable<User> users = con.Fetch<User>(u => u.Id == userid);
                        if (users.Any())
                        {
                            User user = users.First();


                            var recommendationObjects = from ug in user.UserGroups
                                                        let gr = ug.Group
                                                        from r in gr.Recommendations
                                                        where r.DateTime.CompareTo(user.LastDisplayedRecom) == 1  // filtre  limit skip na paging
                                                        orderby r.DateTime descending

                                                        select new
                                                        {
                                                            Recommendation = r,
                                                            Rating = r.Page.Visitors.Average(v => (int?)v.Rating)
                                                        };

                            itemsPerPage = 10;

                            if (recommendationObjects.Count() > itemsPerPage)
                                itemsPerPage = recommendationObjects.Count();

                            /* ak nie je  dostatok nevydenych doplnime starsie */
                            if (recommendationObjects.Count() < itemsPerPage)
                            {
                                var recommendationObjects2 = from ug in user.UserGroups
                                                             let gr = ug.Group
                                                             from r in gr.Recommendations
                                                             orderby r.DateTime descending

                                                             select new
                                                             {
                                                                 Recommendation = r,
                                                                 Rating = r.Page.Visitors.Average(v => (int?)v.Rating)
                                                             };

                                recommendationObjects = recommendationObjects.Union(recommendationObjects2);
                            }

                            var recomms = con.FetchSet<GroupRecommendation>().Select(y => new { y, rating = y.Page.Visitors.Average(v => (int?)v.Rating) }).OrderByDescending(v => v.y.DateTime).Take(itemsPerPage);

                            foreach (var urpig in recommendationObjects.Take(itemsPerPage))
                            {
                                urpwr = new ServiceEntities.UserRecommendedPageWithRating();
                                urpwr.User.Username = urpig.Recommendation.User.Username;
                                urpwr.User.Id = urpig.Recommendation.User.Id;
                                urpwr.User.Firstname = urpig.Recommendation.User.Firstname;
                                urpwr.User.Surname = urpig.Recommendation.User.Surname;
                                urpwr.User.IsTeacher = urpig.Recommendation.User.IsTeacher;

                                urpwr.Page.AbsoluteURI = urpig.Recommendation.Page.AbsoluteURI;
                                urpwr.Page.Title = urpig.Recommendation.Page.Title;
                                urpwr.Page.AvgRating = urpig.Rating ?? 0.0;
                                urpwr.Page.Id = urpig.Recommendation.Page.Id;
                                urpwr.Recommendation.Group.Name = urpig.Recommendation.Group.Name;
                                urpwr.Recommendation.Description = urpig.Recommendation.Description;
                                urpwr.Recommendation.Title = urpig.Recommendation.Title;
                                urpwr.RecommendationTimeStamp = urpig.Recommendation.DateTime;
                                if (urpig.Recommendation.User.Id == user.Id || urpig.Recommendation.DateTime.CompareTo(user.LastDisplayedRecom) != 1)
                                    urpwr.IsNew = false;
                                else
                                    urpwr.IsNew = true;

                                if (urpig.Recommendation.Page.Visitors.Where(p => p.UserId == user.Id).Any())
                                {
                                    urpwr.Visited = true;
                                }
                                ret.Recommendations.Add(urpwr);
                            }

                            user.LastDisplayedRecom = DateTime.Now;
                            user.SaveChanges();
                            con.SaveChanges();
                        }
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
                throw new WebFaultException<ServiceEntities.ErrorHandler>(new ServiceEntities.ErrorHandler { cause = e.Message, inner = e.InnerException != null ? e.InnerException.Message : "", errorCode = 500 }, System.Net.HttpStatusCode.InternalServerError);
            }

            return ret;
        } 

        [WebInvoke(UriTemplate = "share/files/create/", Method = "POST")]
        public void ShareFile(ServiceEntities.SharedFile sharedFile)
        {
            try
            {
                using (MyDBContext con = new MyDBContext())
                {
                    IQueryable<User> users = con.Fetch<User>(u => u.Id == sharedFile.User.Id);
                    IQueryable<SharedFolder> folders = con.Fetch<SharedFolder>(u => u.Id == sharedFile.SharedFolderId);
                    IQueryable<SharedFileObject> fileObjects = con.Fetch<SharedFileObject>(u => u.Id == sharedFile.FileObjectId);
                    if (users.Any() && folders.Any() && fileObjects.Any())
                    {
                        User user = users.First();
                        SharedFolder folder = folders.First();
                        SharedFileObject fileObject = fileObjects.First();

                        SharedFile file = new SharedFile()
                        {
                            Title = sharedFile.Title,
                            Description = sharedFile.Description,
                            SharedFileObject = fileObject,
                            User = user,
                            SharedFolder = folder,
                            FileName = sharedFile.FileName
                        };                        
                        con.FetchSet<SharedFile>().Add(file);
                        Activity act = con.CreateActivityItem(DatabaseContext.ActivityType.Share, DatabaseContext.ActivityAction.Create, FileSharing.GetFolderPath(folder) + "\\", user, true, null, file);
                        con.RegisterActivity(act);
                        con.SaveChanges();
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
        }

        [WebInvoke(UriTemplate = "share/folders/create/", Method = "POST")]
        public ServiceEntities.SharedFolder CreateSharedFolder(ServiceEntities.SharedFolder sharedFolder)
        {
            try
            {
                using (MyDBContext con = new MyDBContext())
                {
                    IQueryable<SharedFolder> folders = con.Fetch<SharedFolder>(u => u.Id == sharedFolder.ParentFolderId);
                    if (folders.Any())
                    {
                        SharedFolder folder = folders.First();
                        SharedFolder newFolder = new SharedFolder()
                        {
                            Name = sharedFolder.Name,
                            ParentFolder = folder
                        };

                        con.FetchSet<SharedFolder>().Add(newFolder);

                        con.SaveChanges();
                        sharedFolder.Id = newFolder.Id;
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
            return sharedFolder;
        }

        [WebInvoke(UriTemplate = "share/folders/rename/", Method = "POST")]
        public void RenameSharedFolder(ServiceEntities.SharedFolder sharedFolder)
        {
            try
            {
                using (MyDBContext con = new MyDBContext())
                {

                    IQueryable<SharedFolder> folders = con.Fetch<SharedFolder>(u => u.Id == sharedFolder.Id);
                    if (folders.Any())
                    {
                        SharedFolder folder = folders.First();
                        folder.Name = sharedFolder.Name;

                        con.SaveChanges();
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
        }

        [WebInvoke(UriTemplate = "share/folders/delete/", Method = "POST")]
        public void DeleteSharedFolder(ServiceEntities.SharedFolder sharedFolder)
        {
            try
            {
                using (MyDBContext con = new MyDBContext())
                {
                    var folders = con.Fetch<SharedFolder>(u => u.Id == sharedFolder.Id);
                    if (folders.Any())
                    {
                        SharedFolder folder = folders.First();
                        FileSharing.DeleteFolder(folder, con);

                        con.SaveChanges();
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
        }

        [WebGet(UriTemplate = "share/folders/")]
        public ServiceEntities.SharedFolder GetSharedFolderStructure()
        {
            ServiceEntities.SharedFolder rootFolder = null;
            try
            {
                using (MyDBContext con = new MyDBContext())
                {
                    rootFolder = FileSharing.GetFolderStructure(con);
                }
            }
            catch (Exception e)
            {
                throw new WebFaultException<ServiceEntities.ErrorHandler>(new ServiceEntities.ErrorHandler { cause = e.Message, inner = e.InnerException != null ? e.InnerException.Message : "", errorCode = 500 }, System.Net.HttpStatusCode.InternalServerError);
            }

            return rootFolder;
        }

        [WebGet(UriTemplate = "share/folders/{folderId}/")]
        public ServiceEntities.SharedFolder GetSharedFolder(string folderId)
        {
            ServiceEntities.SharedFolder folder = null;
            try
            {
                int id = Convert.ToInt32(folderId);
                using (MyDBContext con = new MyDBContext())
                {
                    folder = FileSharing.GetFolder(id, con);
                    if (folder == null)
                    {
                        SharedFolder folderItem = FileSharing.GetRootFolderItem(con);
                        folder = new ServiceEntities.SharedFolder()
                        {
                            Name = folderItem.Name,
                            FullPath = folderItem.Name,
                            ParentFolderId = -1
                        };
                    }
                }
            }
            catch (Exception e)
            {
                throw new WebFaultException<ServiceEntities.ErrorHandler>(new ServiceEntities.ErrorHandler { cause = e.Message, inner = e.InnerException != null ? e.InnerException.Message : "", errorCode = 500 }, System.Net.HttpStatusCode.InternalServerError);
            }

            return folder;
        }

        [WebInvoke(UriTemplate = "share/files/{fileId}/delete/", Method="DELETE")]
        public void DeleteSharedFile(string fileId)
        {
            try
            {
                using (MyDBContext con = new MyDBContext())
                {
                    int id = Convert.ToInt32(fileId);
                    var files = con.Fetch<SharedFile>(u => u.SharedFileObject.Id == id);
                    if (files.Any())
                    {
                        SharedFile file = files.First();

                        FileSharing.DeleteFile(file, con);
                        
                        con.SaveChanges();
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
        }

        [WebGet(UriTemplate = "news/page/{page}/")]
        public ServiceEntities.ActivityList GetAllActivities(string page)
        {
            ServiceEntities.ActivityList ret = new ServiceEntities.ActivityList();
            User user = null;
            IQueryable<Activity> newsQuery;
            
            int[] actions = { (int)ActivityAction.Create, (int)ActivityAction.Update, (int)ActivityAction.Delete };
            int[] types = { (int)ActivityType.Share, (int)ActivityType.Tag, (int)ActivityType.Register, (int)ActivityType.Recommend, (int)ActivityType.Rating, (int)ActivityType.Group };
            
            try
            {
                int pag = Convert.ToInt32(page);
                using (MyDBContext con = new MyDBContext())
                {
                    try
                    {
                        // select only some of them
                        newsQuery = con.Fetch<Activity>(a => actions.Contains(a.ActionValue) && types.Contains(a.TypeValue) && a.Visible == true).OrderByDescending(n => n.TimeStamp);
                        ServiceEntities.Activity item = null;

                        int currentPage = pag;
                        int totalPages = pag;

                        Helpers.Search.ProcessPages(newsQuery.Count(), pag, out totalPages, out currentPage);

                        ret.TotalPages = totalPages;
                        ret.CurrentPage = currentPage;

                        Helpers.Search.ExtractPage(ref newsQuery, currentPage);

                        List<Activity> news = newsQuery.ToList();

                        foreach (Activity newsItem in news)
                        {
                            item = new ActivityEntity(newsItem);
                           
                            ret.Activities.Add(item);
                        }
                        if (user != null)
                        {
                            user.LastVisit = DateTime.Now;
                            con.SaveChanges();
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

        [WebInvoke(UriTemplate = "news/{id}/", Method = "DELETE")]
        public bool HideActivity(string id)
        {
            try
            {
                int aid = Convert.ToInt32(id);

                using (MyDBContext con = new MyDBContext())
                {
                    try
                    {
                        IQueryable<Activity> acts = con.Fetch<Activity>(a => a.Id == aid);
                        if (acts.Any())
                        {
                            acts.First().Visible = false;
                            con.SaveChanges();
                            return true;
                        }
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

        [WebGet(UriTemplate = "live/stream/")]
        public ServiceEntities.ActivityList GetLiveStream()
        {
            ServiceEntities.ActivityList ret = new ServiceEntities.ActivityList();
            ret.Activities = LiveStream.ActivityStream;

            return ret;
        }

        [WebGet(UriTemplate = "live/stream?since={since}")]
        public ServiceEntities.ActivityList GetLiveStreamSince(string since)
        {
            DateTime dtSince = Helpers.Common.FullDateTimeFromString(since);
            ServiceEntities.ActivityList ret = new ServiceEntities.ActivityList();
            ret.Activities = LiveStream.GetNewerThan(dtSince);

            return ret;
        }

        [WebInvoke(UriTemplate = "messages/create/", Method = "POST")]
        public void CreateMessage(ServiceEntities.Activity activity)
        {
            try
            {
                using (MyDBContext con = new MyDBContext())
                {
                    IQueryable<User> users = con.Fetch<User>(u => u.Id == activity.User.Id);
                    if (users.Any())
                    {
                        User user = users.First();
                        activity.User.Username = user.Username;
                        activity.User.Firstname = user.Firstname;
                        activity.User.Surname = user.Surname;
                        activity.User.IsTeacher = user.IsTeacher;
                    }
                }
            }
            catch (Exception e)
            {
                ClientAndServerShared.LogsController.WriteException(e.Message);
            }
            LiveStream.AddActivity(activity);
        }
    }
}
