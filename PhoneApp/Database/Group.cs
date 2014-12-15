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
using System.ComponentModel;
using System.Data.Linq.Mapping;
using System.Data.Linq;

namespace PhoneApp.Database
{
    [Table]
    public class Group
    {
        private EntitySet<Recommendation> recommendationsRef;
        private EntitySet<UserGroup> userGroupsRef;

        public Group()
        {
            recommendationsRef = new EntitySet<Recommendation>(OnRecommendationAdded, OnRecommendationRemoved);
            userGroupsRef = new EntitySet<UserGroup>(OnUserGroupAdded, OnUserGroupRemoved);

            LastRecommendationsUpdate = new DateTime(1970, 1, 1);
            DateCreated = DateTime.Now;
            IsGlobal = true;
            Description = "";
        }

        [Column(IsPrimaryKey = true, DbType = "INT NOT NULL", CanBeNull = false, AutoSync = AutoSync.OnInsert)]
        public int Id { get; set; }

        [Column(CanBeNull = false)]
        public string Name { get; set; }

        [Column(CanBeNull = false)]
        public string Description { get; set; }

        [Column(CanBeNull = false)]
        public DateTime LastRecommendationsUpdate { get; set; }

        [Column(CanBeNull = false)]
        public bool IsGlobal { get; set; }

        [Column]
        public int LocalServerId { get; set; }

        [Column(CanBeNull = false)]
        public DateTime DateCreated { get; set; }

        [Association(Name = "FK_Group_Recommendations", Storage = "recommendationsRef", ThisKey = "Id", OtherKey = "GroupId")]
        public EntitySet<Recommendation> Recommendations
        {
            get
            {
                return this.recommendationsRef;
            }
        }

        [Association(Name = "FK_Group_UserGroups", Storage = "userGroupsRef", ThisKey = "Id", OtherKey = "GroupId")]
        public EntitySet<UserGroup> UserGroups
        {
            get
            {
                return this.userGroupsRef;
            }
        }

        private void OnRecommendationAdded(Recommendation recommendation)
        {
            recommendation.Group = this;
        }

        private void OnRecommendationRemoved(Recommendation recommendation)
        {
            recommendation.Group = null;
        }

        private void OnUserGroupAdded(UserGroup userGroup)
        {
            userGroup.Group = this;
        }

        private void OnUserGroupRemoved(UserGroup userGroup)
        {
            userGroup.Group = null;
        }
    }
}
