using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace ServiceEntities.CentralService.v3
{
    [DataContract]
    public class ReportLog
    {
        [DataMember]
        public List<ServiceEntities.CentralService.v2.AccessLogReport> AccessLogs { get; set; }

        [DataMember]
        public List<ServiceEntities.CentralService.v2.ActivityLogReport> ActivityLogs { get; set; }

        [DataMember]
        public List<ServiceEntities.CentralService.v2.GroupReport> GroupLogs { get; set; }

        [DataMember]
        public List<ServiceEntities.CentralService.v2.GroupRecommendationReport> GroupRecommendationLogs { get; set; }

        [DataMember]
        public List<UserReport> UserLogs { get; set; }

        [DataMember]
        public List<UserGroupReport> UserGroupReports { get; set; }
    }
}
