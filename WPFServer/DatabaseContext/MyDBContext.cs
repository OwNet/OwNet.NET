using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;

namespace WPFServer.DatabaseContext
{
    public class Lock { }

    // Tabulky v databaze
    public enum Table
    {
        User,
        Page,
        UserVisitsPage,
        Tag,
        TagPage,
        SharedFile,
        SharedFolder,
        Group,
        UserGroup,
        GroupTag,
        AdminGroup,
        GroupRecommendation,
        GroupSharedFolder,
        CacheLink,
        Client,
        ClientCacheLink,
        AccessLog,
        GlobalCacheUpdate,
        Edge,
        UserTraversesEdge,
        UserSimilarity,
        Prediction,
		SharedFileObject,
		Cache,
		CacheUpdate,
		CacheHeader,
		Prefetch
    }

    public class MyDBContext : DbContext
    {

        private DbSet<Page> Pages { get { return Set<Page>(); } }
        private DbSet<User> Users { get { return Set<User>(); } }
        private DbSet<UserVisitsPage> Visits { get { return Set<UserVisitsPage>(); } }
        private DbSet<Tag> Tags { get { return Set<Tag>(); } }
        private DbSet<TagPage> TagPages { get { return Set<TagPage>(); } }
        private DbSet<Activity> Activities { get { return Set<Activity>(); } }
        private DbSet<SharedFile> SharedFiles { get { return Set<SharedFile>(); } }
        private DbSet<SharedFolder> SharedFolders { get { return Set<SharedFolder>(); } }
        private DbSet<UserGroup> UserGroups { get { return Set<UserGroup>(); } }
        private DbSet<Group> Groups { get { return Set<Group>(); } }
        private DbSet<GroupTag> GroupTags { get { return Set<GroupTag>(); } }
        private DbSet<AdminGroup> AdminGroups { get { return Set<AdminGroup>(); } }
        private DbSet<GroupRecommendation> GroupRecommendations { get { return Set<GroupRecommendation>(); } }
        private DbSet<GroupSharedFolder> GroupSharedFolders { get { return Set<GroupSharedFolder>(); } }
        private DbSet<Edge> Edges { get { return Set<Edge>(); } }
        private DbSet<UserTraversesEdge> Traverses { get { return Set<UserTraversesEdge>(); } }
        private DbSet<UserSimilarity> Similarities { get { return Set<UserSimilarity>(); } }
        private DbSet<Prediction> Predictions { get { return Set<Prediction>(); } }
        private DbSet<CacheLink> CacheLinks { get { return Set<CacheLink>(); } }
        private DbSet<Client> Clients { get { return Set<Client>(); } }
        private DbSet<ClientCacheLink> ClientCacheLinks { get { return Set<ClientCacheLink>(); } }
        private DbSet<AccessLog> AccessLogs { get { return Set<AccessLog>(); } }
        private DbSet<GlobalCacheUpdate> GlobalCacheUpdates { get { return Set<GlobalCacheUpdate>(); } }
        private DbSet<SharedFileObject> SharedFileObjects { get { return Set<SharedFileObject>(); } }
        private DbSet<Cache.CacheUpdate> CacheUpdates { get { return Set<Cache.CacheUpdate>(); } }
        private DbSet<Cache.CacheHeader> CacheHeaders { get { return Set<Cache.CacheHeader>(); } }
        private DbSet<Cache.Cache> Caches { get { return Set<Cache.Cache>(); } }
        private DbSet<Prefetch> Prefetchs { get { return Set<Prefetch>(); } }

        private static Dictionary<Table, Lock> locks = new Dictionary<Table, Lock>
        {
            { Table.Page, new Lock() },
            { Table.User, new Lock() },
            { Table.UserVisitsPage, new Lock() },
            { Table.Tag, new Lock() },
            { Table.TagPage, new Lock() },
            { Table.UserGroup, new Lock() },
            { Table.Group, new Lock() },
            { Table.GroupTag, new Lock() },
            { Table.AdminGroup, new Lock() },
            { Table.GroupRecommendation, new Lock() },
            { Table.GroupSharedFolder, new Lock() },
            { Table.Edge, new Lock() },
            { Table.UserTraversesEdge, new Lock() },
            { Table.UserSimilarity, new Lock() },
            { Table.Prediction, new Lock() },
            { Table.CacheLink, new Lock() },
            { Table.Client, new Lock() },
            { Table.ClientCacheLink, new Lock() },
            { Table.AccessLog, new Lock() },
            //{ Table.Cache, new Lock() },
            //{ Table.CacheHeader, new Lock() },
            //{ Table.CacheUpdate, new Lock() },
            { Table.GlobalCacheUpdate, new Lock() },
            { Table.SharedFileObject, new Lock() },
            { Table.Prefetch, new Lock() }
        };
        
