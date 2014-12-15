using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;

namespace WPFServer.DatabaseContext
{
    public class UserSimilarity : DbContext, DbFetch
    {
        public UserSimilarity()
        {
            EvaluatedAt = DateTime.Now;
        }

        public int UserLeftId { get; set; }

        public virtual User UserLeft { get; set; }

        public int UserRightId { get; set; }

        public virtual User UserRight { get; set; }

        public double Value { get; set; }

        public DateTime EvaluatedAt { get; set; }

        public Table GetTableType() { return Table.UserSimilarity; }
        public System.Linq.Expressions.Expression<Func<T, bool>> GetCreateConstraint<T>()
        {
            return y => (this.UserLeft.Username.ToLower().Equals((y as UserSimilarity).UserLeft.Username.ToLower()) && this.UserRight.Username.ToLower().Equals((y as UserSimilarity).UserRight.Username.ToLower()))
                || (this.UserRight.Username.ToLower().Equals((y as UserSimilarity).UserLeft.Username.ToLower()) && this.UserLeft.Username.ToLower().Equals((y as UserSimilarity).UserRight.Username.ToLower()));
        }

        public void Update(DbFetch updateItem)
        {
            UserSimilarity item = updateItem as UserSimilarity;
            this.Value = item.Value;
            this.EvaluatedAt = DateTime.Now;
        }

        public class EntityConfiguration : System.Data.Entity.ModelConfiguration.EntityTypeConfiguration<UserSimilarity>
        {
            public EntityConfiguration()
            {
                HasRequired(s => s.UserLeft).WithMany(u => u.RightNeighbors).HasForeignKey(s => s.UserLeftId).WillCascadeOnDelete(false);
                HasRequired(s => s.UserRight).WithMany(u => u.LeftNeighbors).HasForeignKey(s => s.UserRightId).WillCascadeOnDelete(false);
                HasKey(t => new { t.UserLeftId, t.UserRightId });
            }

        }
    }
    
}
