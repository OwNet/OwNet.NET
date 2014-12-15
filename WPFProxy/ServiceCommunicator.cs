using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using ClientAndServerShared;
using Helpers;
using Microsoft.Http;
using ServiceEntities;
using WPFProxy.Proxy;

namespace WPFProxy
{
    class ServiceCommunicator
    {
        private static System.Text.RegularExpressions.Regex rreg = new System.Text.RegularExpressions.Regex(@"[\r\n\t\'""]", System.Text.RegularExpressions.RegexOptions.Compiled);

        public static string ComposeUsername()
        {
            return ComposeUsername(Settings.UserFirstname, Settings.UserSurname);
            //(Settings.Instance.UserFirstName ?? "") + (String.IsNullOrWhiteSpace(Settings.Instance.UserSurname)  ? "default" : Settings.Instance.UserSurname);
        }
        public static string ComposeUsername(string firstname, string surname)
        {
            string ret = (firstname + surname).Replace(" ", "");
            if (String.IsNullOrWhiteSpace(ret)) ret = "Anonymous";
            return ret;
            //(Settings.Instance.UserFirstName ?? "") + (String.IsNullOrWhiteSpace(Settings.Instance.UserSurname)  ? "default" : Settings.Instance.UserSurname);
        }

        

        public static RegistrationCheck RegisterUser(RequestParameters pars)
        {
            RegistrationCheck status = new RegistrationCheck();
            if (!Controller.UseServer || Settings.IsLoggedIn()) return status;
            string nick, first, sur, pass, pass2,gender, email;
            UserRegisters pobj;
            try
            {
                nick = pars.GetValue("username");
                nick = HttpUtility.UrlDecode(nick);
                first = pars.GetValue("firstname");
                first = HttpUtility.UrlDecode(first);

                sur = pars.GetValue("surname");
                sur = HttpUtility.UrlDecode(sur);

                pass = pars.GetValue("password");
                pass = HttpUtility.UrlDecode(pass);
                pass2 = pars.GetValue("password2");
                pass2 = HttpUtility.UrlDecode(pass2);

                email = pars.GetValue("email");
                email = HttpUtility.UrlDecode(email);


                gender = pars.GetValue("gender").ToLower();

                status = new RegistrationCheck(nick, first, sur, pass, pass2, gender, email);

                // TODO validation

                if (status.IsCorrect)
                {
                    pobj = new UserRegisters();
                    pobj.Username = nick;
                    pobj.Firstname = first;
                    pobj.Surname = sur;
                    pobj.Password = pass;
                    pobj.IsMale = gender == "female" ? false : true;
                    pobj.Email = email;

                    status.UserRegistered = ServerRequestManager.Post<UserRegisters, bool>("user/register/", pobj);
                }
            }
            catch (Exception e)
            {
                LogsController.WriteException("RegisterUser()", e.Message);
            }
            return status;
        }

        public static UpdateCheck UpdateUser(RequestParameters pars)
        {
            UpdateCheck status = new UpdateCheck();
            if (!Controller.UseServer || !Settings.IsLoggedIn()) return status;
            string  first, sur, gender, email, sid, role;
            User pobj;
            bool teacher = false;
            UserUpdated robj = null;
            try
            {
                sid = pars.GetValue("userid");
                int id = Convert.ToInt32(sid);
                if (Settings.UserID != id && Settings.UserTeacher != true) return status;

                first = pars.GetValue("firstname");
                first = HttpUtility.UrlDecode(first);

                sur = pars.GetValue("surname");
                sur = HttpUtility.UrlDecode(sur);

                role = pars.GetValue("role");
                role = HttpUtility.UrlDecode(role);

                if (!String.IsNullOrWhiteSpace(role))
                {
                    if (Settings.UserTeacher)
                    {
                        teacher = role == "teacher" ? true : false;
                    }
                    else
                        return status;
                }
                else if (Settings.UserTeacher)
                {
                    teacher = true;
                }


                email = pars.GetValue("email");
                email = HttpUtility.UrlDecode(email);

                gender = pars.GetValue("gender").ToLower();

                status = new UpdateCheck(first, sur, gender, email);
                
                // TODO validation

                if (status.IsCorrect)
                {
                    pobj = new User();
                    pobj.Firstname = first;
                    pobj.Surname = sur;
                    pobj.IsMale = gender == "female" ? false : true;
                    pobj.IsTeacher = teacher;
                    pobj.Email = email;

                    robj = ServerRequestManager.Put<User, UserUpdated>("user/" + Convert.ToString(id) + "/", pobj);
                    if (robj != null)
                    {
                        status.UserUpdated = robj.Success;
                    }

                    if (status.WasSuccessful && Settings.UserID == robj.Id)
                    {
                        Settings.UserHasLoggedIn(robj.Username, robj.Firstname, robj.Surname, robj.Email, robj.Id, robj.IsTeacher, robj.IsMale);
                    }
                }
            }
            catch (Exception e)
            {
                LogsController.WriteException("UpdateUser()", e.Message);
            }
            return status;
        }

