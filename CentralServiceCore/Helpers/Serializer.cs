using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace CentralServiceCore.Helpers
{
    public class Serializer
    {
        public static byte[] Serialize(object dict)
        {
            NetDataContractSerializer serializer = new NetDataContractSerializer();
            MemoryStream stream = new MemoryStream();
            serializer.Serialize(stream, dict);
            stream.Position = 0;
            return stream.ToArray();
        }

        public static T Deserialize<T>(byte[] serialized)
        {
            NetDataContractSerializer serializer = new NetDataContractSerializer();
            MemoryStream stream = new MemoryStream(serialized);
            return (T)serializer.Deserialize(stream);
        }
    }
}
