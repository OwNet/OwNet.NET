using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;

namespace WPFServer.DatabaseContext
{
    public class GlobalCacheUpdate : DbContext, DbFetch
    {
        public int Id { get; set; }

        public string AbsoluteUri { get; set; }

        public DateTime DateRecommended { get; set; }

        public int Priority { get; set; }

        public int ClientId { get; set; }

        public virtual Client Client { get; set; }

        public Table GetTableType() { return Table.GlobalCacheUpdate; }

        public System.Linq.Expressions.Expression<Func<DbFetch, bool>> GetCreateConstraint<DbFetch>()
        {
            return y => this.Id == (y as GlobalCacheUpdate).Id;
        }

        public void Update(DbFetch item)
        {
            return;
        }

        public class EntityConfiguration : System.Data.Entity.ModelConfiguration.EntityTypeConfiguration<GlobalCacheUpdate>
        {
            public EntityConfiguration()
            {
                HasKey(n => n.Id);
                HasRequired(n => n.Client).WithMany(u => u.CacheUpdates).HasForeignKey(n => n.ClientId).WillCascadeOnDelete(true);
                Property(n => n.AbsoluteUri).IsRequired();
                Property(n => n.DateRecommended).IsRequired();
            }
        }
    }
}
