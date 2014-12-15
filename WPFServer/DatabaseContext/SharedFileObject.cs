using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;

namespace WPFServer.DatabaseContext
{
    [System.Data.Services.Common.HasStream]
    public class SharedFileObject : DbContext, DbFetch
    {
        public SharedFileObject()
        {
            FileName = "";
            ContentType = "";
            DateCreated = DateTime.Now;
        }

        public int Id { get; set; }

        public string FileName { get; set; }

        public string ContentType { get; set; }

        public DateTime DateCreated { get; set; }

        public virtual SharedFile SharedFile
        { get; set; }

        public Table GetTableType() { return Table.SharedFileObject; }

        public System.Linq.Expressions.Expression<Func<DbFetch, bool>> GetCreateConstraint<DbFetch>()
        {
            return y => this.Id == (y as SharedFileObject).Id;
        }

        public void Update(DbFetch item)
        {
            return;
        }

        public class EntityConfiguration : System.Data.Entity.ModelConfiguration.EntityTypeConfiguration<SharedFileObject>
        {
            public EntityConfiguration()
            {
                HasKey(n => n.Id);
                Property(n => n.Id).HasDatabaseGeneratedOption(System.ComponentModel.DataAnnotations.DatabaseGeneratedOption.None);
                Property(n => n.DateCreated).IsRequired();
                Property(n => n.FileName).IsRequired();
                Property(n => n.ContentType).IsRequired();
                HasOptional(n => n.SharedFile).WithRequired(n => n.SharedFileObject);
            }
        }
    }
}
