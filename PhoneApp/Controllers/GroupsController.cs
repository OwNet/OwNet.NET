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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace PhoneApp.Controllers
{
    public class GroupsController : Helpers.NotifierObject
    {
        DateTime _reportTime = DateTime.Now;

        public void GetGroups()
        {
            DateTime lastUpdate = LastGroupsUpdateFromCentralService;
            _reportTime = DateTime.Now;

            if (_reportTime.Subtract(TimeSpan.FromMinutes(2)) < lastUpdate)
                return;

            try
            {
                var client = new PhoneAppCentralService.PhoneAppServiceClient();
                client.GetGroupsAsync(lastUpdate, UsersController.AuthenticatedUser);
                client.GetGroupsCompleted += new EventHandler<PhoneAppCentralService.GetGroupsCompletedEventArgs>(GetGroupsCompleted);
            }
            catch (Exception ex)
            {
                Controllers.LogsController.WriteException("LoadGroups", ex.Message);
            }
        }

        void GetGroupsCompleted(object sender, PhoneAppCentralService.GetGroupsCompletedEventArgs e)
        {
            List<Database.Group> groupItems = new List<Database.Group>();
            try
            {
                var groups = e.Result;

                LastGroupsUpdateFromCentralService = _reportTime;

                foreach (var group in groups)
                {
                    groupItems.Add(new Database.Group()
                    {
                        Name = group.Name,
                        Description = group.Description,
                        Id = group.Id
                    });
                }
                SaveGroups(groupItems);
            }
            catch (Exception ex)
            {
                LogsController.WriteException("GetGroupsCompleted", ex.Message);
            }

            if (groupItems.Any())
                NotifySuccess();
        }

        internal void SaveGroups(List<Database.Group> groups)
        {
            try
            {
                using (Database.DatabaseContext context = new Database.DatabaseContext(Database.DatabaseContext.DBConnectionString))
                {
                    var user = context.Users.FirstOrDefault(u => u.LoggedIn);
                    if (user == null)
                        return;

                    foreach (var group in groups)
                    {
                        var groupInDb = context.Groups.FirstOrDefault(c => c.Id == group.Id);
                        if (groupInDb != null)
                        {
                            groupInDb.Name = group.Name;
                            if (groupInDb.UserGroups.FirstOrDefault(ug => ug.User == user) == null)
                                groupInDb.UserGroups.Add(new Database.UserGroup()
                                {
                                    User = user
                                });
                        }
                        else
                        {
                            group.UserGroups.Add(new Database.UserGroup()
                            {
                                User = user
                            });
                            context.Groups.InsertOnSubmit(group);
                        }
                    }
                    
                    context.SubmitChanges();
                }
            }
            catch (Exception ex)
            {
                LogsController.WriteException("SaveGroups", ex.Message);
            }
        }

        DateTime LastGroupsUpdateFromCentralService
        {
            get
            {
                return SettingsController.GetDateTimeSetting("LastGroupsUpdateFromCentralService");
            }
            set
            {
                SettingsController.SetDateTimeSetting("LastGroupsUpdateFromCentralService", value);
            }
        }
    }
}
