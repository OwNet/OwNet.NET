using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using ServiceEntities.Cache;

namespace ServiceEntities
{

    [CollectionDataContract(Namespace = @"http://imaginecup.fiit.stuba.sk/2012/", Name = "PagesFrom", ItemName = "Page")]
    public class PageObjectList : List<PageObject> { }

    [CollectionDataContract(Namespace = @"http://imaginecup.fiit.stuba.sk/2012/", Name = "Groups", ItemName = "Group")]
    public class GroupsList : List<Group> { }

    [DataContract(Namespace = @"http://imaginecup.fiit.stuba.sk/2012/", Name = "ActivityList")]
    public class ActivityList : CollectionWithPaging
    {
        [DataMember]
        public List<Activity> Activities { get; set; }

        public ActivityList()
            : base()
        {
            Activities = new List<Activity>();
        }
    }

    [DataContract(Namespace = @"http://imaginecup.fiit.stuba.sk/2012/", Name = "Object")]
    public class PageObject
    {
        public PageObject()
        {
            AbsoluteURI = "";
            Title = "";
            Id = 0;
        }

        [DataMember]
        public int Id { get; set; }

        public string AbsoluteURI { get; set; }
        [DataMember(Name = "AbsoluteURI", EmitDefaultValue = false)]
        private CDataWrapper AbsoluteURICData
        {
            get { return AbsoluteURI; }
            set { AbsoluteURI = value; }
        }

        [DataMember]
        public string Title { get; set; }
    }

    [DataContract(Namespace = @"http://imaginecup.fiit.stuba.sk/2012/", Name = "Object")]
    public class PageObjectWithDateTime : PageObjectWithRating
    {
        public PageObjectWithDateTime()
            : base()
        {
            VisitTimeStamp = DateTime.Now;
        }
        [DataMember]
        public DateTime VisitTimeStamp { get; set; }
    }

    [DataContract(Namespace = @"http://imaginecup.fiit.stuba.sk/2012/", Name = "User")]
    public class UserId
    {
        public UserId()
        {
            Id = 0;
        }
        [DataMember]
        public int Id { get; set; }
    }

    [DataContract(Namespace = @"http://imaginecup.fiit.stuba.sk/2012/", Name = "User")]
    public class User : UserId
    {
        public User() : base()
        {
            Username = "";
            Firstname = "";
            Surname = "";
            IsTeacher = false;
            IsMale = true;
            Email = "";

        }
        public string Firstname { get; set; }
        [DataMember(Name = "Firstname", EmitDefaultValue = false)]
        private CDataWrapper FirstnameCData
        {
            get { return Firstname; }
            set { Firstname = value; }
        }
        public string Surname { get; set; }
        [DataMember(Name = "Surname", EmitDefaultValue = false)]
        private CDataWrapper SurnameCData
        {
            get { return Surname; }
            set { Surname = value; }
        }
          public string Username { get; set; }
        [DataMember(Name = "Username", EmitDefaultValue = false)]
        private CDataWrapper UsernameCData
        {
            get { return Username; }
            set { Username = value; } 
        }

        [DataMember]
        public bool IsTeacher { get; set; }
        [DataMember]
        public bool IsMale { get; set; }
        [DataMember]
        public string Email { get; set; }
        [DataMember]
        public DateTime Registered { get; set; }

    }

    [DataContract(Namespace = @"http://imaginecup.fiit.stuba.sk/2012/", Name = "Object")]
    public class PageObjectWithTags : PageObject
    {
        public PageObjectWithTags()
            : base()
        {
            Tags = new List<string>();
        }
        [DataMember]
        public List<string> Tags { get; set; }
    }

    [DataContract(Namespace = @"http://imaginecup.fiit.stuba.sk/2012/", Name = "Tagging")]
    public class UserTagsPage : UserActs
    {
        public UserTagsPage()
            : base()
        {
            Page = new PageObjectWithTags();
        }
        [DataMember]
        public PageObjectWithTags Page { get; set; }
    }

