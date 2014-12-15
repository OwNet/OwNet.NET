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
using System.Xml.Linq;

namespace PhoneApp.Controllers
{
    public class LocalGroupsController : Helpers.NotifierObject
    {
        DateTime _updateTime = DateTime.Now;

        public void GetLocalGroups()
        {
            DateTime lastUpdate = LastLocalGroupsUpdate;
            _updateTime = DateTime.Now;

            if (_updateTime.Subtract(TimeSpan.FromMinutes(2)) < lastUpdate)
                return;

            if (ServersController.IsServerKnown)
            {
                try
                {
                    WebClient serviceRequest = new WebClient();
                    serviceRequest.DownloadStringAsync(new Uri(ServersController.GetServiceAddress(
                        string.Format("groups/get_local_groups?since={0}", Helpers.Common.GetFullString(lastUpdate)))));
                    serviceRequest.DownloadStringCompleted += new DownloadStringCompletedEventHandler(serviceRequest_DownloadStringCompleted);
                }
                catch (Exception ex)
                {
                    LogsController.WriteException("GetSharedFilesCall", ex.Message);
                }
            }
        }

        void serviceRequest_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            try
            {
                string res = e.Result;
                XDocument document = XDocument.Parse(res);

                if (document.Root != null)
                {
                    var childNode = document.Root.FirstNode;
                    while (childNode != null)
                    {
                        var childElement = childNode as XElement;
                        ReadGroup(childElement);
                        childNode = childNode.NextNode;
                    }
                }

                LastLocalGroupsUpdate = _updateTime;

                NotifySuccess();
            }
            catch (Exception ex)
            {
                Controllers.LogsController.WriteException("GetSharedFiles", ex.Message);
                ServersController.ServerAddressInvalid();
            }
        }

        void ReadGroup(XElement folderElement)
        {
            Database.Group newGroup = new Database.Group()
            {
                IsGlobal = false
            };
            XNode node = folderElement.FirstNode;

            while (node != null)
            {
                var element = node as XElement;
                switch (element.Name.LocalName)
                {
                    case "DateCreated":
                        newGroup.DateCreated = DateTime.Parse(element.Value);
                        break;

                    case "Id":
                        newGroup.LocalServerId = Convert.ToInt32(element.Value);
                        break;

                    case "Name":
                        newGroup.Name = element.Value;
                        break;

                    case "Description":
                        newGroup.Description = element.Value;
                        break;
                }
                node = node.NextNode;
            }

            try
            {
                using (var context = new Database.DatabaseContext(Database.DatabaseContext.DBConnectionString))
                {
                    var user = context.Users.FirstOrDefault(u => u.Username == UsersController.AuthenticatedUser.Username);
                    if (user == null)
                        return;

                    var dbGroups = from ug in user.UserGroups
                                  let g = ug.Group
                                  where !g.IsGlobal && g.LocalServerId == newGroup.LocalServerId
                                  select g;

                    if (dbGroups.Any())
                    {
                        var dbGroup = dbGroups.First();
                        dbGroup.Name = newGroup.Name;
                        dbGroup.DateCreated = newGroup.DateCreated;
                        newGroup = dbGroup;

                        if (newGroup.UserGroups.FirstOrDefault(ug => ug.User == user) == null)
                            newGroup.UserGroups.Add(new Database.UserGroup()
                            {
                                User = user
                            });
                    }
                    else
                    {
                        newGroup.Id = string.Format("{0}-{1}", newGroup.LocalServerId, DateTime.Now.ToString()).GetHashCode();
                        newGroup.UserGroups.Add(new Database.UserGroup()
                        {
                            User = user
                        });
                        context.Groups.InsertOnSubmit(newGroup);
                    }
                    context.SubmitChanges();
                }
            }
            catch (Exception ex)
            {
                LogsController.WriteException("ReadGroup", ex.Message);
            }
        }

        DateTime LastLocalGroupsUpdate
        {
            get
            {
                return SettingsController.GetDateTimeSetting("LastLocalGroupsUpdate");
            }
            set
            {
                SettingsController.SetDateTimeSetting("LastLocalGroupsUpdate", value);
            }
        }
    }
}
