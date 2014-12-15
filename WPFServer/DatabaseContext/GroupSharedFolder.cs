using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;

namespace WPFServer.DatabaseContext
{
    public class GroupSharedFolder : DbContext, DbFetch
    {

        GroupSharedFolder()
        {
            DateCreated = DateTime.Now;
        }


        public int Id { get; set; }//get { return id; } set { id = value; } }

        // private DateTime dateCreated = DateTime.Now;
        public DateTime DateCreated { get; set; }//get { return dateCreated; } set { dateCreated = value; } }

        public Table GetTableType() { return Table.GroupSharedFolder; }

        public System.Linq.Expressions.Expression<Func<DbFetch, bool>> GetCreateConstraint<DbFetch>()
        {
            return y => this.Id == (y as GroupSharedFolder).Id;
        }

        public void Update(DbFetch item)
        {
            return;
        }

        public int GroupId { get; set; }
        
        public virtual Group Group { get; set; }

        public int SharedFolderId { get; set; }

        public virtual SharedFolder SharedFolder { get; set; }

    }
}