    [DataContract(Namespace = @"http://imaginecup.fiit.stuba.sk/2012/", Name = "UserLogin")]
    public class UserLogsIn
    {
        public UserLogsIn()
        {
            Username = "";
            Password = "";
        }
        [DataMember]
        public string Username { get; set; }
        [DataMember]
        public string Password { get; set; } 
    }

    [DataContract(Namespace = @"http://imaginecup.fiit.stuba.sk/2012/", Name = "UserLogged")]
    public class UserLoggedIn : UserActed
    {
        public UserLoggedIn()
            : base()
        {
            Success = false;
        }
        [DataMember]
        public bool Success { get; set; }
    }

    [DataContract(Namespace = @"http://imaginecup.fiit.stuba.sk/2012/", Name = "Object")]
    public class PageObjectWithRating : PageObject
    {
        public PageObjectWithRating()
            : base()
        {
            AvgRating = 0.0;
        }
        [DataMember]
        public double AvgRating { get; set; }
    }

    [DataContract(Namespace = @"http://imaginecup.fiit.stuba.sk/2012/", Name = "Recommendations")]
    public class VisitedPages : CollectionWithPaging
    {
        [DataMember]
        public List<PageObjectWithDateTime> Visits { get; set; }

        public VisitedPages()
            : base()
        {
            Visits = new List<PageObjectWithDateTime>();
        }
    }
    [DataContract(Namespace = @"http://imaginecup.fiit.stuba.sk/2012/", Name = "Users")]
    public class RegisteredUsers : CollectionWithPaging
    {
        [DataMember]
        public List<User> Users { get; set; }

        public RegisteredUsers()
            : base()
        {
            Users = new List<User>();
        }
    }

    [DataContract(Namespace = @"http://imaginecup.fiit.stuba.sk/2012/", Name = "Collection")]
    public class CollectionWithPaging
    {
        public CollectionWithPaging()
        {
            CurrentPage = 0;
            TotalPages = 0;
        }
        [DataMember]
        public int CurrentPage { get; set; }
        [DataMember]
        public int TotalPages { get; set; }
    }

    [DataContract(Namespace = @"http://imaginecup.fiit.stuba.sk/2012/", Name = "Ratings")]
    public class RatedPages : CollectionWithPaging
    {
        [DataMember]
        public List<PageObjectWithRating> Ratings { get; set; }

        public RatedPages()
            : base()
        {
            Ratings = new List<PageObjectWithRating>();
        }
    }

    [CollectionDataContract(Namespace = @"http://imaginecup.fiit.stuba.sk/2012/", Name = "TagCloud", ItemName = "TagWithCount")]
    public class TagCloud : Dictionary<string, int> { }

    [DataContract(Namespace = @"http://imaginecup.fiit.stuba.sk/2012/", Name = "Rating")]
    public class UserRatesPage : UserVisitsPage
    {
        public UserRatesPage()
            : base()
        {
            Rating = 0;
            Reason = ActivityType.Rating;
        }
        [DataMember]
        public int Rating { get; set; }
    }

    [DataContract(Namespace = @"http://imaginecup.fiit.stuba.sk/2012/", Name = "Rating")]
    public class UserVisitsPage : UserActs
    {
        public UserVisitsPage()
            : base()
        {
            Page = new PageObject();
            Reason = ActivityType.Visit;
        }
        [DataMember]
        public PageObject Page { get; set; }
        [DataMember]
        public ActivityType Reason { get; set; }
    }

    [DataContract(Namespace = @"http://imaginecup.fiit.stuba.sk/2012/", Name = "Rating")]
    public class UserTraversesLink : UserVisitsPage
    {
        public UserTraversesLink() : base()
        {
            From = new PageObject();
        }
        [DataMember]
        public PageObject From { get; set; }
    }


    [DataContract(Namespace = @"http://imaginecup.fiit.stuba.sk/2012/", Name = "Rating")]
    public class UserRatedPage : UserVisitedPage
    {
        public UserRatedPage()
            : base()
        {
            Rating = 0;
        }
        [DataMember]
        public int Rating { get; set; }
    }

