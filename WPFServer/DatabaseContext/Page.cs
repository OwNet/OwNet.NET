using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Web;

namespace WPFServer.DatabaseContext
{
    public class Page : DbContext, DbFetch      // stranka
    {
        public Page()
        {
            AbsoluteURI = "";
            Title = "";
        }
//        private int id = 0;
        public int Id { get; set; }//get { return id; } set { id = value; } }

        private string absoluteURI = "";
        public string AbsoluteURI { get { return absoluteURI; } set { absoluteURI = Page.CleanURI(HttpUtility.UrlDecode(value)); } }

//        private string title = "";
        public string Title { get; set; }//get { return title; } set { title = value; } }

        public virtual ICollection<UserVisitsPage> Visitors { get; set; }

       // private UserRecommendsPageInGroupItem recommendation = null;
       // public virtual UserRecommendsPageInGroup Recommendation { get; set; } //get { return recommendation; } set { if (value.PageId == this.Id) recommendation = value; } }

        public virtual ICollection<TagPage> Tags { get; set; }
        public virtual ICollection<Activity> Activities { get; set; }

		public virtual ICollection<GroupRecommendation> GroupRecommendations { get; set; }

        public virtual ICollection<Edge> EdgesFrom { get; set; }
        public virtual ICollection<Edge> EdgesTo { get; set; }

        public virtual ICollection<Prediction> Predictions { get; set; }

        public Table GetTableType() { return Table.Page; }
        public System.Linq.Expressions.Expression<Func<DbFetch, bool>> GetCreateConstraint<DbFetch>()
        {
            return y => this.AbsoluteURI.Equals((y as Page).AbsoluteURI);
        }

        public static int GetUriHash(string uri)
        {
            return uri.TrimEnd('/').GetHashCode();
        }

        public static string CleanURI(string uri)
        {
            string ret = System.Text.RegularExpressions.Regex.Replace(uri, "#.*$", "");
            return ret;
        }

        public bool IsTheSameAs(Page item)
        {
            if (this.AbsoluteURI.Equals(item.AbsoluteURI)) return true;
            return false;
        }

        public void Update(DbFetch item)
        {
            Page pageItem = item as Page;
            if (!String.IsNullOrWhiteSpace(pageItem.Title) && !this.Title.Equals(pageItem.Title)) this.Title = pageItem.Title;
        }
        public class EntityConfiguration : System.Data.Entity.ModelConfiguration.EntityTypeConfiguration<Page>
        {
            public EntityConfiguration()
            {
                Property(m => m.AbsoluteURI).IsRequired().HasMaxLength(2048);
                HasKey(m => m.Id);
                Property(m => m.Id).HasDatabaseGeneratedOption(System.ComponentModel.DataAnnotations.DatabaseGeneratedOption.Identity);
            }
        }

    }

   
}
