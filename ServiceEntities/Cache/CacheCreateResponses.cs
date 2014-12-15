using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace ServiceEntities.Cache
{
    [CollectionDataContract(Namespace = @"http://imaginecup.fiit.stuba.sk/2012/", Name = "CacheCreateResponses", ItemName = "CacheCreateResponse")]
    public class CacheCreateResponses : List<CacheCreateResponse>
    {
        public CacheCreateResponses()
            : base()
        { }

        public CacheCreateResponses(List<CacheCreateResponse> other)
            : base(other)
        { }
    }
}