        public static List<Table> Tables {
            get { return locks.Keys.ToList(); }
        } 
                
        public MyDBContext()
            : base("name=MyDBContext")      // connection string, ktory sa pouzije 
        {

            
        }

        public void Reset()
        {
            this.Database.ExecuteSqlCommand("DELETE FROM [Activity]");
            this.Database.ExecuteSqlCommand("DELETE FROM [UserTraversesEdge]");
            this.Database.ExecuteSqlCommand("DELETE FROM [Prediction]");
            this.Database.ExecuteSqlCommand("DELETE FROM [Edge]");
            this.Database.ExecuteSqlCommand("DELETE FROM [UserSimilarity]");
            this.Database.ExecuteSqlCommand("DELETE FROM [UserVisitsPage]");
            this.Database.ExecuteSqlCommand("DELETE FROM [TagPage]");
            this.Database.ExecuteSqlCommand("DELETE FROM [Tag]");
            this.Database.ExecuteSqlCommand("DELETE FROM [User]");
            this.Database.ExecuteSqlCommand("DELETE FROM [Page]");
            this.Database.ExecuteSqlCommand("DELETE FROM [SharedFile]");
            this.Database.ExecuteSqlCommand("DELETE FROM [SharedFolder]");
            this.Database.ExecuteSqlCommand("DELETE FROM [UserGroup]");
            this.Database.ExecuteSqlCommand("DELETE FROM [Group]");
            this.Database.ExecuteSqlCommand("DELETE FROM [GroupTag]");
            this.Database.ExecuteSqlCommand("DELETE FROM [AdminGroup]");
            this.Database.ExecuteSqlCommand("DELETE FROM [GroupRecommendation]");
            this.Database.ExecuteSqlCommand("DELETE FROM [GroupSharedFolder]");
            this.Database.ExecuteSqlCommand("DELETE FROM [CacheLink]");
            this.Database.ExecuteSqlCommand("DELETE FROM [Client]");
            this.Database.ExecuteSqlCommand("DELETE FROM [ClientCacheLink]");
            this.Database.ExecuteSqlCommand("DELETE FROM [AccessLog]");
            this.Database.ExecuteSqlCommand("DELETE FROM [GlobalCacheUpdate]");
            this.Database.ExecuteSqlCommand("DELETE FROM [SharedFileObject]");
            this.Database.ExecuteSqlCommand("DELETE FROM [CacheHeader]");
            this.Database.ExecuteSqlCommand("DELETE FROM [CacheUpdate]");
            this.Database.ExecuteSqlCommand("DELETE FROM [Cache]");
            this.Database.ExecuteSqlCommand("DELETE FROM [Prefetch]");
            this.AddDefaultPageGroups();
            this.SaveChanges();
			

           // this.AddDefaultPageGroups();

            this.SaveChanges();
        }


