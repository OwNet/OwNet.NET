using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace ServiceEntities.Cache
{
    [DataContract(Namespace = @"http://imaginecup.fiit.stuba.sk/2012/", Name = "CacheLogsReport")]
    public class CacheLogsReport
    {
        [DataMember]
        public List<CacheLog> Logs { get; set; }
        [DataMember]
        public string ClientName { get; set; }
    }
}
