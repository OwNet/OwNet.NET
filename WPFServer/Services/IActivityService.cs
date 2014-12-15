using System.ServiceModel;

namespace WPFServer
{
    [ServiceContract(Namespace = @"http://imaginecup.fiit.stuba.sk/2012/")]
    public interface IActivityService
    {
        [OperationContract]
        ServiceEntities.UserRatedPage GetRating(string usersid, string uri);
        [OperationContract]
        void SetRating(ServiceEntities.UserRatesPage rating);
        [OperationContract]
        void SetUsage(ServiceEntities.UserVisitsPage move);
       
        [OperationContract]
        int RecommendPage(ServiceEntities.UserRecommendsPage urp);

      /*  [OperationContract]
        ServiceEntities.UserRecommendedPage GetRecommendedPage(string url);*/

        [OperationContract]
        bool DeleteRecommendedPage(string gid, string pid);

        [OperationContract]
        ServiceEntities.UserRecommendations GetRecommendedInGroupWithVisitedInformation(string group, string page, string usersid);
      //  [OperationContract]
     //   ServiceEntities.UserRecommendations GetRecommendedInGroup(string group, string page);

//        [OperationContract]
        //        ServiceEntities.UserRecommendations GetRecommendedTopRated();

     //   [OperationContract]
     //   ServiceEntities.UserRecommendations GetRecommendedMostRecent();
        [OperationContract]
        ServiceEntities.UserRecommendations GetRecommendedMostRecentWithVisitedInformation(string usersid);
        
        [OperationContract]
        ServiceEntities.RatedPages GetAllTopRated(string part);
        
        [OperationContract]
        ServiceEntities.RatedPages GetUserTopRated(string usersid, string part);

        //[OperationContract]
        //ServiceEntities.ActivityList GetActivities(string usersid, string page);

        [OperationContract]
        ServiceEntities.ActivityList GetAllActivities(string page);

        //[OperationContract]
        //ServiceEntities.ActivityCount GetActivitiesCount(string usersid);

        [OperationContract]
        void ShareFile(ServiceEntities.SharedFile usf);

        [OperationContract]
        ServiceEntities.SharedFolder CreateSharedFolder(ServiceEntities.SharedFolder sharedFolder);

        [OperationContract]
        void RenameSharedFolder(ServiceEntities.SharedFolder sharedFolder);

        [OperationContract]
        void DeleteSharedFolder(ServiceEntities.SharedFolder sharedFolder);

        [OperationContract]
        void DeleteSharedFile(string fileId);

        [OperationContract]
        ServiceEntities.SharedFolder GetSharedFolderStructure();

        [OperationContract]
        ServiceEntities.SharedFolder GetSharedFolder(string folderId);

        [OperationContract]
        bool HideActivity(string id);

        //[OperationContract]
        //ServiceEntities.UserRecommendations GetRecommendedAll(string page);

        [OperationContract]
        ServiceEntities.ActivityList GetLiveStream();

        [OperationContract]
        ServiceEntities.ActivityList GetLiveStreamSince(string since);

        [OperationContract]
        void CreateMessage(ServiceEntities.Activity activity);

        [OperationContract]
        int GetCountNotDisplayedRecomms(string usersid);
    }
}
