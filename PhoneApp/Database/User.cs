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
using System.Data.Linq.Mapping;
using System.Data.Linq;

namespace PhoneApp.Database
{
    [Table]
    public class User
    {
        private EntitySet<UserGroup> userGroupsRef;

        public User()
        {
            LoggedIn = false;

            userGroupsRef = new EntitySet<UserGroup>(OnUserGroupAdded, OnUserGroupRemoved);
        }

        [Column(IsPrimaryKey = true, IsDbGenerated = true, DbType = "INT NOT NULL Identity", CanBeNull = false, AutoSync = AutoSync.OnInsert)]
        public int Id { get; set; }

        [Column(CanBeNull = false)]
        public string Username { get; set; }

        [Column(CanBeNull = false)]
        public string ServerUsername { get; set; }

        [Column]
        public string Cookie { get; set; }

        [Column]
        public bool LoggedIn { get; set; }

        [Column(CanBeNull = false)]
        public DateTime DateCreated { get; set; }

        [Association(Name = "FK_User_UserGroups", Storage = "userGroupsRef", ThisKey = "Id", OtherKey = "UserId")]
        public EntitySet<UserGroup> UserGroups
        {
            get
            {
                return this.userGroupsRef;
            }
        }

        private void OnUserGroupAdded(UserGroup userGroup)
        {
            userGroup.User = this;
        }

        private void OnUserGroupRemoved(UserGroup userGroup)
        {
            userGroup.User = null;
        }
    }
}