        public static ChangePasswordCheck ChangeUserPassword(RequestParameters pars)
        {
            ChangePasswordCheck status = new ChangePasswordCheck();
            if (!Controller.UseServer || !Settings.IsLoggedIn()) return status;
            string sid, oldpass, pass, pass2;
            bool reset = false;
            UserUpdatesPassword pobj;
            try
            {
                sid = pars.GetValue("userid");
                int id = Convert.ToInt32(sid);
                if (Settings.UserID != id && Settings.UserTeacher != true) return status;


                oldpass = pars.GetValue("password0");
                oldpass = HttpUtility.UrlDecode(oldpass);

                pass = pars.GetValue("password1");
                pass = HttpUtility.UrlDecode(pass);
                pass2 = pars.GetValue("password2");
                pass2 = HttpUtility.UrlDecode(pass2);

                if (Settings.UserTeacher && Settings.UserID != id)
                {
                    oldpass = "12345";
                    reset = true;
                }
                
                status = new ChangePasswordCheck(oldpass, pass, pass2);

                // TODO validation

                if (status.IsCorrect)
                {
                    pobj = new UserUpdatesPassword();
                    pobj.OldPassword = oldpass;
                    pobj.Password = pass;
                    pobj.Reset = reset;
                    status.PasswordChanged = ServerRequestManager.Put<UserUpdatesPassword, bool>("user/" + Convert.ToString(id) + "/password/", pobj);

                    if (status.WasSuccessful)
                    {
                        // logout?
                    }
                }
            }
            catch (Exception e)
            {
                LogsController.WriteException("ChangeUserPassword()", e.Message);
            }
            return status;
        }

        public static UserLoggedIn UserLogIn(UserLogsIn pobj)
        {
            UserLoggedIn ret = null;
            if (Controller.UseServer)
            {
                try
                {
                    ret = ServerRequestManager.Post<UserLogsIn, UserLoggedIn>("user/login/", pobj);
                }
                catch (Exception e)
                {
                    LogsController.WriteException("UserLogin()", e.Message);
                }
            }
            return ret ?? new UserLoggedIn();
        }

        public static void RegisterSearch(string page)
        {
            string uri = null;
            UserVisitsPage uvp;
            if (Controller.UseServer)
            {
                try
                {
                    uri = HttpUtility.UrlDecode(page);
                    if (!String.IsNullOrWhiteSpace(uri))
                    {
                        uvp = new UserVisitsPage();
                        uvp.User.Id = Settings.UserID;
                        uvp.Page.AbsoluteURI = uri;
                        uvp.Page.Title = rreg.Replace(HttpLocalResponder.GetPageTitle(uri), " ");
                        ServerRequestManager.PostWithoutResponse<UserVisitsPage>("history/register/visit/", uvp);
                    }
                }
                catch (Exception e)
                {
                    LogsController.WriteException("RegisterSearch()", e.Message);
                }
            }
        }

        public static void RegisterEdge(string from, string to)
        {
            UserTraversesLink utl;
            if (Controller.UseServer)
            {
                try
                {
                    from = HttpUtility.UrlDecode(from);
                    to = HttpUtility.UrlDecode(to);
                    if (!String.IsNullOrWhiteSpace(to) && !String.IsNullOrWhiteSpace(from))
                    {
                        utl = new UserTraversesLink();
                        utl.User.Id = Settings.UserID;
                        utl.Page.AbsoluteURI = to;
                        utl.Page.Title = rreg.Replace(HttpLocalResponder.GetPageTitle(to), " ");
                        utl.From.AbsoluteURI = from;
                        utl.From.Title = rreg.Replace(HttpLocalResponder.GetPageTitle(from), " ");
                        ServerRequestManager.PostWithoutResponse<UserTraversesLink>("history/register/traverse/", utl);
                    }
                }
                catch (Exception e)
                {
                    LogsController.WriteException("RegisterEdge()", e.Message);
                }
            }
        }

        public static PrefetchLinks ReceivePrefetchPrediction(string from, List<string> links)
        {
            PrefetchLinks ret = null;
            PrefetchLinks pobj = new PrefetchLinks();
            if (Controller.UseServer)
            {
                try
                {
                    from = HttpUtility.UrlDecode(from);
                   
                    if (!String.IsNullOrWhiteSpace(from))
                    {
                        // TODO pouzitie dostupnych odkazov zo stranky na cistenie modelu 
                        //if (links != null && links.Any())
                        //{
                        //    pobj.From.AbsoluteURI = from;
                        //    pobj.Count = 3;
                        //    foreach (string link in links)
                        //    {
                        //        pobj.Links.Add(new PageObject() { AbsoluteURI = link });
                        //    }
                        //    if (Settings.IsLoggedIn())
                        //    {
                        //        ret = ServerRequestManager.Post<PrefetchLinks, PrefetchLinks>("history/predict/" + Convert.ToString(Settings.UserID) + "/", pobj);
                        //    }
                        //    else
                        //    {
                        //        ret = ServerRequestManager.Post<PrefetchLinks, PrefetchLinks>("history/predict/", pobj);
                        //    }
                        //}
                        //else
                        //{
                        HttpQueryString vars = new HttpQueryString();
                        vars.Add("page", HttpUtility.UrlEncode(from));
                        //vars.Add("maxcount", Convert.ToString(3));

                        if (Settings.IsLoggedIn())
                        {
                            ret = ServerRequestManager.Get<PrefetchLinks>(new Uri("history/predict/user/" + Convert.ToString(Settings.UserID) + "/", UriKind.Relative), vars);
                        }
                        else
                        {
                            ret = ServerRequestManager.Get<PrefetchLinks>(new Uri("history/predict/", UriKind.Relative), vars);
                        }
                      //  }
                    }
                }
                catch (Exception e)
                {
                    LogsController.WriteException("ReceivePrefetchPrediction()", e.Message);
                }
            }
            return ret;
        }
        public static void ReportPrefetchCompleted(string page)
        {
            if (Controller.UseServer)
            {
                try
                {
                    page = HttpUtility.UrlDecode(page);
                    if (!String.IsNullOrWhiteSpace(page))
                    {
                        PageObject pobj = new PageObject() { AbsoluteURI = page };
                        ServerRequestManager.PostWithoutResponse<PageObject>("history/prefetch/complete/", pobj);
                    }
                }
                catch (Exception e)
                {
                    LogsController.WriteException("ReceivePrefetchPrediction()", e.Message);
                }
            }
        }

