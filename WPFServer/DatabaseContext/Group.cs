using System;
using System.Collections.Generic;
using System.Data.Entity;

namespace WPFServer.DatabaseContext
{
    public class Group : DbContext, DbFetch 
    {

         public Group()
        {
            Name = "";
            Description = "";
            DateCreated = DateTime.Now;
            Location = true;
        }


         public int Id { get; set; }

        public bool Location { get; set; } // false - local,  true - global


        public string Name { get; set; }


        public string Description { get; set; }


        public DateTime DateCreated { get; set; }

        public Table GetTableType() { return Table.Group; }

        public System.Linq.Expressions.Expression<Func<DbFetch, bool>> GetCreateConstraint<DbFetch>()
        {
            return y => this.Name.ToLower().Equals((y as Group).Name.ToLower());
        }

        public void Update(DbFetch item)
        {
            return;
        }

        public virtual ICollection<UserGroup> UserGroups { get; set; }

        public virtual ICollection<GroupTag> GroupTags { get; set; }

        public virtual ICollection<AdminGroup> AdminGroups { get; set; }

        public virtual ICollection<GroupRecommendation> Recommendations { get; set; }

        public virtual ICollection<GroupSharedFolder> SharedFolders { get; set; }

        public virtual ICollection<Activity> Activities { get; set; }

        /* obsahuje prvky z listu list */

      /*  public bool ContainsAllStrings(List<string> strings)
        {
            

            foreach (string s in strings)
            {

                if (!System.Text.RegularExpressions.Regex.IsMatch(this.Name, s, System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                    return false;

            }
            return true;
        }*/

    }
}
