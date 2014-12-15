using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CentralServiceCore.Data
{
    using System;
    using System.Collections.Generic;

    public partial class ClientCache
    {
        public int Id { get; set; }
        public string ClientId { get; set; }
        public System.DateTime DateCreated { get; set; }
        public string Uid { get; set; }

        public virtual WebObject WebObject { get; set; }
        public virtual Workspace Workspace { get; set; }
    }
}
