using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel.Channels;
using System.ServiceModel.Web;
using System.Text;
using System.Web;

namespace CentralRestService
{
    public class JsonHelper
    {
        public static Message Serialize(object obj)
        {
            string myResponseBody = JsonConvert.SerializeObject(obj);
            return WebOperationContext.Current.CreateTextResponse(myResponseBody,
                        "application/json; charset=utf-8",
                        Encoding.UTF8);
        }

        public static T Deserialize<T>(Stream stream)
        {
            return JsonConvert.DeserializeObject<T>(new StreamReader(stream).ReadToEnd());
        }

        public static T Deserialize<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}