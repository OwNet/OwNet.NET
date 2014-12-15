using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CentralServiceCore.Data
{
    public class SyncJournalState
    {
        public int Id { get; set; }
        public string ClientId { get; set; }
        public int? GroupId { get; set; }
        public int LastClientRecordNumber { get; set; }
        public System.DateTime DateCreated { get; set; }

        public virtual Workspace Workspace { get; set; }
    }
}
