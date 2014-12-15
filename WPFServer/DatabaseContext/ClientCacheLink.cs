using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;

namespace WPFServer.DatabaseContext
{
    public class ClientCacheLink : DbContext, DbFetch
    {
        public ClientCacheLink()
        {
            DateCreated = DateTime.Now;
        }

        public int Id { get; set; }

        public int CacheLinkId { get; set; }

        public virtual CacheLink CacheLink { get; set; }

        public int ClientId { get; set; }

        public virtual Client Client { get; set; }

        public DateTime DateCreated { get; set; }

        public Table GetTableType() { return Table.ClientCacheLink; }

        public System.Linq.Expressions.Expression<Func<DbFetch, bool>> GetCreateConstraint<DbFetch>()
        {
            return y => this.ClientId == (y as ClientCacheLink).ClientId
                && this.CacheLinkId == (y as ClientCacheLink).CacheLinkId;
        }

        public void Update(DbFetch item)
        {
            return;
        }

        public class EntityConfiguration : System.Data.Entity.ModelConfiguration.EntityTypeConfiguration<ClientCacheLink>
        {
            public EntityConfiguration()
            {
                HasKey(n => n.Id);
                HasRequired(n => n.Client).WithMany(u => u.ClientCacheLinks).HasForeignKey(n => n.ClientId).WillCascadeOnDelete(true);
                HasRequired(n => n.CacheLink).WithMany(u => u.ClientCacheLinks).HasForeignKey(n => n.CacheLinkId).WillCascadeOnDelete(true);
            }
        }
    }
}