        public static ServerSearchResults ReceiveServerSearchResults(string query, string clients = "")
        {
            ServerSearchResults results = null;
            HttpQueryString vars;
            if (!String.IsNullOrWhiteSpace(query) && Controller.UseServer)
            {
                try
                {
                    vars = new HttpQueryString();
                    vars.Add("query", HttpUtility.UrlEncode(query));
                    vars.Add("clients", clients);
                    results = ServerRequestManager.Get<ServerSearchResults>(new Uri("search/", UriKind.Relative), vars);
                }
                catch (Exception e)
                {
                    LogsController.WriteException("ReceiveServerSearchResults()", e.Message);
                }
            }

            return results ?? new ServerSearchResults();

        }
        public static SearchResults ReceiveSearchTagsResults(string query, int page = 1)
        {
            SearchResults results = null;
            HttpQueryString vars;
            if (!String.IsNullOrWhiteSpace(query) && Controller.UseServer)
            {
                try
                {

                    vars = new HttpQueryString();
                    vars.Add("query", HttpUtility.UrlEncode(query));
                    vars.Add("page", Convert.ToString(page));
                    results = ServerRequestManager.Get<SearchResults>(new Uri("search/tag/", UriKind.Relative), vars);
                }

                catch (Exception e)
                {
                    LogsController.WriteException("ReceiveSearchTagsResults()", e.Message);
                }
            }

            return results ?? new SearchResults();

        }
        public static bool SendRating(RequestParameters pars)
        {
            if (!Controller.UseServer || !Settings.IsLoggedIn()) return false;
            UserRatesPage ratingObj;
            try
            {
                ratingObj = new UserRatesPage();

                string val = pars.GetValue("page");

                ratingObj.Page.AbsoluteURI = HttpUtility.UrlDecode(val);
                //ratingObj.Page.ContentType = "";
                ratingObj.Page.Title = rreg.Replace(HttpLocalResponder.GetPageTitle(val), " ");
                val = pars.GetValue("rating");
                ratingObj.Rating = Convert.ToInt32(val);

                string username = ComposeUsername();
                ratingObj.User.Id = Settings.UserID;
                ServerRequestManager.PostWithoutResponse<UserRatesPage>("activity/rating/", ratingObj);


                return true;
            }
            catch (Exception e)
            {
                LogsController.WriteException("SendRating()", e.Message);
            }
            return false;
        }
        public static bool ReceiveRating(string page, out int rating, out double avgrating)
        {
            UserRatedPage results = null;
            HttpQueryString vars;
            string user;

            if (!String.IsNullOrWhiteSpace(page) && Controller.UseServer && Settings.IsLoggedIn())
            {
                try
                {

                    vars = new HttpQueryString();
                    vars.Add("page", HttpUtility.UrlDecode(page));

                    user = Convert.ToString(Settings.UserID);
                    results = ServerRequestManager.Get<UserRatedPage>(new Uri("activity/rating/user/" + user + "/", UriKind.Relative), vars);

                    if (results != null)
                    {
                        avgrating = results.Page.AvgRating;
                        rating = results.Rating;
                        return true;
                    }
                }
                catch (Exception e)
                {
                    LogsController.WriteException("ReceiveRating()", e.Message);
                }
            }
            avgrating = 0.0;
            rating = 0;
            return false;

        }

        public static bool SendSharedFile(SharedFile userSharesFile)
        {
            if (Controller.UseServer && Settings.IsLoggedIn())
            {
                try
                {
                    ServerRequestManager.PostWithoutResponse<SharedFile>("activity/share/files/create/", userSharesFile);
                    return true;
                }
                catch (Exception e)
                {
                    LogsController.WriteException("SendShare()", e.Message);
                }
            }
            return false;
        }

        public static ActivityList ReceiveActivityList(int page)
        {
            ActivityList results = null;
            if (Controller.UseServer && Settings.IsLoggedIn())
            {
                try
                {
                    results = ServerRequestManager.Get<ActivityList>(new Uri("activity/news/page/" + Convert.ToString(page) + "/", UriKind.Relative));
                }
                catch (Exception e)
                {
                    LogsController.WriteException("ReceiveActivityList()", e.Message);
                }
            }
            return results ?? new ActivityList();

        }


       

        public static int ReceiveActivityListCount()        // NOT USED YET
        {
            ActivityCount results = null;
            if (Controller.UseServer && Settings.IsLoggedIn())
            {

                try
                {
                    string user = Convert.ToString(Settings.UserID);
                    results = ServerRequestManager.Get<ActivityCount>(new Uri("activity/news/user/" + user + "/count/", UriKind.Relative));
                }
                catch (Exception e)
                {
                    LogsController.WriteException("ReceiveActivityListCount()", e.Message);
                }
            }
            return (results != null) ? results.Count : 0;
        }

        public static List<int> ReceiveCachedLinks(List<int> links)
        {
            List<int> listOut = null;

            if (Controller.UseServer && Controller.UseDataService && links.Any())
            {
                try
                {
                    listOut = ServerRequestManager.Post<List<int>, List<int>>("search/links/", links);
                }
                catch (Exception e)
                {
                    LogsController.WriteException("ReceiveCachedLinks()", e.Message);
                }
            }

            return listOut;

        }