        public void AddDefaultPageGroups()
        {
            this.Groups.Add(new Group() { Name = "All", Id = 1, Location = false });
            this.Groups.Add(new Group() { Name = "School", Description = "School group", Id = 2, Location = false });
            this.Groups.Add(new Group() { Name = "Entertainment", Description = "Entertainment group", Id = 3, Location = false });
            this.Groups.Add(new Group() { Name = "Sport", Description = "Sport group", Id = 4, Location = false });
            this.Groups.Add(new Group() { Name = "News", Description = "News group", Id = 5, Location = false});
           
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<System.Data.Entity.ModelConfiguration.Conventions.PluralizingTableNameConvention>();

            modelBuilder.Configurations.Add(new UserVisitsPage.EntityConfiguration());
            modelBuilder.Configurations.Add(new Page.EntityConfiguration());
            modelBuilder.Configurations.Add(new User.EntityConfiguration());
            modelBuilder.Configurations.Add(new Tag.EntityConfiguration());
            modelBuilder.Configurations.Add(new TagPage.EntityConfiguration());
            modelBuilder.Configurations.Add(new Activity.EntityConfiguration());
            modelBuilder.Configurations.Add(new SharedFile.EntityConfiguration());
            modelBuilder.Configurations.Add(new SharedFolder.EntityConfiguraton());
            modelBuilder.Configurations.Add(new Edge.EntityConfiguration());
            modelBuilder.Configurations.Add(new UserTraversesEdge.EntityConfiguration());
            modelBuilder.Configurations.Add(new UserSimilarity.EntityConfiguration());
            modelBuilder.Configurations.Add(new Prediction.EntityConfiguration());
            modelBuilder.Configurations.Add(new SharedFileObject.EntityConfiguration());
            modelBuilder.Configurations.Add(new Prefetch.EntityConfiguration());
            modelBuilder.Configurations.Add(new Cache.Cache.EntityConfiguration());
            modelBuilder.Configurations.Add(new Cache.CacheHeader.EntityConfiguration());
            modelBuilder.Configurations.Add(new Cache.CacheUpdate.EntityConfiguration());
            modelBuilder.Configurations.Add(new CacheLink.EntityConfiguration());
            modelBuilder.Configurations.Add(new Client.EntityConfiguration());
            modelBuilder.Configurations.Add(new ClientCacheLink.EntityConfiguration());
            modelBuilder.Configurations.Add(new AccessLog.EntityConfiguration());
            modelBuilder.Configurations.Add(new GlobalCacheUpdate.EntityConfiguration());

            // UserGroup
            modelBuilder.Entity<UserGroup>().HasKey(n => n.Id);
            modelBuilder.Entity<UserGroup>().HasRequired(n => n.User).WithMany(u => u.UserGroups).HasForeignKey(n => n.UserId).WillCascadeOnDelete(true);
            modelBuilder.Entity<UserGroup>().HasRequired(n => n.Group).WithMany(u => u.UserGroups).HasForeignKey(n => n.GroupId).WillCascadeOnDelete(true);
            modelBuilder.Entity<UserGroup>().Property(n => n.DateCreated).IsRequired();

            // groupItem
            modelBuilder.Entity<Group>().HasKey(n => n.Id);

            modelBuilder.Entity<Group>().Property(n => n.Id).HasDatabaseGeneratedOption(System.ComponentModel.DataAnnotations.DatabaseGeneratedOption.None);
            modelBuilder.Entity<Group>().Property(n => n.Description).IsRequired();
            modelBuilder.Entity<Group>().Property(n => n.Name).IsRequired();
            modelBuilder.Entity<Group>().Property(n => n.DateCreated).IsRequired();
            modelBuilder.Entity<GroupTag>().HasRequired(n => n.Group).WithMany(u => u.GroupTags).HasForeignKey(n => n.GroupId).WillCascadeOnDelete(true);

            // groupTagItem
            modelBuilder.Entity<GroupTag>().HasKey(n => n.Id);
            modelBuilder.Entity<GroupTag>().Property(n => n.Value).IsRequired();
            modelBuilder.Entity<GroupTag>().Property(n => n.DateCreated).IsRequired();

            // AdminGroup
            modelBuilder.Entity<AdminGroup>().HasKey(n => n.Id);
            modelBuilder.Entity<AdminGroup>().HasRequired(n => n.User).WithMany(u => u.AdminGroups).HasForeignKey(n => n.UserId).WillCascadeOnDelete(true);
            modelBuilder.Entity<AdminGroup>().HasRequired(n => n.Group).WithMany(u => u.AdminGroups).HasForeignKey(n => n.GroupId).WillCascadeOnDelete(true);
            modelBuilder.Entity<AdminGroup>().Property(n => n.DateCreated).IsRequired();

            // GroupRecommendation
            modelBuilder.Entity<GroupRecommendation>().HasKey(n => n.Id);
            modelBuilder.Entity<GroupRecommendation>().HasRequired(n => n.Group).WithMany(u => u.Recommendations).HasForeignKey(n => n.GroupId).WillCascadeOnDelete(true);
            //modelBuilder.Entity<GroupRecommendationItem>().HasRequired(n => n.Recommendation).WithMany(u => u.Recommendations).HasForeignKey(n => n.RecommendationId).WillCascadeOnDelete(true);

            // GroupSharedFolder
            modelBuilder.Entity<GroupSharedFolder>().HasKey(n => n.Id);
            modelBuilder.Entity<GroupSharedFolder>().HasRequired(n => n.Group).WithMany(u => u.SharedFolders ).HasForeignKey(n => n.GroupId).WillCascadeOnDelete(true);
            modelBuilder.Entity<GroupSharedFolder>().HasRequired(n => n.SharedFolder).WithMany(u => u.SharedFolders).HasForeignKey(n => n.SharedFolderId).WillCascadeOnDelete(true);
        }

