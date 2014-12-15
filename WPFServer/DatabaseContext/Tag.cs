using System;
using System.Collections.Generic;
using System.Data.Entity;

namespace WPFServer.DatabaseContext
{
    public class Tag : DbContext, DbFetch
    {
        public Tag()
        {
            Keyword = "default";
        }
//        private int id = 0;
        public int Id {  get; set; }//get { return id; } set { id = value; } }

//        private string tag = "none";
        public string Keyword { get; set; } // get { return tag; } set { tag = value; Id = GetHashTag(tag); } }

        public virtual ICollection<TagPage> Pages { get; set; }

        public static int GetHashTag(string tag)
        {
            return tag.ToLower().GetHashCode();
        }

        public Table GetTableType() { return Table.Tag; }

        //public bool IsTheSameAs(TagItem item)
        //{
        //    return (item != null && this.E
        //}

        public System.Linq.Expressions.Expression<Func<DbFetch, bool>> GetCreateConstraint<DbFetch>()
        {
            return y => this.Keyword.ToLower().Equals((y as Tag).Keyword.ToLower());
        }

        public void Update(DbFetch item)
        {
            return;
        }


        public class EntityConfiguration : System.Data.Entity.ModelConfiguration.EntityTypeConfiguration<Tag>
        {
            public EntityConfiguration()
            {
                Property(t => t.Keyword).IsRequired();
                HasKey(t => t.Id);
                Property(t => t.Id).HasDatabaseGeneratedOption(System.ComponentModel.DataAnnotations.DatabaseGeneratedOption.Identity);
            }
        }
    }

   
}
