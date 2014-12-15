using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;

namespace WPFServer.DatabaseContext
{
    public class CacheLink : DbContext, DbFetch
    {
        public CacheLink()
        {
            AbsoluteURI = "";
            DateCreated = DateTime.Now;
        }

        public int Id { get; set; }

        public string AbsoluteURI { get; set; }

        public DateTime DateCreated { get; set; }

        public DateTime LastModified { get; set; }

        public virtual ICollection<ClientCacheLink> ClientCacheLinks { get; set; }

        public Table GetTableType() { return Table.CacheLink; }

        public System.Linq.Expressions.Expression<Func<DbFetch, bool>> GetCreateConstraint<DbFetch>()
        {
            return y => this.Id == (y as CacheLink).Id;
        }

        public bool IsTheSameAs(CacheLink item)
        {
            return this.AbsoluteURI == item.AbsoluteURI;
        }

        public void Update(DbFetch item)
        {
            return;
        }

        public class EntityConfiguration : System.Data.Entity.ModelConfiguration.EntityTypeConfiguration<CacheLink>
        {
            public EntityConfiguration()
            {
                HasKey(n => n.Id);
                Property(n => n.Id).HasDatabaseGeneratedOption(System.ComponentModel.DataAnnotations.DatabaseGeneratedOption.None);
                Property(n => n.AbsoluteURI).IsRequired();
                Property(n => n.DateCreated).IsRequired();
                Property(n => n.LastModified).IsRequired();
            }
        }
    }
}
