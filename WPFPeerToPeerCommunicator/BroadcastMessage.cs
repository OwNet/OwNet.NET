using System;
using System.Runtime.Serialization;

namespace WPFPeerToPeerCommunicator
{
    [Serializable()]
    public class BroadcastMessage : ISerializable
    {
        public Version Version = null;
        public Version FirstCompatibleVersion = null;
        public string Hostname { get; set; }
        public string Username { get; set; }
        public int UserID = -1;
        public DateTime CreatedAt = DateTime.Now;

        public BroadcastMessage()
        {
            UserID = Settings.ID;
        }

        public BroadcastMessage(SerializationInfo info, StreamingContext ctxt)
        {
            //Get the values from info and assign them to the appropriate properties
            Version = Version.Parse((string)info.GetValue("Version", typeof(string)));
            FirstCompatibleVersion = Version.Parse((string)info.GetValue("FirstCompatibleVersion", typeof(string)));
            Hostname = (string)info.GetValue("Hostname", typeof(string));
            Username = (string)info.GetValue("Username", typeof(string));
            UserID = (int)info.GetValue("UserID", typeof(int));
        }
           
        //Serialization function.
        public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
        {
            //You can use any custom name for your name-value pair. But make sure you
            // read the values with the same name. For ex:- If you write EmpId as "EmployeeId"
            // then you should read the same with "EmployeeId"
            info.AddValue("Version", Settings.MessagesVersion.ToString());
            info.AddValue("FirstCompatibleVersion", Settings.MessagesFirstCompatibleVersion.ToString());
            info.AddValue("Hostname", Hostname);
            info.AddValue("Username", Username);
            info.AddValue("UserID", UserID);
        }
    }
}
