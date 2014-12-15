using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace ServiceEntities.Cache
{
    [DataContract(Namespace = @"http://imaginecup.fiit.stuba.sk/2012/", Name = "CacheBatchRequest")]
    public class CacheBatchRequest
    {
        [DataMember]
        public List<CacheRequest> Requests { get; set; }
        [DataMember]
        public List<NewCacheReport> Reports { get; set; }
        [DataMember]
        public string ClientName { get; set; }
    }
}