        public class DropCreateInitializer : IDatabaseInitializer<MyDBContext>
        {
            public void InitializeDatabase(MyDBContext context)
            {
             
                //context.Database.Delete();
                if (!context.Database.Exists() || !context.Database.CompatibleWithModel(true))
                {
                    if (context.Database.Exists()) context.Database.Delete();
                    context.Database.Create();
                    
                    // Initialize default items
                    context.AddDefaultPageGroups();
                    context.SaveChanges();

                    //   context.Database.ExecuteSqlCommand("CREATE UNIQUE INDEX IX_User_Username ON UserItems (Username)");
                }
            }
        }

        public IQueryable<T> Fetch<T>(System.Linq.Expressions.Expression<Func<T, bool>> criteria) where T : DbContext
        {
            IQueryable<T> ret = this.Set<T>().Where(criteria);
            return ret;
        }

        public DbSet<T> FetchSet<T>() where T : DbContext
        {
            DbSet<T> ret = this.Set<T>();
            return ret;
        }

        public bool FindAndRemove<T>(System.Linq.Expressions.Expression<Func<T, bool>> criteria) where T : DbContext
        {
            DbSet<T> set = this.FetchSet<T>();
            IQueryable<T> coll = set.Where(criteria);
            if (coll.Any())
            {
                set.Remove(coll.First());
                this.SaveChanges();
                return true;
            }

            return false;
        }

        public bool Remove<T>(T item) where T : DbContext, DbFetch
        {
            if (Fetch(item).Any())
            {
                DbSet<T> set = FetchSet<T>();
                set.Remove(item);

                this.SaveChanges();
                return true;
            }
            return false;
        }

        public bool RemoveWithoutSaving<T>(T item) where T : DbContext, DbFetch
        {
            if (Fetch(item).Any())
            {
                DbSet<T> set = FetchSet<T>();
                set.Remove(item);

                return true;
            }
            return false;
        }

        public IQueryable<T> Fetch<T>(T item) where T : DbContext, DbFetch
        {
            IQueryable<T> ret = Fetch<T>(item.GetCreateConstraint<T>());
            return ret;
        }

        public T Create<T>(T item) where T : DbContext, DbFetch
        {
            T ret = null;
            IQueryable<T> retlist = null;
            if (locks.ContainsKey(item.GetTableType()))
            {
                lock (locks[item.GetTableType()])
                {
                    try
                    {
                        retlist = Fetch<T>(item.GetCreateConstraint<T>());
                        if (retlist != null && retlist.Any())
                        {
                            ret = retlist.First();
                        }
                        else
                        {
                            ret = this.Set<T>().Add(item);
                            Activity news = null;
                            this.SaveChanges();

                            switch (item.GetTableType())
                            {
                                case Table.UserVisitsPage:
                                    UserVisitsPage uvp = item as UserVisitsPage;
                                    if (uvp.Rating == null || uvp.Rating < 1) break;
                                    news = CreateActivityItem(ActivityType.Rating, ActivityAction.Create, Convert.ToString(uvp.Rating) + " star" + ((int) uvp.Rating == 1 ? "" : "s"), uvp.User, true, uvp.Page);
                                    break;
                                case Table.GroupRecommendation:
                                    GroupRecommendation urpig = item as GroupRecommendation;
                                    news = CreateActivityItem(ActivityType.Recommend, ActivityAction.Create, urpig.Group.Name, urpig.User, true, urpig.Page, null, urpig.Group);
                                    break;
                                case Table.User:
                                    User ui = item as User;
                                    news = CreateActivityItem(ActivityType.Register, ActivityAction.Create, ui.IsTeacher ? "teacher" : "student", ui, true, null);
                                    break;
                                case Table.Group :

                                default:
                                    break;
                            }
                            if (news != null) this.RegisterActivity(news);


                        }
                    }
                    catch
                    {
                        throw;// TODO
                    }
                }
            }
            return ret;
        }

