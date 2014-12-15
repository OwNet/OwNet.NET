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
    public class UserGroup
    {
        private EntityRef<Group> groupRef = new EntityRef<Group>();
        private EntityRef<User> userRef = new EntityRef<User>();

        [Column(IsPrimaryKey = true, IsDbGenerated = true, DbType = "INT NOT NULL Identity", CanBeNull = false, AutoSync = AutoSync.OnInsert)]
        public int Id { get; set; }

        [Association(Name = "FK_Group_UserGroups", Storage = "groupRef", ThisKey = "GroupId", OtherKey = "Id", IsForeignKey = true)]
        public Group Group
        {
            get
            {
                return this.groupRef.Entity;
            }
            set
            {
                this.groupRef.Entity = value;
                value.UserGroups.Add(this);
                this.GroupId = value.Id;
            }
        }

        [Column(CanBeNull = false)]
        public int GroupId { get; set; }

        [Association(Name = "FK_User_UserGroups", Storage = "userRef", ThisKey = "UserId", OtherKey = "Id", IsForeignKey = true)]
        public User User
        {
            get
            {
                return this.userRef.Entity;
            }
            set
            {
                this.userRef.Entity = value;
                value.UserGroups.Add(this);
                this.UserId = value.Id;
            }
        }

        [Column(CanBeNull = false)]
        public int UserId { get; set; }
    }
}
