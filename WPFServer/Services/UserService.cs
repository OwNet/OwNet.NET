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
    public class UserService : IUserService
    {

        [WebInvoke(UriTemplate = "register/", Method = "POST")]
        public bool RegisterStudent(ServiceEntities.UserRegisters reg)
        {
            try
            {
                using (MyDBContext con = new MyDBContext())
                {
                    DateTime reged = DateTime.Now;

                    User user = new User() { Username = reg.Username, Firstname = reg.Firstname, Surname = reg.Surname, Password = reg.Password, Gender = reg.IsMale ? UserGender.Male : UserGender.Female, Email = reg.Email, IsTeacher = false };
                    user = con.FetchOrCreate<User>(user);


                    if (reged <= user.Registered)
                    {
                        for (int i = 1; i <= 5; i++)
                        {

                            Group Group = Group = con.Fetch<Group>(g => g.Id == i).First();

                            UserGroup uG = new UserGroup() { User = user, Group = Group, LastUsed = DateTime.Now };
                            con.FetchOrCreate<UserGroup>(uG);
                        }
                        return true;
                    }
                }
            }
            catch (Exception e)
            {
                throw new WebFaultException<ServiceEntities.ErrorHandler>(new ServiceEntities.ErrorHandler { cause = e.Message, inner = e.InnerException != null ? e.InnerException.Message : "", errorCode = 500 }, System.Net.HttpStatusCode.InternalServerError);
            }
            return false;
        }

        [WebGet(UriTemplate = "list/page/{part}/")]
        public ServiceEntities.RegisteredUsers List(string part)
        {
            ServiceEntities.RegisteredUsers ret = new ServiceEntities.RegisteredUsers();
            try
            {
                int pag = Convert.ToInt32(part);
                using (MyDBContext con = new MyDBContext())
                {
                    try
                    {
                        IQueryable<User> users = con.FetchSet<User>().OrderBy(u => u.Username);

                        int currentPage = pag;
                        int totalPages = pag;

                        Helpers.Search.ProcessPages(users.Count(), pag, out totalPages, out currentPage);

                        ret.TotalPages = totalPages;
                        ret.CurrentPage = currentPage;

                        Helpers.Search.ExtractPage(ref users, currentPage);

                        FillUserList(ret, users.ToList());
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

        private bool FillUser(User user, ServiceEntities.User ret)
        {
            if (user == null || ret == null) return false;
            ret.Username = user.Username;
            ret.Firstname = user.Firstname;
            ret.Surname = user.Surname;
            ret.Email = user.Email;
            ret.IsMale = (user.Gender == UserGender.Male) ? true : false;
            ret.IsTeacher = user.IsTeacher;
            ret.Registered = user.Registered;
            ret.Id = user.Id;
            return true;
        }

        private void FillUserList(ServiceEntities.RegisteredUsers ret, List<User> users)
        {
            ServiceEntities.User ul;
            foreach (User user in users)
            {
                ul = new ServiceEntities.User();
                if (FillUser(user, ul) == true)
                    ret.Users.Add(ul);
            }
        }

        [WebGet(UriTemplate = "list/group/{group}/page/{part}/")]
        public ServiceEntities.RegisteredUsers ListGroup(string group, string part)
        {
            ServiceEntities.RegisteredUsers ret = new ServiceEntities.RegisteredUsers();
            try
            {
                int pag = Convert.ToInt32(part);
                group = group.ToLower();
                if (!group.Equals("teacher") && !group.Equals("student"))
                    return ret;
                using (MyDBContext con = new MyDBContext())
                {
                    try
                    {
                        bool teacher = group.Equals("teacher") ? true : false;
                        IQueryable<User> users = con.Fetch<User>(u => u.IsTeacher == teacher).OrderBy(u => u.Username);

                        int currentPage = pag;
                        int totalPages = pag;

                        Helpers.Search.ProcessPages(users.Count(), pag, out totalPages, out currentPage);

                        ret.TotalPages = totalPages;
                        ret.CurrentPage = currentPage;

                        Helpers.Search.ExtractPage(ref users, currentPage);

                        FillUserList(ret, users.ToList());
                    }
                    catch (Exception) { } 
                }
            }
            catch (Exception)
            {
            }
            return ret;
        }


        public bool RegisterTeacher(ServiceEntities.UserRegisters reg)
        {
            try
            {
                using (MyDBContext con = new MyDBContext())
                {
                    DateTime reged = DateTime.Now;

                    User user = new User() { Username = reg.Username, Firstname = reg.Firstname, Surname = reg.Surname, Password = reg.Password, Gender = reg.IsMale ? UserGender.Male : UserGender.Female, Email = reg.Email, IsTeacher = true };
                    user = con.FetchOrCreate<User>(user);

                    for (int i = 1; i <= 5; i++)
                    {

                        Group Group = Group = con.Fetch<Group>(g => g.Id == i).First();

                        UserGroup uG = new UserGroup() { User = user, Group = Group, LastUsed = DateTime.Now };
                        con.FetchOrCreate<UserGroup>(uG);
                        con.SaveChanges();
                    }

                    if (reged <= user.Registered)
                    {
                        return true;
                    }
                }

            }
            catch (Exception)
            {
           
            }
            return false;
        }
        
        public bool AnyTeacher()
        {
            bool ret = false;
            try
            {
                using (MyDBContext con = new MyDBContext())
                {
                    ret = con.Fetch<User>(u => u.IsTeacher == true).Any();
                }
            }
            catch (Exception)
            {
            }
            return ret;
        }

        [WebGet(UriTemplate = "{id}/")]
        public ServiceEntities.User GetUser(string id)
        {
            IQueryable<User> users;
            User user;
            ServiceEntities.User ret = new ServiceEntities.User();
            try
            {

                using (MyDBContext con = new MyDBContext())
                {
                    try
                    {
                        int sid = Convert.ToInt32(id);
                        users = con.Fetch<User>(u => u.Id == sid);

                        if (users.Any())
                        {
                            user = users.First();
                            FillUser(user, ret);
                        }
                    }
                    catch (Exception) { }
                }
            }
            catch (Exception)
            {
            }

            return ret;
        }


        [WebInvoke(UriTemplate = "{id}/", Method = "PUT")]
        public ServiceEntities.UserUpdated UpdateUser(string id, ServiceEntities.User uu)
        {
            User user;
            ServiceEntities.UserUpdated ret = new ServiceEntities.UserUpdated();
            try
            {
                int sid = Convert.ToInt32(id);
                using (MyDBContext con = new MyDBContext())
                {
                    IQueryable<User> users = con.Fetch<User>(u => u.Id == sid);
                    if (users.Any())
                    {
                        user = users.First();

                        FillUser(user, ret);
                        
                        user.Surname = uu.Surname;
                        user.Firstname = uu.Firstname;
                        user.Gender = uu.IsMale ? UserGender.Male : UserGender.Female;
                        user.IsTeacher = uu.IsTeacher;
                        user.Email = uu.Email;
                       
                        con.SaveChanges();

                        FillUser(user, ret);
                        ret.Success = true;
                    }
                }
            }
            catch (Exception)
            {
            }
            return ret;
        }

        [WebInvoke(UriTemplate = "{id}/password/", Method = "PUT")]
        public bool ChangePassword(string id, ServiceEntities.UserUpdatesPassword uu)
        {
            User user;
            bool pass = false;
            try
            {
                int sid = Convert.ToInt32(id);
                using (MyDBContext con = new MyDBContext())
                {
                    IQueryable<User> users = con.Fetch<User>(u => u.Id == sid);
                    if (users.Any())
                    {
                        user = users.First();

                        if (!String.IsNullOrWhiteSpace(uu.Password) && !String.IsNullOrWhiteSpace(uu.OldPassword) && User.IsValidPassword(uu.Password))
                        {
                            if ((uu.Reset == false && user.VerifiyPassword(uu.OldPassword)) || (uu.Reset == true))
                            {
                                user.Password = uu.Password;
                                con.SaveChanges();
                                pass = true;
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
            return pass;
        }

        [WebInvoke(UriTemplate = "login/", Method = "POST")]
        public ServiceEntities.UserLoggedIn LogIn(ServiceEntities.UserLogsIn uwp)
        {
            IQueryable<User> users;
            User user;
            ServiceEntities.UserLoggedIn ret = new ServiceEntities.UserLoggedIn();
            try
            {
                using (MyDBContext con = new MyDBContext())
                {
                    try
                    {
                        users = con.Fetch<User>(new User() { Username = uwp.Username });

                        if (users.Any())
                        {
                            user = users.First();

                            if (user.VerifiyPassword(uwp.Password))
                            {
                                FillUser(user, ret.User);
                                ret.Success = true;
                            }
                        }
                    }
                    catch { throw; }
                }
            }
            catch (Exception e)
            {
                throw new WebFaultException<ServiceEntities.ErrorHandler>(new ServiceEntities.ErrorHandler { cause = e.Message, inner = e.InnerException != null ? e.InnerException.Message : "", errorCode = 500 }, System.Net.HttpStatusCode.InternalServerError);
            }

            return ret;
        }

        [WebInvoke(UriTemplate = "{id}/", Method = "DELETE")]
        public bool DeleteUser(string id)
        {
            try
            {
                int uid = Convert.ToInt32(id);
                using (MyDBContext con = new MyDBContext())
                {
                    try
                    {
                        IQueryable<User> users = con.Fetch<User>(u => u.Id == uid);
                        if (users.Any())
                        {
                            User user = users.First();
                            while (user.Activities.Any())
                            {
                                con.FetchSet<Activity>().Remove(user.Activities.First());
                            }
                            while (user.LeftNeighbors.Any())
                            {
                                con.FetchSet<UserSimilarity>().Remove(user.LeftNeighbors.First());
                            }
                            while (user.RightNeighbors.Any())
                            {
                                con.FetchSet<UserSimilarity>().Remove(user.RightNeighbors.First());
                            }
                            con.Remove<User>(user);
                            return true;
                        }
                    }
                    catch (Exception) { }
                }
            }
            catch (Exception)
            {
            }
            return false;
        }
    }
}