        public static int SendExplicitRecommendation(RequestParameters pars)
        {
            int ret = 0; // 0 nepodarilo sa, 1 nova recomm, 2 edit recomm

            UserRecommendsPage recommendObj;
            if (Controller.UseServer && Settings.IsLoggedIn())
            {
                Regex rreg = new Regex(@"[\r\n\t\'""]", RegexOptions.Compiled);

                try
                {
                    recommendObj = new UserRecommendsPage();

                   string x;
                    x = HttpUtility.UrlDecode(pars.GetValue("userGroup"));

                    recommendObj.GroupName = x;
                    recommendObj.Recommendation.IsSet = true;
                    string val = pars.GetValue("page");
                    recommendObj.Page.AbsoluteURI = HttpUtility.UrlDecode(val);
                    recommendObj.Page.Title = rreg.Replace(HttpLocalResponder.GetPageTitle(recommendObj.Page.AbsoluteURI), " "); 
                    recommendObj.User.Id = Settings.UserID;
                  
                    /* val = pars.GetValue("group");
                    recommendObj.Recommendation.Group.Name = val;*/

                    val = pars.GetValue("title");
                    val = rreg.Replace(HttpUtility.UrlDecode(val), " ");
                    val = (val.Length > 70) ? val.Substring(0, 70) : val;
                    recommendObj.Recommendation.Title = val;
                    val = pars.GetValue("desc");
                    val = rreg.Replace(HttpUtility.UrlDecode(val)," ");
                    val = (val.Length > 300) ? val.Substring(0, 300) : val;
                    recommendObj.Recommendation.Description = val;

                    ret = ServerRequestManager.Post<UserRecommendsPage, int>("activity/recommend/", recommendObj);
                    return ret;
                }
                catch (Exception e)
                {
                    LogsController.WriteException("SendRecommendation()", e.Message);
                }
            }
            return ret;
        }

        public static bool DeleteExplicitRecommendation(string pid, string gid)
        {
            if (Controller.UseServer && Settings.UserTeacher)
            {
                try
                {
                    int id = Convert.ToInt32(pid);
                    int group = Convert.ToInt32(gid);
                    return ServerRequestManager.Delete<bool>(new Uri("activity/recommend/group/" + group + "/page/" + id + "/", UriKind.Relative));
                }
                catch (Exception e)
                {
                    LogsController.WriteException("DeleteRecommendation()", e.Message);
                }
            }
            return false;
        }
        public static bool DeleteHistory(string pid)
        {
            if (Controller.UseServer && Settings.IsLoggedIn())
            {
                try
                {
                    int id = Convert.ToInt32(pid);
                    return ServerRequestManager.Delete<bool>(new Uri("history/user/" + Settings.UserID + "/page/" + id + "/", UriKind.Relative));
                }
                catch (Exception e)
                {
                    LogsController.WriteException("DeleteHistory()", e.Message);
                }
            }
            return false;
        }
      
        public static UserRecommendedPage ReceiveExplicitRecommendation(string page)
        {

            UserRecommendedPage recommendObj = null;
            HttpQueryString vars;
            if (Controller.UseServer)
            {
                try
                {
                    vars = new HttpQueryString();
                    vars.Add("page", page);

                    recommendObj = ServerRequestManager.Get<UserRecommendedPage>(new Uri("activity/recommend/", UriKind.Relative), vars);
                }
                catch (Exception e)
                {
                    LogsController.WriteException("ReceiveRecommendation()", e.Message);
                }
            }
            return recommendObj;
        }
      
        //public static UserRecommendations ReceiveExplicitRecommendationsMostRecent()
        //{

        //    UserRecommendations recommendObj = null;
        //    if (Controller.UseServer)
        //    {
        //        try
        //        {
        //            recommendObj = ServerRequestManager.Get<UserRecommendations>(new Uri("activity/recommend/recent/", UriKind.Relative));
        //        }
        //        catch (Exception e)
        //        {
        //            LogsController.WriteException("ReceiveExplicitRecommendationMostRecent()", e.Message);
        //        }
        //    }
        //    return recommendObj;
        //}

        public static UserRecommendations ReceiveExplicitRecommendationsMostRecentWithVisitInformation()
        {

            UserRecommendations recommendObj = null;
            if (Controller.UseServer && Settings.IsLoggedIn())
            {
                try
                {
                    recommendObj = ServerRequestManager.Get<UserRecommendations>(new Uri("activity/recommend/recent/user/"+Convert.ToString(Settings.UserID)+"/", UriKind.Relative));
                }
                catch (Exception e)
                {
                    LogsController.WriteException("ReceiveExplicitRecommendationMostRecent()", e.Message);
                }
            }
            return recommendObj;
        }
        //public static UserRecommendations ReceiveExplicitRecommendationsInGroup(string group, int page)
        //{

        //    UserRecommendations recommendObj = null;
        //    if (Controller.UseServer)
        //    {
        //        try
        //        {
        //            recommendObj = ServerRequestManager.Get<UserRecommendations>(new Uri("activity/recommend/group/" + group + "/page/" + Convert.ToString(page) + "/", UriKind.Relative));
        //        }
        //        catch (Exception e)
        //        {
        //            LogsController.WriteException("ReceiveExplicitRecommendationInGroup()", e.Message);
        //        }
        //    }
        //    return recommendObj;
        //}

