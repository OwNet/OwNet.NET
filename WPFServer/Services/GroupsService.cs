using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using WPFServer.DatabaseContext;
using ClientAndServerShared;

namespace WPFServer
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(UseSynchronizationContext = false, InstanceContextMode = InstanceContextMode.PerCall)]
    public class GroupsService : IGroupsService
    {

        private static int itemsPerPage = 10;

        [WebInvoke(UriTemplate = "create/", Method = "POST")]
        public bool CreateGroup(ServiceEntities.UserGroup userGroup)
        {
            try
            {
                using (MyDBContext con = new MyDBContext())
                {
                    /* selectne usera */
                    IQueryable<User> users = con.Fetch<User>(u => u.Id == userGroup.UserId);
                    if (!users.Any())
                        return false;
                    User user = users.First();
                    int id = string.Format("{0}-{1}", userGroup.Name, Helpers.Common.FullDateTimeToString(DateTime.Now)).GetHashCode();

                    Group group = new Group()
                    {
                        Name = userGroup.Name,
                        Description = userGroup.Description,
                        DateCreated = DateTime.Now,
                        Location = userGroup.Location,
                        Id = id
                    };

                    IQueryable<Group> groups = con.Fetch<Group>(group);
                    if (groups.Any()) return false;

                    Group groupItem = con.FetchOrCreate<Group>(group);

                    UserGroup userGroupItem = con.FetchOrCreate<UserGroup>(new UserGroup()
                    {
                        Group = groupItem,
                        User = user,
                        LastUsed = new DateTime(1970, 1, 1)
                    });

                    AdminGroup adminGroupItem = con.FetchOrCreate<AdminGroup>(new AdminGroup()
                    {
                        Group = groupItem,
                        User = user
                    });

                    foreach (string tag in userGroup.Tags)
                    {
                        con.FetchOrCreate<GroupTag>(new GroupTag()
                        {
                            Value = tag,
                            DateCreated = DateTime.Now,
                            Group = groupItem
                        });

                    }

                    Activity act = con.CreateActivityItem(DatabaseContext.ActivityType.Group, DatabaseContext.ActivityAction.Create, group.Name, user, true, null, null, group);
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
            return true;
        }

        [WebGet(UriTemplate = "my/{user_id}/")]
        public ServiceEntities.GroupsList GetGroups(string user_id)
        {

            ServiceEntities.GroupsList groups = new ServiceEntities.GroupsList();
            
            try
            {
                using (MyDBContext con = new MyDBContext())
                {
                    int conID = Convert.ToInt32(user_id);
                    IQueryable<User> users = con.Fetch<User>(u => u.Id == conID);
                    if (users.Any())
                    {
                        User user = users.First();
                        


                        
                        foreach (UserGroup userGroup in user.UserGroups)
                        {
                            Group group = userGroup.Group;

                            groups.Add(new ServiceEntities.Group()
                            {
                                Name = group.Name,
                                Description = group.Description,
                                Id = group.Id,
                                DateCreated = group.DateCreated
                            });
                        }
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


            return groups;
        }


        [WebGet(UriTemplate = "all/")]
        public ServiceEntities.GroupsList GetAllGroups()
        {

            ServiceEntities.GroupsList groups = new ServiceEntities.GroupsList();

            try
            {
                using (MyDBContext con = new MyDBContext())
                {

                    IQueryable<Group> gs = con.FetchSet<Group>();

                    foreach (var g in gs)
                    {
                        Group group = g;

                        groups.Add(new ServiceEntities.Group()
                        {
                            Name = group.Name,
                            Description = group.Description,
                            Id = group.Id,
                            DateCreated = group.DateCreated
                        });
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


            return groups;
        }



        /* join group */
        [WebInvoke(UriTemplate = "join/", Method = "POST")]
        public void JoinGroup(ServiceEntities.IdUG idUG)
        {
            try
            {
                using (MyDBContext con = new MyDBContext())
                {
                    /* selectne usera */
                    IQueryable<User> users = con.Fetch<User>(u => u.Id == idUG.UserId);
                    if (!users.Any())
                        return;
                    User user = users.First();


                    /* selectne groupu */
                    IQueryable<Group> groups = con.Fetch<Group>(g => g.Id == idUG.GroupId);
                    if (!groups.Any())
                        return;
                    Group group = groups.First();

                    /* overenie ci user uz nie je v skupine */
                    IQueryable<UserGroup> userGroup = con.Fetch<UserGroup>(g => g.GroupId == group.Id && g.UserId == user.Id);
                    if (userGroup.Any())
                        return;

                    UserGroup userGroupItem = new UserGroup()
                    {
                        Group = group,
                        User = user,
                        LastUsed = DateTime.Now
                    };

                    //zaznamy do db
                    userGroupItem = con.FetchOrCreate<UserGroup>(userGroupItem);

                    Activity act = con.CreateActivityItem(DatabaseContext.ActivityType.Group, DatabaseContext.ActivityAction.Update, group.Name, user, true, null, null, group);
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



        /* join group */
        [WebInvoke(UriTemplate = "leave/", Method = "POST")]
        public void LeaveGroup(ServiceEntities.IdUG idUG)
        {
            try
            {
                using (MyDBContext con = new MyDBContext())
                {
                    /* selectne usera */
                   

                    /* overenie ci user uz nie je v skupine */
                    IQueryable<UserGroup> userGroup = con.Fetch<UserGroup>(g => g.GroupId == idUG.GroupId && g.UserId == idUG.UserId);
                    if (!userGroup.Any())
                        return;

                    //vymazat z db
                    con.Remove<DatabaseContext.UserGroup>(userGroup.First());


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



        [WebGet(UriTemplate = "show/{group_id}/user/{user_id}/")]
        public ServiceEntities.Group GetGroup(string group_id, string user_id)
        {
            ServiceEntities.Group group = new ServiceEntities.Group();

            try
            {
                using (MyDBContext con = new MyDBContext())
                {
                    int conID = Convert.ToInt32(group_id);
                    IQueryable<Group> gr = con.Fetch<Group>(g => g.Id == conID);
                    if (gr.Any())
                    {
                        Group groupS = gr.First();
                        group.Id = groupS.Id;
                        group.Name = groupS.Name;
                        group.Description = groupS.Description;
                        group.Location = groupS.Location;

                        foreach (GroupTag groupTag in groupS.GroupTags)
                        {
                            group.Tags.Add(groupTag.Value);
                        }

                    }

                    int uId = Convert.ToInt32(user_id);
                    int gId = Convert.ToInt32(group_id);

                    IQueryable<UserGroup> ugs = con.Fetch<UserGroup>(gg => gg.UserId == uId && gg.GroupId == gId);
                    if (ugs.Any())
                    {
                        var ug = ugs.First();

                        ug.LastUsed = DateTime.Now;

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
                throw new WebFaultException<ServiceEntities.ErrorHandler>(new ServiceEntities.ErrorHandler { cause = e.Message, inner = e.InnerException != null ? e.InnerException.Message : "", errorCode = 500 }, System.Net.HttpStatusCode.InternalServerError);
            }

            return group;
        }


        [WebInvoke(UriTemplate = "delete/{group_id}/", Method = "DELETE")]
        public bool  DeleteGroup(string group_id)
        {
            try
            {
                using (DatabaseContext.MyDBContext con = new DatabaseContext.MyDBContext())
                {
                    int id = Convert.ToInt32(group_id);
                    try
                    {
                        IQueryable<Group> groups = con.Fetch<Group>(g => g.Id == id);
                        if (groups.Any())
                        {
                            DatabaseContext.Group groupItem = groups.First();
                            while (groupItem.Activities.Any())
                            {
                                con.FetchSet<Activity>().Remove(groupItem.Activities.First());
                            }
                            con.Remove<DatabaseContext.Group>(groupItem);
                            return true;
                        }
                    }
                    catch (Exception e)
                    {
                        throw new WebFaultException<ServiceEntities.ErrorHandler>(new ServiceEntities.ErrorHandler { cause = e.Message, inner = e.InnerException != null ? e.InnerException.Message : "", errorCode = 500 }, System.Net.HttpStatusCode.InternalServerError);
                    }
                }
            }
            catch (Exception e)
            {
                throw new WebFaultException<ServiceEntities.ErrorHandler>(new ServiceEntities.ErrorHandler { cause = e.Message, inner = e.InnerException != null ? e.InnerException.Message : "", errorCode = 500 }, System.Net.HttpStatusCode.InternalServerError);
            }
            return false;
        }

        [WebGet(UriTemplate = "recommend/group/{group}/page/{page}/user/{usersid}/")]
        public ServiceEntities.UserRecommendations GetAllActivities(string group, string page, string usersid)
        {
            ServiceEntities.UserRecommendations ret = new ServiceEntities.UserRecommendations();
            try
            {
                using (MyDBContext con = new MyDBContext())
                {
                    int pag = Convert.ToInt32(page);
                    int userid = Convert.ToInt32(usersid);
                    int groupId = Convert.ToInt32(group);
                    ServiceEntities.UserRecommendedPageWithRating urpwr = null;


                    try
                    {
                        IQueryable<User> users = con.Fetch<User>(u => u.Id == userid);
                        User user = null;
                        IQueryable<GroupRecommendation> recs = con.Fetch<GroupRecommendation>(g => g.GroupId == groupId);

                        IQueryable<UserGroup> ug = con.Fetch<UserGroup>(g => g.UserId == userid && g.GroupId == groupId);
                        
                        if (recs.Any() && users.Any() && ug.Any() )
                        {
                            user = users.First();

                            /* nastavenie casu zmeny */
                            var userGroup = ug.First();
                            userGroup.LastUsed = DateTime.Now;
                            userGroup.SaveChanges();


                            

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
                                urpwr.Recommendation.Group.Name = userGroup.Group.Name;
                                urpwr.Recommendation.Group.Id = userGroup.Group.Id;
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



        [WebGet(UriTemplate = "autocomplete/{term}/")]
        public List<ServiceEntities.AutocompletedGroup> AutocompletedGroups(string term)
        {
            List<ServiceEntities.AutocompletedGroup> ret = new List<ServiceEntities.AutocompletedGroup>();

            try
            {
                using (MyDBContext con = new MyDBContext())
                {
                    try
                    {
                        List<string> termList = term.Split(' ').ToList();
                        IQueryable<Group> groups = con.Fetch<Group>(g => termList.Any(t => g.Name.Contains(t)));

                        if (groups.Any())
                        {
                            foreach (Group group in groups)
                            {
                                ServiceEntities.AutocompletedGroup ag = new ServiceEntities.AutocompletedGroup();
                                ag.GroupId = group.Id;
                                ag.GroupName = group.Name;
                                ret.Add(ag);
                            }
                        }
                    }
                    catch (MyDBContextException e)
                    {
                        throw new WebFaultException<ServiceEntities.ErrorHandler>(new ServiceEntities.ErrorHandler { cause = e.Message, inner = e.InnerException != null ? e.InnerException.Message : "", errorCode = 500 }, System.Net.HttpStatusCode.InternalServerError);
                    }
                }
            }
            catch (Exception e)
            {
                throw new WebFaultException<ServiceEntities.ErrorHandler>(new ServiceEntities.ErrorHandler { cause = e.Message, inner = e.InnerException != null ? e.InnerException.Message : "", errorCode = 500 }, System.Net.HttpStatusCode.InternalServerError);
            }

            return ret;
        }

        /* zisti ci sa user nachadza *v skupine */
        [WebGet(UriTemplate = "isIn/{user_id}/{group_id}/")]
        public bool IsIn(string user_id, string group_id)
        {

            int uId = Convert.ToInt32(user_id);
            int gId = Convert.ToInt32(group_id);

            try
            {
                using (MyDBContext con = new MyDBContext()){
                    IQueryable<UserGroup> groups = con.Fetch<UserGroup>(ug => ug.UserId == uId && ug.GroupId == gId);
                    if (groups.Any())
                        return true;
                    else
                        return false;
                }
            }
            catch (MyDBContextException e)
            {
                throw new WebFaultException<ServiceEntities.ErrorHandler>(new ServiceEntities.ErrorHandler { cause = e.Message, inner = e.InnerException != null ? e.InnerException.Message : "", errorCode = 500 }, System.Net.HttpStatusCode.InternalServerError);
            }     

        }

        /* zisti ci je user admin skupiny*/
        [WebGet(UriTemplate = "isAdmin/{user_id}/{group_id}/")]
        public bool IsAdmin(string user_id, string group_id)
        {

            int uId = Convert.ToInt32(user_id);
            int gId = Convert.ToInt32(group_id);

            try
            {
                using (MyDBContext con = new MyDBContext())
                {
                    IQueryable<AdminGroup> groups = con.Fetch<AdminGroup>(ug => ug.UserId == uId && ug.GroupId == gId);
                    if (groups.Any())
                        return true;
                    else
                        return false;
                }
            }
            catch (MyDBContextException e)
            {
                throw new WebFaultException<ServiceEntities.ErrorHandler>(new ServiceEntities.ErrorHandler { cause = e.Message, inner = e.InnerException != null ? e.InnerException.Message : "", errorCode = 500 }, System.Net.HttpStatusCode.InternalServerError);
            }

        }


        [WebGet(UriTemplate = "getUsers/{group_id}/")]
        public List<ServiceEntities.User> GetUsers(string group_id)
        {

            List<ServiceEntities.User> users = new List<ServiceEntities.User>();

            int gId = Convert.ToInt32(group_id);

            try
            {
                using (MyDBContext con = new MyDBContext())
                {
                    IQueryable<UserGroup> ugs = con.Fetch<UserGroup>(ug => ug.GroupId == gId);

                    var UserObjects = from ug in ugs
                                           select ug.User;
                    if (UserObjects.Any())
                        foreach (User u in UserObjects)
                        {
                            var user = new ServiceEntities.User();
                            user.Email = u.Email;
                            user.Username = u.Username;
                            user.Firstname = u.Firstname;
                            user.Surname = u.Surname;
                            user.IsTeacher = u.IsTeacher;
                            users.Add(user);
                        }
                }
            }
            catch (MyDBContextException e)
            {
                throw new WebFaultException<ServiceEntities.ErrorHandler>(new ServiceEntities.ErrorHandler { cause = e.Message, inner = e.InnerException != null ? e.InnerException.Message : "", errorCode = 500 }, System.Net.HttpStatusCode.InternalServerError);
            }

            return users;
        }

        //adresa sluzby
        [WebGet(UriTemplate = "lastused/{user_id}/")]
        public ServiceEntities.GroupsList GetLastUsed(string user_id)
        {
            //list skupin
            ServiceEntities.GroupsList gl = new ServiceEntities.GroupsList();
            try
            {
                using (MyDBContext con = new MyDBContext())
                {
                    // id aktualneho pouzivatela
                    int userid = Convert.ToInt32(user_id);
                    try
                    {
                        // vyber 4 posledne pouzitych skupin daneho pouzivatela
                        IQueryable<UserGroup> uig = con.Fetch<UserGroup>(u => u.UserId == userid).OrderByDescending(g => g.LastUsed);
                        if (uig.Count() > 4)
                            uig = uig.Take(4);

                        var GroupObjects = from ug in uig
                                           where ug.UserId == userid
                                           orderby ug.LastUsed descending
                                           select ug.Group;
                         //pridanie skupin do zoznamu skupin, ktory sluzba vracia    
                         foreach (var group in GroupObjects)
                         {
                            gl.Add( new ServiceEntities.Group()
                            {
                               Id = group.Id,
                               Name = group.Name
                  

                             });
                         }
                    }
                    catch (Exception ex)
                    {
                        LogsController.WriteException("GetLastUsed", ex.Message);
                    }
                }
            }
            catch (MyDBContextException e)
            {
                throw new WebFaultException<ServiceEntities.ErrorHandler>(new ServiceEntities.ErrorHandler { cause = e.Message, inner = e.InnerException != null ? e.InnerException.Message : "", errorCode = 500 }, System.Net.HttpStatusCode.InternalServerError);
            }
            
            return gl;
        }

        [WebInvoke(UriTemplate = "previousrecommendations/", Method = "POST")]
        public List<ServiceEntities.Recommendation> PreviousRecommendations(ServiceEntities.PageObject pageObject)
        {
            List<ServiceEntities.Recommendation> recommendationsList = new List<ServiceEntities.Recommendation>();
            try
            {
                // pripojenie sa k databaze 
                using (MyDBContext con = new MyDBContext())
                {
                    var pages = con.Fetch<Page>(p => p.AbsoluteURI == pageObject.AbsoluteURI);
                    if (!pages.Any())
                        return recommendationsList;

                    Page page = pages.First();
                    
                    if (page.GroupRecommendations.Any())
                    {
                        foreach (var recommendation in page.GroupRecommendations)
                        {
                            recommendationsList.Add(new ServiceEntities.Recommendation()
                            {
                                Description = recommendation.Description,
                                Title = recommendation.Title,
                                Group = new ServiceEntities.PageGroup()
                                {
                                    Name = recommendation.Group.Name
                                }
                            });
                        }
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
            return recommendationsList;
        }


        //adresa sluzby
        [WebGet(UriTemplate = "get_group_by_name/{group_name}/")]
        public bool GetGroupByName(string group_name)
        {
            try
            {
                using (MyDBContext con = new MyDBContext())
                {
                    // id aktualneho pouzivatela
                    try
                    {
                        // vyber 4 posledne pouzitych skupin daneho pouzivatela
                        IQueryable<Group> groups = con.Fetch<Group>(u => u.Name == group_name);
                        if (groups.Any())
                            return true;
                        else
                            return false;

                    }
                    catch (Exception ex)
                    {
                        LogsController.WriteException("GetGroupByName", ex.Message);
                    }
                }
            }
            catch (MyDBContextException e)
            {
                throw new WebFaultException<ServiceEntities.ErrorHandler>(new ServiceEntities.ErrorHandler { cause = e.Message, inner = e.InnerException != null ? e.InnerException.Message : "", errorCode = 500 }, System.Net.HttpStatusCode.InternalServerError);
            }
            return false;

        }

        [WebGet(UriTemplate = "get_local_groups?since={since}")]
        public List<ServiceEntities.Group> GetLocalGroups(string since)
        {
            List<ServiceEntities.Group> groups = new List<ServiceEntities.Group>();
            try
            {
                DateTime sinceDt = DateTime.ParseExact(since, "yyyy'-'MM'-'dd'T'HH'_'mm'_'ss.fffffffK", null);

                using (MyDBContext con = new MyDBContext())
                {
                    try
                    {
                        IQueryable<Group> dbGroups = con.Fetch<Group>(g => !g.Location && g.DateCreated > sinceDt);
                        foreach (var group in dbGroups)
                        {
                            groups.Add(new ServiceEntities.Group()
                            {
                                Name = group.Name,
                                Description = group.Description,
                                DateCreated = group.DateCreated,
                                Id = group.Id
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        LogsController.WriteException("GetLocalGroups", ex.Message);
                    }
                }
            }
            catch (MyDBContextException e)
            {
                throw new WebFaultException<ServiceEntities.ErrorHandler>(new ServiceEntities.ErrorHandler { cause = e.Message, inner = e.InnerException != null ? e.InnerException.Message : "", errorCode = 500 }, System.Net.HttpStatusCode.InternalServerError);
            }

            return groups;
        }

        [WebGet(UriTemplate = "recommendations?group={groupId}&since={since}")]
        public List<ServiceEntities.Groups.SimpleRecommendation> GetRecommendations(string groupId, string since)
        {
            var recommendations = new List<ServiceEntities.Groups.SimpleRecommendation>();
            try
            {
                DateTime sinceDt = DateTime.ParseExact(since, "yyyy'-'MM'-'dd'T'HH'_'mm'_'ss.fffffffK", null);

                using (MyDBContext con = new MyDBContext())
                {
                    try
                    {
                        int iGroupId = Convert.ToInt32(groupId);
                        IQueryable<Group> dbGroups = con.Fetch<Group>(g => g.Id == iGroupId);
                        if (dbGroups.Any())
                        {
                            var group = dbGroups.First();
                            foreach (var recommendation in group.Recommendations.Where(r => r.DateTime > sinceDt))
                            {
                                recommendations.Add(new ServiceEntities.Groups.SimpleRecommendation()
                                {
                                    Description = recommendation.Description,
                                    Title = recommendation.Title,
                                    AbsoluteUri = recommendation.Page.AbsoluteURI,
                                    DateCreated = recommendation.DateTime
                                });
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        LogsController.WriteException("GetLocalGroups", ex.Message);
                    }
                }
            }
            catch (MyDBContextException e)
            {
                throw new WebFaultException<ServiceEntities.ErrorHandler>(new ServiceEntities.ErrorHandler { cause = e.Message, inner = e.InnerException != null ? e.InnerException.Message : "", errorCode = 500 }, System.Net.HttpStatusCode.InternalServerError);
            }

            return recommendations;
        }
    }
}
