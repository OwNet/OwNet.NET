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
    public class Recommendation
    {
        private EntityRef<Group> groupRef = new EntityRef<Group>();

        public Recommendation()
        {
            DateCreated = DateTime.Now;
        }

        [Column(IsPrimaryKey = true, IsDbGenerated = true, DbType = "INT NOT NULL Identity", CanBeNull = false, AutoSync = AutoSync.OnInsert)]
        public int Id { get; set; }

        [Column(CanBeNull = false)]
        public string AbsoluteUri { get; set; }

        [Column]
        public string Title { get; set; }

        [Column]
        public string Description { get; set; }

        [Column(CanBeNull = false)]
        public DateTime DateCreated { get; set; }

        [Association(Name = "FK_Group_Recommendations", Storage = "groupRef", ThisKey = "GroupId", OtherKey = "Id", IsForeignKey = true)]
        public Group Group
        {
            get
            {
                return this.groupRef.Entity;
            }
            set
            {
                this.groupRef.Entity = value;
                value.Recommendations.Add(this);
                this.GroupId = value.Id;
            }
        }

        [Column(CanBeNull = false)]
        public int GroupId { get; set; }
    }
}
