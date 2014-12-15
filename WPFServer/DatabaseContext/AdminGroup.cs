using System;
using System.Collections.Generic;
using System.Data.Entity;

namespace WPFServer.DatabaseContext
{
    public class AdminGroup : DbContext, DbFetch
    {

        public AdminGroup()
        {
            DateCreated = DateTime.Now;
        }
        // private int id = 0;
        public int Id { get; set; }//get { return id; } set { id = value; } }


        //        private DateTime dateCreated = DateTime.Now;
        public DateTime DateCreated { get; set; }//get { return dateCreated; } set { dateCreated = value; } }

        public int UserId { get; set; }
        //        private UserItem user;
        public virtual User User
        { get; set; }

        public int GroupId { get; set; }
        //        private UserItem user;
        public virtual Group Group
        { get; set; }

        public Table GetTableType() { return Table.AdminGroup; }

        public System.Linq.Expressions.Expression<Func<DbFetch, bool>> GetCreateConstraint<DbFetch>()
        {
            return y => this.Id == (y as AdminGroup).Id;
        }

        public void Update(DbFetch item)
        {
            return;
        }


    }
}