        public static UserRecommendations ReceiveExplicitRecommendationsInGroupWithVisitInformation(string group, int page)
        {

            UserRecommendations recommendObj = null;
            if (Controller.UseServer && Settings.IsLoggedIn())
            {
                try
                {
                    recommendObj = ServerRequestManager.Get<UserRecommendations>(new Uri("activity/recommend/group/" + group + "/page/" + Convert.ToString(page) + "/user/" + Convert.ToString(Settings.UserID) + "/", UriKind.Relative));
                }
                catch (Exception e)
                {
                    LogsController.WriteException("ReceiveExplicitRecommendationInGroup()", e.Message);
                }
            }
            return recommendObj;
        }
        public static RegisteredUsers ReceiveCompleteUserlist(int page)
        {
            RegisteredUsers reg = null;
            if (Controller.UseServer)
            {
                try
                {
                    reg = ServerRequestManager.Get<RegisteredUsers>(new Uri("user/list/page/" + Convert.ToString(page) + "/", UriKind.Relative));
                }
                catch (Exception e)
                {
                    LogsController.WriteException("ReceiveCompleteUserlist()", e.Message);
                }
            }
            return reg;
        }
        public static RegisteredUsers ReceiveUserlist(string group, int page)
        {
            RegisteredUsers reg = null;
            if (Controller.UseServer)
            {
                try
                {
                    reg = ServerRequestManager.Get<RegisteredUsers>(new Uri("user/list/group/" + group + "/page/" + Convert.ToString(page) + "/", UriKind.Relative));
                }
                catch (Exception e)
                {
                    LogsController.WriteException("ReceiveUserlist()", e.Message);
                }
            }
            return reg;
        }

        public static RatedPages ReceiveAllTopRated(int page)
        {
            RatedPages ret = null;
            if (Controller.UseServer)
            {
                try
                {
                    ret = ServerRequestManager.Get<RatedPages>(new Uri("activity/top/all/page/" + Convert.ToString(page) + "/", UriKind.Relative));
                }
                catch (Exception e)
                {
                    LogsController.WriteException("ReceiveAllTopRated()", e.Message);
                }
            }
            return ret ?? new RatedPages();
        }

        public static RatedPages ReceiveUserTopRated(int page)
        {
            RatedPages ret = null;
            string user;
            if (Controller.UseServer && Settings.IsLoggedIn())
            {
                try
                {
                    user = Convert.ToString(Settings.UserID);
                    ret = ServerRequestManager.Get<RatedPages>(new Uri("activity/top/user/" + user + "/page/" + Convert.ToString(page) + "/", UriKind.Relative));
                }
                catch (Exception e)
                {
                    LogsController.WriteException("ReceiveUserTopRated()", e.Message);
                }
            }
            return ret ?? new RatedPages();
        }

        public static VisitedPages ReceiveUserHistory(string period, int page)
        {
            VisitedPages ret = null;
            string user;
            if (Controller.UseServer && Settings.IsLoggedIn())
            {
                try
                {
                    user = Convert.ToString(Settings.UserID);
                    ret = ServerRequestManager.Get<VisitedPages>(new Uri("history/period/" + period + "/user/" + user + "/page/" + Convert.ToString(page) + "/", UriKind.Relative));
                }
                catch (Exception e)
                {
                    LogsController.WriteException("ReceiveUserHistory()", e.Message);
                }
            }
            return ret ?? new VisitedPages();
        }

        public static List<string> SendTags(RequestParameters pars)
        {
            UserTagsPage pObj;
            PageObjectWithTags results = null;
            string val;
            if (Controller.UseServer && Settings.IsLoggedIn())
            {
                try
                {
                    pObj = new UserTagsPage();
                    pObj.User.Id = Settings.UserID;
                    val = pars.GetValue("page");
                    pObj.Page.AbsoluteURI = HttpUtility.UrlDecode(val);
                    pObj.Page.Title = rreg.Replace(HttpLocalResponder.GetPageTitle(val), " ");
                    val = pars.GetValue("tags");
                    string[] tags = HttpUtility.UrlDecode(val).Split(new char[] {' '} , StringSplitOptions.RemoveEmptyEntries);
                    foreach (string tag in tags)
                    {
                        string t = System.Text.RegularExpressions.Regex.Replace(tag, @"[\^\$\W\.@-]", "");
                        if (!String.IsNullOrWhiteSpace(t))
                        {
                            pObj.Page.Tags.Add(t);
                        }
                    }
                    if (pObj.Page.Tags.Any()) 
                    {
                        results = ServerRequestManager.Post<UserTagsPage, PageObjectWithTags>("search/tag/list/", pObj);
                    }
                }
                catch (Exception e)
                {
                    LogsController.WriteException("SendTags()", e.Message);
                }
            }
            return results != null ? results.Tags : new List<string>();
        }

        public static List<string> ReceiveTags(string page)
        {
            PageObjectWithTags results = null;
            HttpQueryString vars;
            if (!String.IsNullOrWhiteSpace(page) && Controller.UseServer)
            {
                try
                {
                    vars = new HttpQueryString();
                    vars.Add("page", page);

                    results = ServerRequestManager.Get<PageObjectWithTags>(new Uri("search/tag/list/", UriKind.Relative), vars);
                }
                catch (Exception e)
                {
                    LogsController.WriteException("ReceiveTags()", e.Message);
                }
            }
            
            return results != null ? results.Tags : new List<string>();
        }
        
        public static TagCloud ReceiveTagCloud()
        {
            TagCloud results = null;
            if (Controller.UseServer)
            {
                try
                {
                    results = ServerRequestManager.Get<TagCloud>(new Uri("search/tag/cloud/", UriKind.Relative));
                }
                catch (Exception e)
                {
                    LogsController.WriteException("ReceiveTagCloud()", e.Message);
                }
            }
            return results ?? new TagCloud();
        }

