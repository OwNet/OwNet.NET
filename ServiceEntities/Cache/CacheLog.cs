using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace ServiceEntities.Cache
{
    [DataContract(Namespace = @"http://imaginecup.fiit.stuba.sk/2012/", Name = "CacheLog")]
    public class CacheLog
    {
        [DataMember]
        public string AbsoluteUri { get; set; }
        [DataMember]
        public int DownloadedFrom { get; set; }
        [DataMember]
        public DateTime AccessedAt { get; set; }
        [DataMember]
        public double FetchDuration { get; set; }
        [DataMember]
        public int Type { get; set; }
    }
}
