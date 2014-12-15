using System;
using System.Data.Entity;

namespace WPFServer.DatabaseContext
{
    public class UserVisitsPage : DbContext, DbFetch
    {
        public UserVisitsPage()
        {
            VisitedAt = null;
            RatedAt = null;
            Count = 0;
            EstimatedRating = null;
        }

        public int UserId { get; set; }

      //  private UserItem user;
        public virtual User User { get; set; }//get { return user; } set { user = value; UserId = value.Id; } }

//        private PageItem page;
        public virtual Page Page { get; set; }//get { return page; } set { page = value; PageId = value.Id; } }

        public int PageId { get; set; }

        private int? rating = null; // default value
        public int? Rating
        {
            get { return rating; }
            set
            {
                if (value > 5) rating = 5;
                else if (value < 1) rating = null;
                else rating = value;
            }
        }

        public int Count { get; set; }

        public DateTime? VisitedAt { get; set; }
        public DateTime? RatedAt { get; set; }

        public double? EstimatedRating { get; set; }

        public Table GetTableType() { return Table.UserVisitsPage; }
        public System.Linq.Expressions.Expression<Func<T, bool>> GetCreateConstraint<T>()
        {
            return y => this.Page.AbsoluteURI.Equals((y as UserVisitsPage).Page.AbsoluteURI) && this.User.Username.ToLower().Equals((y as UserVisitsPage).User.Username.ToLower());
        }

        public void Update(DbFetch updateItem)
        {
            UserVisitsPage item = updateItem as UserVisitsPage;
            
            if (item.Rating != null && this.Rating != item.Rating)
            {
                this.Rating = item.Rating;
                this.RatedAt = DateTime.Now;
            }
            
            if (item.VisitedAt != null && this.VisitedAt < item.VisitedAt)
            {
                this.VisitedAt = item.VisitedAt;
                this.Count += 1;
            }

            if (item.EstimatedRating != null)
            {
                this.EstimatedRating = item.EstimatedRating;
            }
        }
        public class EntityConfiguration : System.Data.Entity.ModelConfiguration.EntityTypeConfiguration<UserVisitsPage>
        {
            public EntityConfiguration()
            {
                HasRequired(e => e.Page).WithMany(l => l.Visitors).HasForeignKey(m => m.PageId).WillCascadeOnDelete(true);
                HasRequired(e => e.User).WithMany(l => l.Visits).HasForeignKey(m => m.UserId).WillCascadeOnDelete(true);
                HasKey(t => new { t.UserId, t.PageId });
            }
        }
    }


}
