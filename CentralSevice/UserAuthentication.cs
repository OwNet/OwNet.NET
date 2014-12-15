using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ServiceEntities.CentralService.v3;
using Helpers;

namespace CentralServerShared
{
    public class UserAuthentication
    {
        public static AuthenticateUserResult Authenticate(AuthenticateUserRequest request)
        {
            AuthenticateUserResult result = new AuthenticateUserResult()
            {
                Success = false,
                Cookie = ""
            };

            try
            {
                using (var container = new DataModelContainer())
                {
                    var users = container.Users.Where(u => u.Username == request.Username
                        && u.Server.Username == request.ServerUsername);
                    if (users.Any())
                    {
                        var user = users.First();

                        SaltedHash hash = new SaltedHash();

                        if (hash.VerifyHashString(request.Password, user.PasswordHash, user.PasswordSalt))
                        {
                            string cookie = Helpers.Common.RandomString(12, true);

                            user.UserCookies.Add(new UserCookie()
                            {
                                Cookie = cookie,
                                DateCreated = DateTime.Now
                            });

                            container.SaveChanges();

                            result.Cookie = cookie;
                            result.Success = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Controller.WriteException("AuthenticateUser", ex.Message);
            }
            return result;
        }

        public static User GetUserByAuthentication(ServiceEntities.CentralService.v3.AuthenticatedUser authenticatedUser,
            DataModelContainer container)
        {
            try
            {
                var users = from uc in container.UserCookies
                            where uc.Cookie == authenticatedUser.Cookie
                            && uc.User.Username == authenticatedUser.Username
                            && uc.User.Server.Username == authenticatedUser.ServerUsername
                            select uc.User;

                if (users.Any())
                    return users.First();
            }
            catch (Exception ex)
            {
                Controller.WriteException("GetUserByAuthentication", ex.Message);
            }
            return null;
        }
    }
}