using System;
using System.Collections.Generic;
using System.Data.Entity;

namespace WPFServer.DatabaseContext
{
    public class GroupTag : DbContext, DbFetch
    {

        public GroupTag()
        {
            Value = "";
            DateCreated = DateTime.Now;
        }

        public int Id { get; set; }//get { return id; } set { id = value; } }


        public string Value { get; set; }//get { return title; } set { title = value; } }


        //        private DateTime dateCreated = DateTime.Now;
        public DateTime DateCreated { get; set; }//get { return dateCreated; } set { dateCreated = value; } }

        public Table GetTableType() { return Table.GroupTag; }

        public System.Linq.Expressions.Expression<Func<DbFetch, bool>> GetCreateConstraint<DbFetch>()
        {
            return y => this.Id == (y as GroupTag).Id;
        }

        public void Update(DbFetch item)
        {
            return;
        }

        public int GroupId { get; set; }
        //        private UserItem user;
        public virtual Group Group
        { get; set; }

    }
}
