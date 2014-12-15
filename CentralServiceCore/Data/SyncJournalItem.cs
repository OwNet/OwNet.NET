using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CentralServiceCore.Data
{
    public class SyncJournalItem
    {
        public enum OperationTypes
        {
            InsertOrUpdate = 1,
            Delete = 2
        }

        public int Id { get; set; }
        public string ClientId { get; set; }
        public int ClientRecordNumber { get; set; }
        public string TableName { get; set; }
        public string Uid { get; set; }
        public int? GroupId { get; set; }
        public string SyncWith { get; set; }
        public string Columns { get; set; }
        public int OperationTypeValue { get; set; }
        public System.DateTime DateCreated { get; set; }

        public virtual Workspace Workspace { get; set; }

        public OperationTypes OperationType
        {
            get { return (OperationTypes)OperationTypeValue; }
            set { OperationTypeValue = (int)value; }
        }
    }
}
