using System;
using System.Runtime.Serialization;

namespace WPFPeerToPeerCommunicator
{
    [Serializable()]
    public class ReplyMessage : ISerializable
    {
        public string Hostname = null;
        public int UserID = -1;
        public string[] RecommendedLinks = null;

        public ReplyMessage()
        {
            UserID = Settings.ID;
        }

        public ReplyMessage(SerializationInfo info, StreamingContext ctxt)
        {
            Hostname = (string)info.GetValue("Hostname", typeof(string));
            UserID = (int)info.GetValue("UserID", typeof(int));
            RecommendedLinks = (string[])info.GetValue("RecommendedLinks", typeof(string[]));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
        {
            info.AddValue("Hostname", Hostname);
            info.AddValue("UserID", UserID);
            info.AddValue("RecommendedLinks", RecommendedLinks);
        }
    }
}
