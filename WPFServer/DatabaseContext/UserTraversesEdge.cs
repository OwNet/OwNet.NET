using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;

namespace WPFServer.DatabaseContext
{
    public class UserTraversesEdge : DbContext, DbFetch
    {
        public UserTraversesEdge()
        {
            Frequency = 0;
            TimeStamp = null; //DateTime.Now;
            EstimatedFrequency = null;
        }

        public virtual Edge Edge { get; set; }

        public int EdgeId { get; set; }

        public virtual User User { get; set; }
        
        public int UserId { get; set; }

        public int Frequency { get; set; }

        public DateTime? TimeStamp { get; set; }

        public double? EstimatedFrequency { get; set; }

        public Table GetTableType() { return Table.UserTraversesEdge; }
        public System.Linq.Expressions.Expression<Func<T, bool>> GetCreateConstraint<T>()
        {
            return y => this.Edge.Id.Equals((y as UserTraversesEdge).Edge.Id) && this.User.Username.ToLower().Equals((y as UserTraversesEdge).User.Username.ToLower());
        }

        public void Update(DbFetch updateItem)
        {
            UserTraversesEdge item = updateItem as UserTraversesEdge;
            if (item.TimeStamp != null && item.TimeStamp > this.TimeStamp)//item.Edge.Id == this.Edge.Id && item.User.Username.ToLower().Equals(this.User.Username.ToLower()))
            {
                this.Frequency += 1;
                this.TimeStamp = item.TimeStamp;
               // this.Estimated = false;
            }
            if (item.EstimatedFrequency != null)
            {
                this.EstimatedFrequency = item.EstimatedFrequency;
            }
        }

        public class EntityConfiguration : System.Data.Entity.ModelConfiguration.EntityTypeConfiguration<UserTraversesEdge>
        {
            public EntityConfiguration()
            {
                HasRequired(e => e.Edge).WithMany(l => l.Traverses).HasForeignKey(m => m.EdgeId).WillCascadeOnDelete(false);
                HasRequired(e => e.User).WithMany(l => l.Traverses).HasForeignKey(m => m.UserId).WillCascadeOnDelete(true);
                HasKey(t => new { t.UserId, t.EdgeId });
            }
        }
    }

    
}
