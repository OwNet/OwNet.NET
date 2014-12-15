using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Objects;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CentralServiceCore.Data
{
    public interface ICentralDataModelContainer
    {
        IDbSet<Workspace> Workspaces { get; }
        IDbSet<AccessLog> AccessLogs { get; }
        IDbSet<WebObject> WebObjects { get; }
        IDbSet<RecommendedUpdate> RecommendedUpdates { get; }
        IDbSet<PreviousUpdate> PreviousUpdates { get; }
        IDbSet<ActivityLog> ActivityLogs { get; }
        IDbSet<Group> Groups { get; }
        IDbSet<Tag> Tags { get; }
        IDbSet<Recommendation> Recommendations { get; }
        IDbSet<GroupServer> GroupServers { get; }
        IDbSet<Prediction> Predictions { get; }
        IDbSet<User> Users { get; }
        IDbSet<UserCookie> UserCookies { get; }
        IDbSet<UserGroup> UserGroups { get; }
        IDbSet<SyncJournalItem> SyncJournalItems { get; }
        IDbSet<SyncJournalState> SyncJournalStates { get; }
        IDbSet<ClientCache> ClientCaches { get; }
        IDbSet<Feedback> Feedbacks { get; set; }

        int SaveChanges();
    }
}
