using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;

namespace CentralServiceCore.Data
{
    public partial class CentralDataModelContainer : DbContext, ICentralDataModelContainer
    {
        public CentralDataModelContainer()
            : base("CentralDataModelContainer")
        {
        }

        public IDbSet<Workspace> Workspaces { get; set; }
        public IDbSet<AccessLog> AccessLogs { get; set; }
        public IDbSet<WebObject> WebObjects { get; set; }
        public IDbSet<RecommendedUpdate> RecommendedUpdates { get; set; }
        public IDbSet<PreviousUpdate> PreviousUpdates { get; set; }
        public IDbSet<ActivityLog> ActivityLogs { get; set; }
        public IDbSet<Group> Groups { get; set; }
        public IDbSet<Tag> Tags { get; set; }
        public IDbSet<Recommendation> Recommendations { get; set; }
        public IDbSet<GroupServer> GroupServers { get; set; }
        public IDbSet<Prediction> Predictions { get; set; }
        public IDbSet<User> Users { get; set; }
        public IDbSet<UserCookie> UserCookies { get; set; }
        public IDbSet<UserGroup> UserGroups { get; set; }
        public IDbSet<SyncJournalItem> SyncJournalItems { get; set; }
        public IDbSet<SyncJournalState> SyncJournalStates { get; set; }
        public IDbSet<ClientCache> ClientCaches { get; set; }
        public IDbSet<Feedback> Feedbacks { get; set; }
    }
}
