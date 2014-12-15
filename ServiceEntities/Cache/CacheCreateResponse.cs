using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace ServiceEntities.Cache
{
    [DataContract(Namespace = @"http://imaginecup.fiit.stuba.sk/2012/", Name = "CacheCreateResponse")]
    public class CacheCreateResponse
    {
        [DataMember]
        public int RequestId { get; set; }
        [DataMember]
        public bool Found { get; set; }
        [DataMember]
        public int CacheID { get; set; }
        [DataMember]
        public string ClientIP { get; set; }
    }
}
