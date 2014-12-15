using CentralServiceCore.Data;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CentralServiceCore.Test.Helpers
{
    class DataModelContainerMock : Data.ICentralDataModelContainer
    {
        public DataModelContainerMock()
        {
            this.Workspaces = new FakeDbSet<Workspace>();
            this.AccessLogs = new FakeDbSet<AccessLog>();
            this.WebObjects = new FakeDbSet<WebObject>();
            this.RecommendedUpdates = new FakeDbSet<RecommendedUpdate>();
            this.PreviousUpdates = new FakeDbSet<PreviousUpdate>();
            this.ActivityLogs = new FakeDbSet<ActivityLog>();
            this.Groups = new FakeDbSet<Group>();
            this.Tags = new FakeDbSet<Tag>();
            this.Recommendations = new FakeDbSet<Recommendation>();
            this.GroupServers = new FakeDbSet<GroupServer>();
            this.Predictions = new FakeDbSet<Prediction>();
            this.UserCookies = new FakeDbSet<UserCookie>();
            this.UserGroups = new FakeDbSet<UserGroup>();
            this.SyncJournalItems = new FakeDbSet<SyncJournalItem>();
            this.SyncJournalStates = new FakeDbSet<SyncJournalState>();
            this.ClientCaches = new FakeDbSet<ClientCache>();
            this.Feedbacks = new FakeDbSet<Feedback>();
        }

        public IDbSet<Workspace> Workspaces { get; private set; }
        public IDbSet<AccessLog> AccessLogs { get; private set; }
        public IDbSet<WebObject> WebObjects { get; private set; }
        public IDbSet<RecommendedUpdate> RecommendedUpdates { get; private set; }
        public IDbSet<PreviousUpdate> PreviousUpdates { get; private set; }
        public IDbSet<ActivityLog> ActivityLogs { get; private set; }
        public IDbSet<Group> Groups { get; private set; }
        public IDbSet<Tag> Tags { get; private set; }
        public IDbSet<Recommendation> Recommendations { get; private set; }
        public IDbSet<GroupServer> GroupServers { get; private set; }
        public IDbSet<Prediction> Predictions { get; private set; }
        public IDbSet<User> Users { get; private set; }
        public IDbSet<UserCookie> UserCookies { get; private set; }
        public IDbSet<UserGroup> UserGroups { get; private set; }
        public IDbSet<SyncJournalItem> SyncJournalItems { get; private set;  }
        public IDbSet<SyncJournalState> SyncJournalStates { get; private set; }
        public IDbSet<ClientCache> ClientCaches { get; private set; }
        public IDbSet<Feedback> Feedbacks { get; set; }

        public int SaveChanges()
        {
            return 0;
        }
    }
}