        public static void Ping()
        {
            ServerSettings settings = null;
            if (Controller.UseServer)
            {
                try
                {
                    settings = ServerRequestManager.Get<ServerSettings>(new Uri("cache/ping/?clientName=" + Settings.ClientName, UriKind.Relative));
                }
                catch (Exception ex)
                {
                    LogsController.WriteException("Ping", ex.Message);
                }

                if (settings != null)
                {
                    Settings.Update(settings);
                    AppHelpers.ShowInformation("Server reached succesfully.");
                }
                else
                {
                    AppHelpers.ShowError("Could not reach server.");
                }
            }
            else
            {
                AppHelpers.ShowWarning("You need to set up your connection to server first.");
            }
        }


        public static void SendUsage(ActivityType activityType, RequestParameters pars)
        {
            if (!Controller.UseServer || !Settings.IsLoggedIn()) return;

            string uri = null;
            UserVisitsPage pobj;

            try
            {
                uri = pars.GetValue("page");
                uri = HttpUtility.UrlDecode(uri);
                if (!String.IsNullOrWhiteSpace(uri))
                {
                    pobj = new UserVisitsPage();
                    pobj.User.Id = Settings.UserID;
                    pobj.Page.AbsoluteURI = uri;
                    pobj.Reason = activityType;

                    ServerRequestManager.PostWithoutResponse<UserVisitsPage>("activity/usage/", pobj);
                }
            }
            catch (Exception e)
            {
                LogsController.WriteException("SendUsage()", e.Message);
            }
        }

        public static void SendUsageById(ActivityType activityType, RequestParameters pars)
        {
            if (!Controller.UseServer || !Settings.IsLoggedIn()) return;

            string id = null;
            UserVisitsPage pobj;

            try
            {
                id = pars.GetValue("page");
                id = HttpUtility.UrlDecode(id);
                int pid = Convert.ToInt32(id);
                pobj = new UserVisitsPage();
                pobj.User.Id = Settings.UserID;
                pobj.Page.AbsoluteURI = "";
                pobj.Page.Id = pid;
                pobj.Reason = activityType;

                ServerRequestManager.PostWithoutResponse<UserVisitsPage>("activity/usage/", pobj);
            }
            catch (Exception e)
            {
                LogsController.WriteException("SendUsage()", e.Message);
            }
        }
        /*
        public static string UpdateUserRole(string rid)
        {
            if (!Controller.UseServer || !Settings.IsTeacher()) return null;

            UserId pobj;
            try
            {
                int id = Convert.ToInt32(rid);
                 
                if (id > 0)
                {
                    pobj = new UserId() { Id = id };
                    User user = ServerRequestManager.Put<UserId, User>("user/role/", pobj);
                    if (user.Id > 0) 
                        return user.IsTeacher ? "Teacher" : "Student";
                }
            }
            catch (Exception e)
            {
                ConsoleManager.WriteException("UpdateSwitchRole()", e.Message);
            }
            return null;
        }
        */
        public static SharedFolder GetSharedFolderStructure()
        {
            SharedFolder rootFolder = null;
            try
            {
                if (Controller.UseServer)
                {
                    rootFolder = ServerRequestManager.Get<SharedFolder>(new Uri("activity/share/folders/", UriKind.Relative));
                }
            }
            catch (Exception e)
            {
                    LogsController.WriteException("GetSharedFolderStructure()", e.Message);
            }
            return rootFolder;
        }

        public static SharedFolder GetSharedFolder(int id)
        {
            SharedFolder folder = null;
            try
            {
                if (Controller.UseServer)
                {
                    folder = ServerRequestManager.Get<SharedFolder>(new Uri(String.Format("activity/share/folders/{0}/", id), UriKind.Relative));
                }
            }
            catch (Exception e)
            {
                LogsController.WriteException("GetSharedFolderStructure()", e.Message);
            }
            return folder;
        }
 
        public static bool DeleteUser(string rid)
        {
            if (Controller.UseServer)
            {
                try
                {
                    int id = Convert.ToInt32(rid);
                    return ServerRequestManager.Delete<bool>(new Uri("user/" + id, UriKind.Relative));
                }
                catch (Exception e)
                {
                    LogsController.WriteException("DeleteUser()", e.Message);
                }
            }
            return false;
        }
        
        public static SharedFolder CreateFolder(int parentId, string name)
        {
            SharedFolder folder = new SharedFolder()
            {
                Id = -1,
                Name = name,
                ParentFolderId = parentId
            };
            if (Controller.UseServer)
            {
                try
                {
                    folder = ServerRequestManager.Post<SharedFolder, SharedFolder>("activity/share/folders/create/", folder);
                }
                catch (Exception e)
                {
                    LogsController.WriteException("CreateFolder()", e.Message);
                    return null;
                }
            }
            return folder;
        }

        public static bool RenameFolder(int id, string name)
        {
            if (Controller.UseServer)
            {
                try
                {
                    SharedFolder folder = new SharedFolder()
                    {
                        Id = id,
                        Name = name
                    };
                    ServerRequestManager.PostWithoutResponse<SharedFolder>("activity/share/folders/rename/",folder);
                }
                catch (Exception ex)
                {
                    LogsController.WriteException("RenameFolder()", ex.Message);
                    return false;
                }
            }

            return true;
        }

        public static bool DeleteFolder(int id)
        {
            if (Controller.UseServer)
            {
                try
                {
                    SharedFolder folder = new SharedFolder()
                    {
                        Id = id
                    };
                    ServerRequestManager.PostWithoutResponse<SharedFolder>("activity/share/folders/delete/",folder);
                }
                catch (Exception ex)
                {
                    LogsController.WriteException("DeleteFolder()", ex.Message);
                    return false;
                }
            }

            return true;
        }

