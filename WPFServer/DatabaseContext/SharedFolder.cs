using System;
using System.Collections.Generic;
using System.Data.Entity;

namespace WPFServer.DatabaseContext
{
    public class SharedFolder : DbContext, DbFetch
    {
        public SharedFolder()
        {
            Name = "";
            DateCreated = DateTime.Now;
        }

        public int Id { get; set; }

        public string Name { get; set; }

        public DateTime DateCreated { get; set; }

        public int? ParentFolderId { get; set; }

        public virtual SharedFolder ParentFolder
        { get; set; }

        public virtual ICollection<SharedFile> SharedFiles { get; set; }
        public virtual ICollection<SharedFolder> ChildFolders { get; set; }

		public virtual ICollection<GroupSharedFolder> SharedFolders { get; set; }

        public Table GetTableType() { return Table.SharedFolder; }

        public System.Linq.Expressions.Expression<Func<DbFetch, bool>> GetCreateConstraint<DbFetch>()
        {
            return y => this.Id == (y as SharedFolder).Id;
        }

        public void Update(DbFetch item)
        {
            return;
        }

        public class EntityConfiguraton : System.Data.Entity.ModelConfiguration.EntityTypeConfiguration<SharedFolder>
        {
            public EntityConfiguraton()
            {
                HasKey(n => n.Id);
                HasOptional(n => n.ParentFolder).WithMany(u => u.ChildFolders).HasForeignKey(n => n.ParentFolderId).WillCascadeOnDelete(false);
                Property(n => n.Name).IsRequired();
                Property(n => n.DateCreated).IsRequired();
            }
        }
    
    }

  
}
