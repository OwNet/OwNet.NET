using System;
using System.Collections.Generic;
using System.Data.Entity;

namespace WPFServer.DatabaseContext
{
    public class SharedFolderItem : DbContext, DbFetch
    {
        public SharedFolderItem()
        {
            Name = "";
            DateCreated = DateTime.Now;
        }
//        private int id = 0;
        public int Id { get; set; }//get { return id; } set { id = value; } }

//        private string name = "";
        public string Name { get; set; } //get { return name; } set { name = value; } }

//        private DateTime dateCreated = DateTime.Now;
        public DateTime DateCreated { get; set; } //get { return dateCreated; } set { dateCreated = value; } }

        public int? ParentFolderId { get; set; }
//        private SharedFolderItem parentFolder;
        public virtual SharedFolderItem ParentFolder
        {
            get;
            set;
            //          get { return parentFolder; }
            //          set {
            ////              parentFolder = value;
            //              if (parentFolder == null)
            //  /                ParentFolderId = null;
            //              else
            //                  ParentFolderId = parentFolder.Id;
            //          }
        }

        public virtual ICollection<SharedFileItem> SharedFiles { get; set; }
        public virtual ICollection<SharedFolderItem> ChildFolders { get; set; }

        public virtual ICollection<GroupSharedFolderItem> SharedFolders { get; set; }

        public Table GetTableType() { return Table.SharedFolderItem; }

        public System.Linq.Expressions.Expression<Func<DbFetch, bool>> GetCreateConstraint<DbFetch>()
        {
            return y => this.Id == (y as SharedFolderItem).Id;
        }

        public void Update(DbFetch item)
        {
            return;
        }
    }
}