        public static bool DeleteFile(int id)
        {
            if (Controller.UseServer)
            {
                try
                {
                    ServerRequestManager.DeleteWithoutResponse(new Uri(String.Format("activity/share/files/{0}/delete/", id), UriKind.Relative));
                }
                catch (Exception ex)
                {
                    LogsController.WriteException("DeleteFile()", ex.Message);
                    return false;
                }
            }
            return true;
        }

        public static User ReceiveUserInformation(string uid)
        {
            if (!Controller.UseServer) return null;

            try
            {
                int id = Convert.ToInt32(uid);

                if (id > 0 && ((!Settings.UserTeacher && id == Settings.UserID) || Settings.UserTeacher))
                {
                   User user = ServerRequestManager.Get<User>(new Uri("user/" + id + "/", UriKind.Relative));
                   return user;
                }
            }
            catch (Exception e)
            {
                LogsController.WriteException("ReceiveUserInformation()", e.Message);
            }
            return null;
        }

        public static bool DeleteActivity(string id)
        {
            if (Controller.UseServer && Settings.UserTeacher)
            {
                try
                {
                    int k = Convert.ToInt32(id);
                    return ServerRequestManager.Delete<bool>(new Uri("activity/news/" + id + "/", UriKind.Relative));
                }
                catch (Exception e)
                {
                    LogsController.WriteException("DeleteActivity()", e.Message);
                }
            }
            return false;
        }
        public static bool DeleteTag(string tag)
        {
            if (Controller.UseServer && Settings.UserTeacher)
            {
                try
                {
                    return ServerRequestManager.Delete<bool>(new Uri("search/tag/" + tag + "/", UriKind.Relative));
                }
                catch (Exception e)
                {
                    LogsController.WriteException("DeleteTag()", e.Message);
                }
            }
            return false;
        }

        public static ActivityList GetLiveStream()
        {
            ActivityList results = null;
            if (Controller.UseServer && Settings.IsLoggedIn())
            {
                try
                {
                    results = ServerRequestManager.Get<ActivityList>(new Uri("activity/live/stream/", UriKind.Relative));
                }
                catch (Exception e)
                {
                    LogsController.WriteException("GetLiveStream()", e.Message);
                }
            }
            return results ?? new ActivityList();
        }

        public static ActivityList GetLiveStream(string since)
        {
            ActivityList results = null;
            if (Controller.UseServer && Settings.IsLoggedIn())
            {
                try
                {
                    results = ServerRequestManager.Get<ActivityList>(new Uri("activity/live/stream?since=" + since, UriKind.Relative));
                }
                catch (Exception e)
                {
                    LogsController.WriteException("GetLiveStream()", e.Message);
                }
            }
            return results ?? new ActivityList();
        }

        public static bool CreateGroup(string name, string description, List<string> tags, string location)
        {
            if (Controller.UseServer && Settings.IsLoggedIn())
            {
                ServiceEntities.UserGroup userGroup = new ServiceEntities.UserGroup()
                {
                    Name = name,
                    Description = description,
                    DateCreated = DateTime.Now,
                    UserId = Settings.UserID,
                    Tags = tags
                };

                if (location == "local")
                    userGroup.Location = false;
                else
                    userGroup.Location = true;

                try
                {
                    return ServerRequestManager.Post<ServiceEntities.UserGroup, bool>("groups/create/", userGroup);
                }
                catch (Exception ex)
                {
                    LogsController.WriteException("createGroup()", ex.Message);
                }
            }
            return false;
        }



        public static GroupsList GetUserGroupsList()
        {
            GroupsList results = null;
            if (Controller.UseServer && Settings.IsLoggedIn())
            {
                try
                {
                    results = ServerRequestManager.Get<GroupsList>(new Uri("groups/my/" + Convert.ToString(Settings.UserID)+"/", UriKind.Relative));
                }
                catch (Exception e)
                {
                    LogsController.WriteException("GetUserGroupsList()", e.Message);
                }
            }
            return results ?? new GroupsList();
        }

        public static GroupsList GetUserGroupsListAll()
        {
            GroupsList results = null;
            if (Controller.UseServer && Settings.IsLoggedIn())
            {
                try
                {
                    results = ServerRequestManager.Get<GroupsList>(new Uri("groups/all/", UriKind.Relative));
                }
                catch (Exception e)
                {
                    LogsController.WriteException("GetUserGroupsList()", e.Message);
                }
            }
            return results ?? new GroupsList();
        }


        public static ServiceEntities.Group GetGroup(int id)
        {
            ServiceEntities.Group results = null;
            if (Controller.UseServer && Settings.IsLoggedIn())
            {
                string group_id = Convert.ToString(id);
                try
                {
                    results = ServerRequestManager.Get<ServiceEntities.Group>(new Uri("groups/show/" + group_id + "/user/" + Settings.UserID + "/", UriKind.Relative));
                }
                catch (Exception e)
                {
                    LogsController.WriteException("GetGroup()", e.Message);
                }
            }
            return results ?? new ServiceEntities.Group();
        }





        public static void JoinGroup(ServiceEntities.IdUG idUG)
        {
            if (Controller.UseServer && Settings.IsLoggedIn())
            {


                try
                {
                    ServerRequestManager.PostWithoutResponse<ServiceEntities.IdUG>("groups/join/", idUG);
                }
                catch (Exception ex)
                {
                    LogsController.WriteException("joinGroup()", ex.Message);
                }
            }
        }


        public static void LeaveGroup(ServiceEntities.IdUG idUG)
        {
            if (Controller.UseServer && Settings.IsLoggedIn())
            {


                try
                {
                    ServerRequestManager.PostWithoutResponse<ServiceEntities.IdUG>("groups/leave/", idUG);
                }
                catch (Exception ex)
                {
                    LogsController.WriteException("LeaveGroup()", ex.Message);
                }
            }
        }