    [DataContract(Namespace = @"http://imaginecup.fiit.stuba.sk/2012/", Name = "Rating")]
    public class UserVisitedPage : UserActed
    {
        public UserVisitedPage()
            : base()
        {
            Page = new PageObjectWithRating();
        }
        [DataMember]
        public PageObjectWithRating Page { get; set; }
    }

    [CollectionDataContract(Namespace = @"http://imaginecup.fiit.stuba.sk/2012/", Name = "SharedFolders", ItemName = "SharedFolder")]
    public class SharedFolderList : List<SharedFolder> { }

    [DataContract(Namespace = @"http://imaginecup.fiit.stuba.sk/2012/", Name = "SharedFolder")]
    public class SharedFolder
    {
        public SharedFolder()
        {
            Name = "";
            Id = -1;
            ParentFolderId = -1;
            Files = null;
        }
        [DataMember]
        public int Id { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public int ParentFolderId { get; set; }
        [DataMember]
        public SharedFolderList ChildFolders { get; set; }
        [DataMember]
        public SharedFileList Files { get; set; }
        [DataMember]
        public string FullPath { get; set; }

        public void AddFile(SharedFile file)
        {
            if (Files == null)
                Files = new SharedFileList();
            Files.Add(file);
        }

        public bool IsEmpty()
        {
            return (ChildFolders == null || ChildFolders.Count == 0) && (Files == null || Files.Count == 0);
        }
    }

    [CollectionDataContract(Namespace = @"http://imaginecup.fiit.stuba.sk/2012/", Name = "SharedFiles", ItemName = "SharedFile")]
    public class SharedFileList : List<SharedFile> { }

    [DataContract(Namespace = @"http://imaginecup.fiit.stuba.sk/2012/", Name = "SharedFile")]
    public class SharedFile : UserActs
    {
        public SharedFile()
            : base()
        {
        }

        [DataMember]
        public string Title { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public int FileObjectId { get; set; }
        [DataMember]
        public int SharedFolderId { get; set; }
        [DataMember]
        public string FileName { get; set; }
        [DataMember]
        public DateTime Created { get; set; }
    }


    [DataContract(Namespace = @"http://imaginecup.fiit.stuba.sk/2012/", Name = "Recommendation")]
    public class Recommendation
    {
        public Recommendation()
        {
            Group = new PageGroup();
            IsSet = false;
            Title = "";
            Description = "";
        }
        [DataMember]
        public bool IsSet { get; set; }
        [DataMember]
        public string Title { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public PageGroup Group { get; set; }
    }

    [DataContract(Namespace = @"http://imaginecup.fiit.stuba.sk/2012/", Name = "Register")]
    public class UserRegisters : User
    {
        public UserRegisters() {        
           Password = "";
        }
        [DataMember]
        public string Password { get; set; }
    }

    [DataContract(Namespace = @"http://imaginecup.fiit.stuba.sk/2012/", Name = "Update")]
    public class UserUpdatesPassword
    {
        public UserUpdatesPassword()
            : base()
        {
            OldPassword = "";
            Password = "";
            Reset = false;
        }
        [DataMember]
        public string OldPassword { get; set; }
        [DataMember]
        public string Password { get; set; }
        [DataMember]
        public bool Reset { get; set; }
    }


    [DataContract(Namespace = @"http://imaginecup.fiit.stuba.sk/2012/", Name = "Register")]
    public class UserUpdated : User
    {
        public UserUpdated()
            : base()
        {
            Success = false;
            //PasswordChanged = false;
        }
        [DataMember]
        public bool Success { get; set; }
        //[DataMember]
        //public bool PasswordChanged { get; set; }
    }

    [DataContract(Namespace = @"http://imaginecup.fiit.stuba.sk/2012/", Name = "Recommendation")]
    public class UserRecommendsPage : UserVisitsPage
    {
        public UserRecommendsPage()
            : base()
        {
            Recommendation = new Recommendation();
           // int GroupId;
            Reason = ActivityType.Recommend;
        }
        [DataMember]
        public Recommendation Recommendation { get; set; }

        [DataMember]
        public int GroupId { get; set; }
        [DataMember]
        public string GroupName { get; set; }


        
    }

