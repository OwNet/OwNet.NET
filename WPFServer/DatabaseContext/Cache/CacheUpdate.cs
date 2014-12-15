using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;

namespace WPFServer.DatabaseContext.Cache
{
    public class CacheUpdate : DbContext, DbFetch
    {
        public CacheUpdate()
        {
            DateRecommended = DateTime.Now;
        }

        public int Id { get; set; }
        public int Priority { get; set; }

        public DateTime DateRecommended { get; set; }

        public int CacheId { get; set; }
        public virtual Cache Cache { get; set; }

        public Table GetTableType() { return Table.CacheUpdate; }

        public System.Linq.Expressions.Expression<Func<DbFetch, bool>> GetCreateConstraint<DbFetch>()
        {
            return y => this.Id == (y as CacheUpdate).Id;
        }

        public void Update(DbFetch item)
        {
            return;
        }

        public class EntityConfiguration : System.Data.Entity.ModelConfiguration.EntityTypeConfiguration<CacheUpdate>
        {
            public EntityConfiguration()
            {
                HasKey(n => n.Id);
                Property(n => n.Priority).IsRequired();

                Property(n => n.DateRecommended).IsRequired();

                HasRequired(n => n.Cache).WithMany(u => u.CacheUpdates).HasForeignKey(n => n.CacheId).WillCascadeOnDelete(true);
            }
        }
    }
}
