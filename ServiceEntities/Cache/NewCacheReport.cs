using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace ServiceEntities.Cache
{
    [DataContract(Namespace = @"http://imaginecup.fiit.stuba.sk/2012/", Name = "NewCacheReport")]
    public class NewCacheReport
    {
        [DataMember]
        public string AbsoluteUri { get; set; }
        [DataMember]
        public DateTime DateModified { get; set; }
    }
}