    [DataContract(Namespace = @"http://imaginecup.fiit.stuba.sk/2012/", Name = "Recommendation")]
    public class UserRecommendedPage : UserActed
    {
        public UserRecommendedPage()
            : base()
        {
            Recommendation = new Recommendation();
            Page = new PageObject();
            RecommendationTimeStamp = DateTime.Now;
        }
        [DataMember]
        public Recommendation Recommendation { get; set; }
        [DataMember]
        public PageObject Page { get; set; }
        [DataMember]
        public DateTime RecommendationTimeStamp { get; set; }
    }

    [DataContract(Namespace = @"http://imaginecup.fiit.stuba.sk/2012/", Name = "Recommendations")]
    public class UserRecommendations : CollectionWithPaging
    {
        [DataMember]
        public List<UserRecommendedPageWithRating> Recommendations { get; set; }

        public UserRecommendations()
            : base()
        {
            Recommendations = new List<UserRecommendedPageWithRating>();
        }
    }

    [DataContract(Namespace = @"http://imaginecup.fiit.stuba.sk/2012/", Name = "Recommendation")]
    public class UserRecommendedPageWithRating : UserVisitedPage
    {
        public UserRecommendedPageWithRating()
            : base()
        {
            RecommendationTimeStamp = DateTime.Now;
            Visited = false;
            Recommendation = new Recommendation();
        }
        [DataMember]
        public bool Visited { get; set; }
        [DataMember]
        public DateTime RecommendationTimeStamp { get; set; }
        [DataMember]
        public Recommendation Recommendation { get; set; }
        [DataMember]
        public bool IsNew { get; set; }
    }


    [DataContract(Namespace = @"http://imaginecup.fiit.stuba.sk/2012/", Name = "Group")]
    public class PageGroup
    {
        public PageGroup()
        {
            Name = "";
            Id = 0;
        }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public int Id { get; set; }
    }

    [DataContract(Namespace = @"http://imaginecup.fiit.stuba.sk/2012/", Name = "UserAction")]
    public class UserActed
    {
        public UserActed()
        {
            User = new User();
        }
        [DataMember]
        public User User { get; set; }
    }

    [DataContract(Namespace = @"http://imaginecup.fiit.stuba.sk/2012/", Name = "UserAction")]
    public class UserActs
    {
        public UserActs() : base()
        {
            User = new UserId();
        }
        [DataMember]
        public UserId User { get; set; }
        [DataMember]
        public string Username { get; set; }
        [DataMember]
        public bool IsTeacher { get; set; }

        public int UserId
        {
            get { return User.Id; }
            set { User.Id = value; }
        }
    }

    [CollectionDataContract(Namespace = @"http://imaginecup.fiit.stuba.sk/2012/", Name = "SearchResultsCollection", ItemName = "Page")]
    public class SearchResultsCollection : List<PageObjectWithContent> { }

    [DataContract(Namespace = @"http://imaginecup.fiit.stuba.sk/2012/", Name = "Results")]
    public class SearchResults : CollectionWithPaging
    {
        [DataMember]
        public List<PageObjectWithContent> Results { get; set; }

        public SearchResults()
            : base()
        {
            Results = new List<PageObjectWithContent>();
        }
    }

    [DataContract(Namespace = @"http://imaginecup.fiit.stuba.sk/2012/", Name = "ServerSearchResults")]
    public class ServerSearchResults : CollectionWithPaging
    {
        [DataMember]
        public List<PageObjectWithContent> Results { get; set; }
        [DataMember]
        public Dictionary<string, int> SearchedClients { get; set; }
    }

    [DataContract(Namespace = @"http://imaginecup.fiit.stuba.sk/2012/", Name = "Page")]
    public class PageObjectWithContent : PageObject
    {
        public PageObjectWithContent()
            : base()
        {
            Content = "";
        }
        [DataMember]
        public string Content { get; set; }
        [DataMember]
        public int Rank { get; set; }
    }

