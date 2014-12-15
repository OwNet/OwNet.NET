using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Linq;

namespace PhoneApp.Controllers
{
    public class UsersController : Helpers.NotifierObject
    {
        private string _username = "";
        private static PhoneAppCentralService.AuthenticatedUser _authenticatedUser = null;

        public static PhoneAppCentralService.AuthenticatedUser AuthenticatedUser
        {
            get { return _authenticatedUser; }
        }

        internal static void Init()
        {
            try
            {
                using (var context = new Database.DatabaseContext(Database.DatabaseContext.DBConnectionString))
                {
                    var users = context.Users.Where(u => u.LoggedIn);
                    if (users.Any())
                    {
                        var user = users.First();
                        _authenticatedUser = new PhoneAppCentralService.AuthenticatedUser()
                        {
                            Username = user.Username,
                            ServerUsername = user.ServerUsername,
                            Cookie = user.Cookie
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                LogsController.WriteException("InitUsers", ex.Message);
            }
        }

        internal void Authenticate(string username, string password)
        {
            _username = username;

            try
            {
                var client = new PhoneAppCentralService.PhoneAppServiceClient();
                client.AuthenticateAsync(new PhoneAppCentralService.AuthenticateUserRequest()
                {
                    Username = username,
                    Password = password,
                    ServerUsername = ServersController.SelectedServer.Username
                });
                client.AuthenticateCompleted += new EventHandler<PhoneAppCentralService.AuthenticateCompletedEventArgs>(AuthenticateCompleted);
            }
            catch (Exception ex)
            {
                LogsController.WriteException("AuthenticateUser", ex.Message);
            }
        }

        void AuthenticateCompleted(object sender, PhoneAppCentralService.AuthenticateCompletedEventArgs e)
        {
            try
            {
                var authenticateResult = e.Result;
                if (authenticateResult.Success)
                {
                    using (var context = new Database.DatabaseContext(Database.DatabaseContext.DBConnectionString))
                    {
                        var users = context.Users.Where(u => u.Username == _username);
                        if (users.Any())
                        {
                            var user = users.First();
                            user.Cookie = authenticateResult.Cookie;
                        }
                        else
                        {
                            context.Users.InsertOnSubmit(new Database.User()
                            {
                                Username = _username,
                                DateCreated = DateTime.Now,
                                LoggedIn = true,
                                ServerUsername = ServersController.SelectedServer.Username,
                                Cookie = authenticateResult.Cookie
                            });
                        }

                        if (_authenticatedUser != null)
                        {
                            var currentUser = context.Users.FirstOrDefault(u => u.Username == _authenticatedUser.Username);
                            if (currentUser != null)
                                currentUser.LoggedIn = false;
                        }

                        context.SubmitChanges();

                        _authenticatedUser = new PhoneAppCentralService.AuthenticatedUser()
                        {
                            Username = _username,
                            ServerUsername = ServersController.SelectedServer.Username,
                            Cookie = authenticateResult.Cookie
                        };
                    }
                }
                NotifySuccess();
            }
            catch (Exception ex)
            {
                LogsController.WriteException("SaveAuthenticateCookie", ex.Message);
            }
        }

        internal static void SwitchUser(string username)
        {
            try
            {
                using (var context = new Database.DatabaseContext(Database.DatabaseContext.DBConnectionString))
                {
                    var user = context.Users.FirstOrDefault(u => u.Username == username);
                    if (user != null)
                    {
                        if (_authenticatedUser != null)
                        {
                            var currentUser = context.Users.FirstOrDefault(u => u.Username == _authenticatedUser.Username);
                            if (currentUser != null)
                                currentUser.LoggedIn = false;
                        }

                        user.LoggedIn = true;

                        context.SubmitChanges();

                        _authenticatedUser = new PhoneAppCentralService.AuthenticatedUser()
                        {
                            Username = user.Username,
                            ServerUsername = user.ServerUsername,
                            Cookie = user.Cookie
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                LogsController.WriteException("SwitchUser", ex.Message);
            }
        }

        internal static void RemoveUser(string username)
        {
            try
            {
                using (var context = new Database.DatabaseContext(Database.DatabaseContext.DBConnectionString))
                {
                    var user = context.Users.FirstOrDefault(u => u.Username == username);
                    if (user != null)
                    {
                        context.Users.DeleteOnSubmit(user);
                        context.SubmitChanges();

                        if (_authenticatedUser != null && _authenticatedUser.Username == username)
                            _authenticatedUser = null;
                    }
                }
            }
            catch (Exception ex)
            {
                LogsController.WriteException("RemoveUser", ex.Message);
            }
        }
    }
}
