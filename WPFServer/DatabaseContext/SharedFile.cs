using System;
using System.Collections.Generic;
using System.Data.Entity;

namespace WPFServer.DatabaseContext
{
    public class SharedFile : DbContext, DbFetch
    {
        public SharedFile()
        {
            Title = "";
            Description = "";
            FileName = "";
            DateCreated = DateTime.Now;
        }

        public int Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string FileName { get; set; }

        public string ContentType { get; set; }

        public DateTime DateCreated { get; set; }

        public int UserId { get; set; }

        public virtual User User
        { get; set; }

        public int SharedFolderId { get; set; }

        public virtual SharedFolder SharedFolder
        { get; set; }

        public virtual SharedFileObject SharedFileObject
        { get; set; }

        public virtual ICollection<Activity> Activities { get; set; }

        public Table GetTableType() { return Table.SharedFile; }

        public System.Linq.Expressions.Expression<Func<DbFetch, bool>> GetCreateConstraint<DbFetch>()
        {
            return y => this.Id == (y as SharedFile).Id;
        }

        public void Update(DbFetch item)
        {
            return;
        }

        public class EntityConfiguration : System.Data.Entity.ModelConfiguration.EntityTypeConfiguration<SharedFile>
        {
            public EntityConfiguration()
            {
                HasKey(n => n.Id);
                HasRequired(n => n.User).WithMany(u => u.SharedFiles).HasForeignKey(n => n.UserId).WillCascadeOnDelete(true);
                HasRequired(n => n.SharedFolder).WithMany(u => u.SharedFiles).HasForeignKey(n => n.SharedFolderId).WillCascadeOnDelete(true);
                HasRequired(n => n.SharedFileObject).WithOptional(u => u.SharedFile).WillCascadeOnDelete(true);
                Property(n => n.Title).IsRequired();
                Property(n => n.Description).IsRequired();
                Property(n => n.DateCreated).IsRequired();
                Property(n => n.FileName).IsRequired();
            }
        }
    }
   
}