    [DataContract(Namespace = @"http://imaginecup.fiit.stuba.sk/2012/", Name = "ActivityType")]
    public enum ActivityType : int
    {
        [EnumMember]
        Visit = 0,
        [EnumMember]
        Rating = 10,
        [EnumMember]
        Recommend = 20,
        [EnumMember]
        Tag = 30,
        [EnumMember]
        Share = 40,
        [EnumMember]
        Register = 50,
        [EnumMember]
        Search = 60,
        [EnumMember]
        Message = 70,
        [EnumMember]
        Group = 80,
        [EnumMember]
        None = 100

    }

    [DataContract(Namespace = @"http://imaginecup.fiit.stuba.sk/2012/", Name = "ActivityAction")]
    public enum ActivityAction : int
    {
        [EnumMember]
        Create = 10,
        [EnumMember]
        Read = 20,
        [EnumMember]
        Update = 30,
        [EnumMember]
        Delete = 40
    }

    [DataContract(Namespace = @"http://imaginecup.fiit.stuba.sk/2012/", Name = "Activity")]
    public class Activity : UserActed
    {
        public Activity()
            : base()
        {
            ActId = 0;
            Page = new PageObject();
            File = new SharedFile();
            Message = "";
            DateTime = DateTime.Now;
            Action = ActivityAction.Read;
            Type = ActivityType.Rating;
        }
        [DataMember]
        public int ActId { get; set; }
        [DataMember]
        public PageObject Page { get; set; }
        [DataMember]
        public SharedFile File { get; set; }
        [DataMember]
        public string Message { get; set; }
        [DataMember]
        public DateTime DateTime { get; set; }
        [DataMember]
        public ActivityAction Action { get; set; }
        [DataMember]
        public ActivityType Type { get; set; }

    }

    [DataContract(Namespace = @"http://imaginecup.fiit.stuba.sk/2012/", Name = "ActivityCount")]
    public class ActivityCount
    {
        [DataMember]
        public int Count { get; set; }
    }

    [DataContract(Namespace = @"http://imaginecup.fiit.stuba.sk/2012/", Name = "ServerSettings")]
    public class ServerSettings
    {
        [DataMember]
        public bool DownloadEverythingThroughServer { get; set; }
        [DataMember]
        public List<CacheLinkToUpdate> Updates { get; set; }
    }

    [DataContract(Namespace = @"http://imaginecup.fiit.stuba.sk/2012/", Name = "Links")]
    public class PrefetchLinks
    {
        public PrefetchLinks()
        {
            Links = new List<PageObjectWithRating>();
            From = new PageObject();
            Count = 0;
        }

        [DataMember]
        public List<PageObjectWithRating> Links { get; set; }

        [DataMember]
        public int Count { get; set; }
        
        [DataMember]
        public int Type { get; set; }
        
        [DataMember]
        public PageObject From { get; set; }
    }


    [DataContract(Namespace = @"http://imaginecup.fiit.stuba.sk/2012/", Name = "Group")]
    public class Group
    {
        public Group()
        {
            Id = 0;
            Name = "";
            Description = "";
            Tags = new List<string>();
        }
        [DataMember]
        public int Id { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public DateTime DateCreated { get; set; }
        [DataMember]
        public List<string> Tags { get; set; }
        [DataMember]
        public bool Location { get; set; }
    }


    [DataContract(Namespace = @"http://imaginecup.fiit.stuba.sk/2012/", Name = "UserGroup")]
    public class UserGroup : Group
    {
        public UserGroup()
        {
            UserId = 0;
        }
        [DataMember]
        public int UserId { get; set; }
        

        
    }

    /* id Usera a groupy */
    [DataContract(Namespace = @"http://imaginecup.fiit.stuba.sk/2012/", Name = "IdUG")]
    public class IdUG
    {
        public IdUG()
        {
            UserId = 0;
            GroupId = 0;
        }
        [DataMember]
        public int UserId { get; set; }
        [DataMember]
        public int GroupId { get; set; }

    }



    /* zoznam autoComplete group */
    [DataContract(Namespace = @"http://imaginecup.fiit.stuba.sk/2012/", Name = "AutocompletedGroup")]
    public class AutocompletedGroup
    {
       
        [DataMember]
        public string GroupName { get; set; }
        [DataMember]
        public int GroupId { get; set; }
    }


}