        public static void DeleteGroup(int group_id)
        {
            if (Controller.UseServer && Settings.IsLoggedIn())
            {


                try
                {
                    ServerRequestManager.Delete<bool>(new Uri("groups/delete/" + Convert.ToString(group_id) + "/", UriKind.Relative));
                }
                catch (Exception ex)
                {
                    LogsController.WriteException("DeleteGroup()", ex.Message);
                }
            }
        }


        public static UserRecommendations ReceiveGroupRecommendations(string group, string page)
        {

            UserRecommendations recommendObj = null;
            if (Controller.UseServer && Settings.IsLoggedIn())
            {
                try
                {
                    recommendObj = ServerRequestManager.Get<UserRecommendations>(new Uri("groups/recommend/group/" + group + "/page/" + Convert.ToString(page) + "/user/" + Convert.ToString(Settings.UserID) + "/", UriKind.Relative));
                    
                }
                catch (Exception e)
                {
                    LogsController.WriteException("ReceiveExplicitRecommendationInGroup()", e.Message);
                }
            }
            return recommendObj;

            // aktivity 
            /*ActivityList results = null;
            if (Controller.UseServer && Settings.IsLoggedIn())
            {
                try
                {
                    results = ServerRequestManager.Get<ActivityList>(new Uri("groups/recommended/"+group+"/page/" + page + "/", UriKind.Relative));
                }
                catch (Exception e)
                {
                    LogsController.WriteException("ReceiveActivityList()", e.Message);
                }
            }
            return results ?? new ActivityList();*/

        }


        public static List<ServiceEntities.AutocompletedGroup> GetGroupAutocomplete(string term)
        {

            List<ServiceEntities.AutocompletedGroup> recommendObj = null;
            if (Controller.UseServer && Settings.IsLoggedIn())
            {
                try
                {
                    recommendObj = ServerRequestManager.Get<List<ServiceEntities.AutocompletedGroup>>(new Uri("groups/autocomplete/" + term + "/", UriKind.Relative));

                }
                catch (Exception e)
                {
                    LogsController.WriteException("ReceiveExplicitRecommendationInGroup()", e.Message);
                }
            }
            return recommendObj;

        }

        public static ServiceEntities.GroupsList GetLastUsedGroups()
        {

            ServiceEntities.GroupsList Gl = new GroupsList();

            if (Controller.UseServer && Settings.IsLoggedIn())
            {
                try
                {
                    Gl = ServerRequestManager.Get<ServiceEntities.GroupsList>(new Uri("groups/lastused/" + Convert.ToString(Settings.UserID) + "/", UriKind.Relative));

                }
                catch (Exception e)
                {
                    LogsController.WriteException("GetLastUsedGroups()", e.Message);
                }
            }
            return Gl;

        }


        public static bool isInGroup(int uId, int gId)
        {

            bool isIn = false ;

            if (Controller.UseServer && Settings.IsLoggedIn())
            {
                try
                {
                    isIn = ServerRequestManager.Get<bool>(new Uri("groups/isIn/" + Convert.ToString(uId) + "/" + Convert.ToString(gId) + "/", UriKind.Relative));

                }
                catch (Exception e)
                {
                    LogsController.WriteException("isInGroup()", e.Message);
                }
            }
            return isIn;

        }


        public static bool isAdmin(int uId, int gId)
        {

            bool isA = false;

            if (Controller.UseServer && Settings.IsLoggedIn())
            {
                try
                {
                    isA = ServerRequestManager.Get<bool>(new Uri("groups/isAdmin/" + Convert.ToString(uId) + "/" + Convert.ToString(gId) + "/", UriKind.Relative));

                }
                catch (Exception e)
                {
                    LogsController.WriteException("isInGroup()", e.Message);
                }
            }
            return isA;

        }

        public static List<ServiceEntities.User> groupUsers(int gId)
        {
            List<ServiceEntities.User> users = new List<ServiceEntities.User>();

            if (Controller.UseServer && Settings.IsLoggedIn())
            {
                try
                {
                    users = ServerRequestManager.Get<List<ServiceEntities.User>>(new Uri("groups/getUsers/" + Convert.ToString(gId) + "/", UriKind.Relative));

                }
                catch (Exception e)
                {
                    LogsController.WriteException("groupUsers()", e.Message);
                }
            }
            return users;
        }

        internal static List<Recommendation> GetPreviousRecommendationsForPage(string url)
        {
            List<Recommendation> recommendations = new List<Recommendation>();

            if (Controller.UseServer && Settings.IsLoggedIn())
            {
                try
                {
                    recommendations = ServerRequestManager.Post<PageObject, List<Recommendation>>("groups/previousrecommendations/", new PageObject()
                    {
                        AbsoluteURI = url
                    });
                }
                catch (Exception e)
                {
                    LogsController.WriteException("groupUsers()", e.Message);
                }
            }
            return recommendations;
        }

        internal static bool ValidateName(string name)
        {
            return ServerRequestManager.Get<bool>(new Uri("groups/get_group_by_name/" + name + "/", UriKind.Relative));
        }

        internal static int ReceiveCountNotDisplayedRecomms()
        {
            try
            {
                int x = ServerRequestManager.Get<int>(new Uri("activity/recommend/not_displayed/" + Settings.UserID + "/", UriKind.Relative));
                return x;
            }
            catch (Exception e)
            {
                LogsController.WriteException("receiveCountDisplayedRecomms", e.Message);
            }

            return 0;
        }
    }
}
