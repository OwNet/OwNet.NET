using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;

namespace WPFServer.DatabaseContext
{
    public class AccessLog : DbContext, DbFetch
    {
        public int Id { get; set; }

        public DateTime AccessedAt { get; set; }

        public double FetchDuration { get; set; }

        public int DownloadedFrom { get; set; }

        public string AbsoluteUri { get; set; }

        public int CacheId { get; set; }

        public int Type { get; set; }

        public Table GetTableType() { return Table.AccessLog; }

        public System.Linq.Expressions.Expression<Func<DbFetch, bool>> GetCreateConstraint<DbFetch>()
        {
            return y => this.Id == (y as AccessLog).Id;
        }

        public void Update(DbFetch item)
        {
            return;
        }

        public class EntityConfiguration : System.Data.Entity.ModelConfiguration.EntityTypeConfiguration<AccessLog>
        {
            public EntityConfiguration()
            {
                HasKey(n => n.Id);
                Property(n => n.AbsoluteUri).IsRequired();
                Property(n => n.AccessedAt).IsRequired();
                Property(n => n.CacheId).IsRequired();
                Property(n => n.DownloadedFrom).IsRequired();
                Property(n => n.FetchDuration).IsRequired();
                Property(n => n.Type).IsRequired();
            }
        }
    }
}
