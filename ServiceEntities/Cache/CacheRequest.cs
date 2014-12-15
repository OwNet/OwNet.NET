using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace ServiceEntities.Cache
{
    [DataContract(Namespace = @"http://imaginecup.fiit.stuba.sk/2012/", Name = "Request")]
    public class CacheRequest
    {
        [DataMember]
        public string AbsoluteUri { get; set; }
        [DataMember]
        public Dictionary<string, string> RequestHeaders { get; set; }
        [DataMember]
        public bool Refresh { get; set; }
        [DataMember]
        public DateTime? RefreshIfOlderThan { get; set; }
        [DataMember]
        public string Method { get; set; }
        [DataMember]
        public char[] PostData { get; set; }
        [DataMember]
        public virtual int CanCache { get; set; }
        [DataMember]
        public virtual bool CanCacheHtml { get; set; }
        [DataMember]
        public int RequestId { get; set; }
    }
}
