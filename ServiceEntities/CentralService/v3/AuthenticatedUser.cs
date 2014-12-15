using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace ServiceEntities.CentralService.v3
{
    [DataContract]
    public class AuthenticatedUser
    {
        [DataMember]
        public string Username { get; set; }

        [DataMember]
        public string ServerUsername { get; set; }

        [DataMember]
        public string Cookie { get; set; }
    }
}