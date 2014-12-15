using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;

namespace WPFServer.DatabaseContext.Cache
{
    public class Cache : DbContext, DbFetch
    {
        public Cache()
        {
            DateCreated = DateTime.Now;
            DateModified = DateTime.Now;
            Expires = DateTime.Now;
        }

        public int Id { get; set; }
        public int StatusCode { get; set; }
        public int AccessCount { get; set; }
        public int Parts { get; set; }

        public long ContentLength { get; set; }
        public long Size { get; set; }

        public double AccessValue { get; set; }

        public string AbsoluteUri { get; set; }
        public string ContentType { get; set; }
        public string UserAgent { get; set; }
        public string StatusDescription { get; set; }
        public string CharacterSet { get; set; }

        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }
        public DateTime Expires { get; set; }

        public virtual ICollection<CacheHeader> CacheHeaders { get; set; }
        public virtual ICollection<CacheUpdate> CacheUpdates { get; set; }

        public Table GetTableType() { return Table.Cache; }

        public System.Linq.Expressions.Expression<Func<DbFetch, bool>> GetCreateConstraint<DbFetch>()
        {
            return y => this.Id == (y as Cache).Id;
        }

        public void Update(DbFetch item)
        {
            return;
        }

        public class EntityConfiguration : System.Data.Entity.ModelConfiguration.EntityTypeConfiguration<Cache>
        {
            public EntityConfiguration()
            {
                HasKey(n => n.Id);
                Property(n => n.Id).HasDatabaseGeneratedOption(System.ComponentModel.DataAnnotations.DatabaseGeneratedOption.None);
                Property(n => n.StatusCode).IsRequired();
                Property(n => n.AccessCount).IsRequired();
                Property(n => n.Parts).IsRequired();

                Property(n => n.ContentLength).IsRequired();
                Property(n => n.Size).IsRequired();

                Property(n => n.AccessValue).IsRequired();

                Property(n => n.AbsoluteUri).IsRequired();
                Property(n => n.ContentType).IsRequired();
                Property(n => n.UserAgent).IsRequired();
                Property(n => n.StatusDescription).IsRequired();
                Property(n => n.CharacterSet).IsRequired();

                Property(n => n.DateCreated).IsRequired();
                Property(n => n.DateModified).IsRequired();
                Property(n => n.Expires).IsRequired();
            }
        }
    }
}
