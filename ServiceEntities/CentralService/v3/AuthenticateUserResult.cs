﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace ServiceEntities.CentralService.v3
{
    [DataContract]
    public class AuthenticateUserResult
    {
        [DataMember]
        public string Cookie { get; set; }

        [DataMember]
        public bool Success { get; set; }
    }
}
