using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;

namespace WPFServer.DatabaseContext
{
    
    public class Prediction : DbContext, DbFetch
    {
        public Prediction() 
        {
            TimeStamp = DateTime.Now;
        }

        public int Id {  get; set; }
        

        public virtual Page Page { get; set; }

        public int PageId { get; set; }

        public virtual User User { get; set; }
        
        public int? UserId { get; set; }

        public DateTime TimeStamp { get; set; }

        public Table GetTableType() { return Table.Prediction; }

        public System.Linq.Expressions.Expression<Func<DbFetch, bool>> GetCreateConstraint<DbFetch>()
        {
            return y => false;//this.Page.AbsoluteURI.Equals((y as Prediction).Page.AbsoluteURI) && (this.User != null && (y as Prediction).User != null && this.User.Username.ToLower().Equals((y as Prediction).User.Username.ToLower())) && this.TimeStamp >= ((y as Prediction).TimeStamp);
        }

        public void Update(DbFetch item)
        {
            return;
        }


        public class EntityConfiguration : System.Data.Entity.ModelConfiguration.EntityTypeConfiguration<Prediction>
        {
            public EntityConfiguration()
            {
                HasRequired(e => e.Page).WithMany(l => l.Predictions).HasForeignKey(m => m.PageId).WillCascadeOnDelete(true);
                HasOptional(e => e.User).WithMany(l => l.Predictions).HasForeignKey(m => m.UserId).WillCascadeOnDelete(true);
                HasKey(m => m.Id);
                Property(m => m.Id).HasDatabaseGeneratedOption(System.ComponentModel.DataAnnotations.DatabaseGeneratedOption.Identity);
            }
        }
    }

   
}

