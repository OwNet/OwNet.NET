using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ServiceEntities.CentralService.v2
{
    [DataContract]
    public class Updates
    {
        [DataMember]
        public List<LinkToUpdate> LinksToUpdate { get; set; }
        [DataMember]
        public List<GroupReport> ModifiedGroupLogs { get; set; }
        [DataMember]
        public List<GroupRecommendationReport> NewGroupRecommendationLogs { get; set; }
    }

   [DataContract]
    public class LinkToUpdate
    {
        [DataMember]
        public string AbsoluteUri { get; set; }
        [DataMember]
        public int Priority { get; set; }
    }

    [DataContract]
    public class ServerInfo
    {
        [DataMember]
        public string Username { get; set; }
        [DataMember]
        public string Password { get; set; }
        [DataMember]
        public string ServerName { get; set; }
    }

    [DataContract]
    public class ReportLog
    {
        [DataMember]
        public List<AccessLogReport> AccessLogs { get; set; }
        [DataMember]
        public List<ActivityLogReport> ActivityLogs { get; set; }
        [DataMember]
        public List<GroupReport> GroupLogs { get; set; }
        [DataMember]
        public List<GroupRecommendationReport> GroupRecommendationLogs { get; set; }
    }

    [DataContract]
    public class GroupReport
    {
        [DataMember]
        public int Id { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public List<string> Tags { get; set; }
    }

    [DataContract]
    public class GroupRecommendationReport
    {
        
        [DataMember]
        public string AbsoluteUri { get; set; }
        [DataMember]
        public int GroupId { get; set; }
        [DataMember]
        public string Title { get; set; }
        [DataMember]
        public string Description { get; set; }
    }


    [DataContract]
    public class AccessLogReport
    {
        [DataMember]
        public string AbsoluteUri { get; set; }
        [DataMember]
        public int DownloadedFrom { get; set; }
        [DataMember]
        public DateTime AccessedAt { get; set; }
        [DataMember]
        public double FetchDuration { get; set; }
        [DataMember]
        public int Type { get; set; }
    }

    [DataContract]
    public class ActivityLogReport
    {
        [DataMember]
        public string AbsoluteURI { get; set; }
        [DataMember]
        public string Title { get; set; }
        [DataMember]
        public string Message { get; set; }
        [DataMember]
        public DateTime DateTime { get; set; }
        [DataMember]
        public ActivityAction Action { get; set; }
        [DataMember]
        public ActivityType Type { get; set; }
        [DataMember]
        public string UserFirstname { get; set; }
        [DataMember]
        public string UserSurname { get; set; }
    }
}
