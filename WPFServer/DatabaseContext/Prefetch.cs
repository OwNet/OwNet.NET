using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;

namespace WPFServer.DatabaseContext
{
    public class Prefetch : DbContext, DbFetch
    {
        public Prefetch()
        {
            DateCreated = DateTime.Now;
            DateCompleted = null;
            Attempts = 0;
        }

        public int Id { get; set; }

        public string AbsoluteUri { get; set; }

        public byte Priority { get; set; }
        public byte Attempts { get; set; }
        public byte Status { get; set; }

        public bool Completed { get; set; }
        public bool Enabled { get; set; }

        public DateTime DateCreated { get; set; }
        public DateTime? DateCompleted { get; set; }

        public Table GetTableType() { return Table.Prefetch; }

        public System.Linq.Expressions.Expression<Func<DbFetch, bool>> GetCreateConstraint<DbFetch>()
        {
            return y => this.Id == (y as Prefetch).Id;
        }

        public void Update(DbFetch item)
        {
            return;
        }

        public class EntityConfiguration : System.Data.Entity.ModelConfiguration.EntityTypeConfiguration<Prefetch>
        {
            public EntityConfiguration()
            {
                HasKey(n => n.Id);

                Property(n => n.AbsoluteUri).IsRequired();

                Property(n => n.Priority).IsRequired();
                Property(n => n.Attempts).IsRequired();
                Property(n => n.Status).IsRequired();

                Property(n => n.Completed).IsRequired();
                Property(n => n.Enabled).IsRequired();

                Property(n => n.DateCreated).IsRequired();
                Property(n => n.DateCompleted).IsRequired();
            }
        }
    }
}
