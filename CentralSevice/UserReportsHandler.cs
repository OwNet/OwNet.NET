using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CentralServerShared
{
    public class UserReportsHandler
    {
        public static void ProcessReport(List<ServiceEntities.CentralService.v3.UserReport> userReports, int serverId)
        {
            if (userReports == null || userReports.Count == 0)
                return;

            try
            {
                using (var container = new DataModelContainer())
                {
                    Server server = container.Servers.FirstOrDefault(s => s.Id == serverId);
                    if (server == null)
                        return;

                    foreach (var userReport in userReports)
                    {
                        var users = server.Users.Where(u => u.Username == userReport.Username);
                        if (users.Any())
                        {
                            var user = users.First();
                            user.PasswordHash = userReport.PasswordHash;
                            user.PasswordSalt = userReport.PasswordSalt;
                            user.FirstName = userReport.FirstName;
                            user.Surname = userReport.Surname;
                            user.DateModified = DateTime.Now;
                        }
                        else
                        {
                            server.Users.Add(new User()
                            {
                                Username = userReport.Username,
                                PasswordHash = userReport.PasswordHash,
                                PasswordSalt = userReport.PasswordSalt,
                                FirstName = userReport.FirstName,
                                Surname = userReport.Surname,
                                DateCreated = DateTime.Now,
                                DateModified = DateTime.Now
                            });
                        }
                    }
                    container.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Controller.WriteException("ProcessUsersReport", ex.Message);
            }
        }

        internal static void ProcessUserGroupsReport(List<ServiceEntities.CentralService.v3.UserGroupReport> userGroupReports, int serverId)
        {
            if (userGroupReports == null || userGroupReports.Count == 0)
                return;

            try
            {
                using (var container = new DataModelContainer())
                {
                    Server server = container.Servers.FirstOrDefault(s => s.Id == serverId);
                    if (server == null)
                        return;

                    foreach (var userGroupReport in userGroupReports)
                    {
                        var users = server.Users.Where(u => u.Username == userGroupReport.Username);
                        if (!users.Any())
                            continue;

                        var user = users.First();

                        var groups = container.Groups.Where(g => g.Id == userGroupReport.GroupId);
                        if (!groups.Any())
                            continue;

                        var group = groups.First();

                        if (!user.UserGroups.Where(ug => ug.Group.Id == userGroupReport.GroupId).Any())
                        {
                            user.UserGroups.Add(new UserGroup()
                            {
                                Group = group,
                                DateCreated = DateTime.Now
                            });
                        }
                    }
                    container.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Controller.WriteException("ProcessUsersReport", ex.Message);
            }
        }
    }
}