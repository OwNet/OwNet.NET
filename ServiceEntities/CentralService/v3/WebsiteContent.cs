using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace ServiceEntities.CentralService.v3
{
    [DataContract]
    public class WebsiteContent
    {
        [DataMember]
        public string Content { get; set; }

        [DataMember]
        public bool Success { get; set; }
    }
}
