using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CentralServiceCore.Data
{
    public class Feedback
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public string Output { get; set; }
        public string ClientId { get; set; }
        public System.DateTime DateCreated { get; set; }
        public virtual Workspace Workspace { get; set; }
    }
}
