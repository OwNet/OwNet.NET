using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace ServiceEntities.Groups
{
    [DataContract(Namespace = @"http://imaginecup.fiit.stuba.sk/2012/", Name = "SimpleRecommendation")]
    public class SimpleRecommendation
    {
        [DataMember]
        public string Title { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public string AbsoluteUri { get; set; }

        [DataMember]
        public DateTime DateCreated { get; set; }
    }
}
