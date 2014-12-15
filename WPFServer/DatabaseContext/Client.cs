using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;

namespace WPFServer.DatabaseContext
{
    public class Client : DbContext, DbFetch
    {
        public Client()
        {
            ClientName = "";
            LastIP = "";
            DateCreated = DateTime.Now;
        }

        public int Id { get; set; }

        public string ClientName { get; set; }

        public string LastIP { get; set; }

        public DateTime DateCreated { get; set; }

        public virtual ICollection<ClientCacheLink> ClientCacheLinks { get; set; }

        public virtual ICollection<GlobalCacheUpdate> CacheUpdates { get; set; }

        public Table GetTableType() { return Table.Client; }

        public System.Linq.Expressions.Expression<Func<DbFetch, bool>> GetCreateConstraint<DbFetch>()
        {
            return y => this.ClientName.Equals((y as Client).ClientName);
        }

        public bool IsTheSameAs(Client item)
        {
            if (this.ClientName.Equals(item.ClientName)) return true;
            return false;
        }

        public void Update(DbFetch item)
        {
            return;
        }

        public class EntityConfiguration : System.Data.Entity.ModelConfiguration.EntityTypeConfiguration<Client>
        {
            public EntityConfiguration()
            {
                HasKey(n => n.Id);
                Property(n => n.ClientName).IsRequired();
                Property(n => n.DateCreated).IsRequired();
            }
        }
    }
}
