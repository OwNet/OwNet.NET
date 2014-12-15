using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace ServiceEntities.Cache
{
    [DataContract(Namespace = @"http://imaginecup.fiit.stuba.sk/2012/", Name = "CacheLinkToUpdate")]
    public class CacheLinkToUpdate
    {
        [DataMember]
        public string AbsoluteUri { get; set; }
        [DataMember]
        public int Priority { get; set; }
    }
}
