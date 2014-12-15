using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;

namespace WPFServer.DatabaseContext
{
    public class Edge : DbContext, DbFetch
    {
        public int Id { get; set; }
        
        public virtual Page PageFrom { get; set; }

        public int PageFromId { get; set; }

        public virtual Page PageTo { get; set; }

        public int PageToId { get; set; }

        public Table GetTableType() { return Table.Edge; }
        public System.Linq.Expressions.Expression<Func<T, bool>> GetCreateConstraint<T>()
        {
            return y => this.PageFrom.AbsoluteURI.Equals((y as Edge).PageFrom.AbsoluteURI) && this.PageTo.AbsoluteURI.Equals((y as Edge).PageTo.AbsoluteURI);
        }

        public virtual ICollection<UserTraversesEdge> Traverses { get; set; }

        public void Update(DbFetch updateItem)
        {
            //UserVisitsPageItem item = updateItem as UserVisitsPageItem;
            //if (item.Rating != null && this.Rating != item.Rating) this.Rating = item.Rating;
            //else if (this.TimeStamp < item.TimeStamp) this.TimeStamp = item.TimeStamp;
        }

        public class EntityConfiguration : System.Data.Entity.ModelConfiguration.EntityTypeConfiguration<Edge>
        {
            public EntityConfiguration()
            {
                HasRequired(e => e.PageFrom).WithMany(l => l.EdgesFrom).HasForeignKey(m => m.PageFromId).WillCascadeOnDelete(false);
                HasRequired(e => e.PageTo).WithMany(l => l.EdgesTo).HasForeignKey(m => m.PageToId).WillCascadeOnDelete(false);
                HasKey(t => t.Id);
                Property(t => t.Id).HasDatabaseGeneratedOption(System.ComponentModel.DataAnnotations.DatabaseGeneratedOption.Identity);
            }
        }
    }

    
}
