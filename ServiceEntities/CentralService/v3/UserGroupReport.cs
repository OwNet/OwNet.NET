using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace ServiceEntities.CentralService.v3
{
    [DataContract]
    public class UserGroupReport
    {
        [DataMember]
        public string Username { get; set; }

        [DataMember]
        public int GroupId { get; set; }
    }
}
