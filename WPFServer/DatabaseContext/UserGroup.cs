using System;
using System.Collections.Generic;
using System.Data.Entity;

namespace WPFServer.DatabaseContext
{
    public class UserGroup :DbContext, DbFetch
    {

        public UserGroup()
        {
            DateCreated = DateTime.Now;
        }

        public int Id { get; set; }

        public DateTime DateCreated { get; set; }

        public int UserId { get; set; }
        public virtual User User { get; set; }

        public int GroupId { get; set; }
        public virtual Group Group { get; set; }

        public DateTime LastUsed { get; set; }

        public Table GetTableType() { return Table.UserGroup; }

        public System.Linq.Expressions.Expression<Func<DbFetch, bool>> GetCreateConstraint<DbFetch>()
        {
            return y => this.Id == (y as UserGroup).Id;
        }

        public void Update(DbFetch item)
        {
            return;
        }


    }
}
