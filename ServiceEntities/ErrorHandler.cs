using System.Runtime.Serialization;

namespace ServiceEntities
{
    [DataContract]
    public class ErrorHandler
    {
        [DataMember]
        public int errorCode { get; set; }
        [DataMember]
        public string cause { get; set; }
        [DataMember]
        public string inner { get; set; }
    }
}