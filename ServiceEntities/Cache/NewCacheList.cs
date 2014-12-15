using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace ServiceEntities.Cache
{
    [DataContract(Namespace = @"http://imaginecup.fiit.stuba.sk/2012/", Name = "NewCaches")]
    public class NewCacheList
    {
        [DataMember]
        public Dictionary<int, DateTime> Items { get; set; }
    }
}
