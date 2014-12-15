using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ClientAndServerShared;
using WPFServer.DatabaseContext;

namespace WPFServer.CentralService
{
    class UserReporter
    {
        internal static List<LocalServerCentralService.UserReport> GenerateUserReport()
        {
            var userReport = new List<LocalServerCentralService.UserReport>();

            try
            {
                using (var con = new MyDBContext())
                {
                    var users = con.Fetch<User>(u =>
                        u.Registered > Properties.Settings.Default.LastCentralServiceReport);

                    foreach (var user in users)
                    {
                        userReport.Add(new LocalServerCentralService.UserReport()
                        {
                            Username = user.Username,
                            PasswordHash = user.PasswordHash,
                            PasswordSalt = user.PasswordSalt,
                            FirstName = user.Firstname,
                            Surname = user.Surname
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                LogsController.WriteException("GenerateUserReport", ex.Message);
            }

            return userReport;
        }

        internal static List<LocalServerCentralService.UserGroupReport> GenerateUserGroupsReport()
        {
            var userGroupsReports = new List<LocalServerCentralService.UserGroupReport>();

            try
            {
                using (var con = new MyDBContext())
                {
                    var userGroups = con.Fetch<UserGroup>(u =>
                        u.DateCreated > Properties.Settings.Default.LastCentralServiceReport && u.Group.Location);

                    foreach (var userGroup in userGroups)
                    {
                        userGroupsReports.Add(new LocalServerCentralService.UserGroupReport()
                        {
                            Username = userGroup.User.Username,
                            GroupId = userGroup.GroupId
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                LogsController.WriteException("GenerateUserGroupsReport", ex.Message);
            }

            return userGroupsReports;
        }
    }
}