        public T FetchOrCreate<T>(T item, bool update = false) where T : DbContext, DbFetch
        {
            T ret = null;
            Activity news = null;
            IQueryable<T> retlist = Fetch<T>(item.GetCreateConstraint<T>());
            if (retlist != null && retlist.Any()) {
                ret = retlist.First();

                if (update)
                {
                    ret.Update(item);
                    this.SaveChanges();

                    switch (item.GetTableType())
                    {
                        case Table.UserVisitsPage :
                            UserVisitsPage uvp = item as UserVisitsPage;
                            if (uvp.Rating == null || uvp.Rating < 1) break;
                            news = CreateActivityItem(ActivityType.Rating, ActivityAction.Update, Convert.ToString(uvp.Rating) + " star" + (uvp.Rating == 1 ? "" : "s"), uvp.User, true, uvp.Page);
                            break;
                        case Table.GroupRecommendation :
                            GroupRecommendation urpig = item as GroupRecommendation;
                            news = CreateActivityItem(ActivityType.Recommend, ActivityAction.Update, urpig.Group.Name, urpig.User, true, urpig.Page, null, urpig.Group);
                            break;
                        default:
                            break;
                    }
                    if (news != null) this.RegisterActivity(news);
                    
                }
            }
            else 
            {
                ret = Create<T>(item);
            }
            
            return ret;
        }
        public Activity CreateActivityItem(ActivityType type, ActivityAction action, string message, User user, bool visible = true, Page page = null, SharedFile file = null, Group group = null)
        {
            Activity news = new Activity();
            news.Type = type;
            news.Action = action;
            news.Message = message;
            news.User = user;
            news.Group = group;
            news.Page = page;
            news.File = file;
            news.Visible = visible;
            return news;
        }

        public bool RegisterActivity(Activity item)     // acts like SPAM filter
        {
            if (item == null) return false;

          //  DateTime from = item.TimeStamp.AddMinutes(-1);
            int[] limitedTypesToOne = { (int)ActivityType.Recommend, (int)ActivityType.Rating };
            int[] limitedTypesToThree = { (int)ActivityType.Tag };
            DbSet<Activity> activities = this.FetchSet<Activity>();
            IQueryable<Activity> acts = null;
            if (item.Visible == true)
            {
                if (limitedTypesToThree.Contains(item.TypeValue))
                {
                    acts = activities.Where(a => a.PageId == item.Page.Id && a.UserId == item.User.Id && a.TypeValue == item.TypeValue).OrderByDescending(a => a.TimeStamp).Skip(2);
                }
                else if (limitedTypesToOne.Contains(item.TypeValue))
                {
                    acts = activities.Where(a => a.PageId == item.Page.Id && a.UserId == item.User.Id && a.TypeValue == item.TypeValue);
                    
                }
                if (acts != null)
                    foreach(Activity act in acts)
                    {
                        act.Visible = false;
                    }
            }
            activities.Add(item);

            this.SaveChanges();

            LiveStream.AddActivity(item);
            return true;
        }

    }

    public interface DbFetch {
        Table GetTableType();
        System.Linq.Expressions.Expression<Func<DbFetch, bool>> GetCreateConstraint<DbFetch>();
        void Update(DbFetch item);
    }

    public class MyDBContextException : System.ApplicationException
    {
        public MyDBContextException() { }
        public MyDBContextException(string message) : base(message) { }
        public MyDBContextException(string message, System.Exception inner) : base(message, inner) { }

        // Constructor needed for serialization 
        // when exception propagates from a remoting server to the client.
        protected MyDBContextException(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}