using System;
using System.Runtime.Serialization;

namespace WPFServer
{
    [DataContract(Namespace = @"http://imaginecup.fiit.stuba.sk/2012/", Name = "Activity")]
    class ActivityEntity : ServiceEntities.Activity
    {
        public ActivityEntity(DatabaseContext.Activity item) : base()
        {
            ActId = item.Id;
            DateTime = item.TimeStamp;
            Message = item.Message;
            User.Username = item.User.Username;
            User.Id = item.User.Id;
            User.Firstname = item.User.Firstname;
            User.Surname = item.User.Surname;
            User.IsTeacher = item.User.IsTeacher;
            Page.AbsoluteURI = (item.Page != null) ? item.Page.AbsoluteURI : "";
            Page.Title = (item.Page != null) ? item.Page.Title : "";
            Page.Id = (item.Page != null) ? item.Page.Id : 0;
            Action = (ServiceEntities.ActivityAction)((int)item.Action);
            if (item.FileId != null)
            {
                File.Title = (String.IsNullOrWhiteSpace(item.File.Title)) ? item.File.FileName : item.File.Title;
                File.FileName = item.File.FileName;
                File.FileObjectId = item.File.SharedFileObject.Id;
            }
            Type = (ServiceEntities.ActivityType)((int)item.Type);
        }
    }
}
