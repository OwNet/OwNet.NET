using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;

namespace WPFServer.DatabaseContext
{
    public class GroupRecommendation : DbContext, DbFetch 
    {
        public int Id { get; set; }

        public int GroupId { get; set; }

        public virtual Group Group { get; set; }

        public int PageId { get; set; }

        public virtual Page Page { get; set; }

        public int UserId { get; set; }

        public virtual User User { get; set; }

        public string Description { get; set; }

        public string Title { get; set; }

        public DateTime DateTime { get; set; }

       // public virtual UserRecommendsPageInGroupItem Recommendation { get; set; }

        public Table GetTableType() { return Table.GroupRecommendation; }

        public void Update(DbFetch item)
        {
            return;
        }

        public System.Linq.Expressions.Expression<Func<DbFetch, bool>> GetCreateConstraint<DbFetch>()
        {
            return y => this.Id == (y as GroupRecommendation).Id;
        }

    }
}
