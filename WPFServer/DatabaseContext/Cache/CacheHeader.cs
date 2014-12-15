using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;

namespace WPFServer.DatabaseContext.Cache
{
    public class CacheHeader : DbContext, DbFetch
    {
        public CacheHeader()
        { }

        public int Id { get; set; }

        public string Key { get; set; }
        public string Value { get; set; }

        public int CacheId { get; set; }
        public virtual Cache Cache { get; set; }

        public Table GetTableType() { return Table.CacheHeader; }

        public System.Linq.Expressions.Expression<Func<DbFetch, bool>> GetCreateConstraint<DbFetch>()
        {
            return y => this.Id == (y as CacheHeader).Id;
        }

        public void Update(DbFetch item)
        {
            return;
        }

        public class EntityConfiguration : System.Data.Entity.ModelConfiguration.EntityTypeConfiguration<CacheHeader>
        {
            public EntityConfiguration()
            {
                HasKey(n => n.Id);

                Property(n => n.Key).IsRequired();
                Property(n => n.Value).IsRequired();

                HasRequired(n => n.Cache).WithMany(u => u.CacheHeaders).HasForeignKey(n => n.CacheId).WillCascadeOnDelete(true);
            }
        }
    }
}
