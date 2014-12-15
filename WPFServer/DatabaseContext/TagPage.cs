using System;
using System.Data.Entity;

namespace WPFServer.DatabaseContext
{
    public class TagPage : DbContext, DbFetch
    {
        public int PageId { get; set; }

     //   private PageItem page;
        public virtual Page Page { get; set; }//get { return page; } set { page = value; PageId = value.Id; } }

        public int TagId { get; set; }

//        private TagItem tag;
        public virtual Tag Tag { get; set; }//get { return tag; } set { tag = value; TagId = value.Id; } }

        public Table GetTableType() { return Table.TagPage; }

        public System.Linq.Expressions.Expression<Func<DbFetch, bool>> GetCreateConstraint<DbFetch>()
        {
            return y => this.Page.AbsoluteURI.Equals((y as TagPage).Page.AbsoluteURI.ToLower())
                && this.Tag.Keyword.ToLower().Equals((y as TagPage).Tag.Keyword.ToLower());
        }
        public void Update(DbFetch item)
        {
            return;
        }


        public class EntityConfiguration : System.Data.Entity.ModelConfiguration.EntityTypeConfiguration<TagPage>
        {
            public EntityConfiguration()
            {
                HasRequired(t => t.Page).WithMany(p => p.Tags).HasForeignKey(t => t.PageId).WillCascadeOnDelete(true);
                HasRequired(t => t.Tag).WithMany(p => p.Pages).HasForeignKey(t => t.TagId).WillCascadeOnDelete(true);
                HasKey(t => new { t.TagId, t.PageId });
            }
        }
    }

    
}
