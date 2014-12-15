using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;

namespace WPFServer
{
    [ServiceContract(Namespace = @"http://imaginecup.fiit.stuba.sk/2012/")]
    interface IGroupsService
    {
         [OperationContract]
         bool CreateGroup(ServiceEntities.UserGroup userGroup);

         [OperationContract]
         ServiceEntities.GroupsList GetGroups(string user_id);

         [OperationContract]
         void JoinGroup(ServiceEntities.IdUG idUG);

         [OperationContract]
         void LeaveGroup(ServiceEntities.IdUG idUG);

         [OperationContract]
         ServiceEntities.Group GetGroup(string group_id, string user_id);

         [OperationContract]
         bool DeleteGroup(string group_id);

         [OperationContract]
         ServiceEntities.UserRecommendations GetAllActivities(string group, string page, string usersid);

         [OperationContract]
         List<ServiceEntities.AutocompletedGroup> AutocompletedGroups(string term);

         [OperationContract]
         ServiceEntities.GroupsList GetLastUsed(string user_id);

         [OperationContract]
         bool IsIn(string user_id, string group_id);

         [OperationContract]
         List<ServiceEntities.User> GetUsers(string group_id);

         [OperationContract]
         bool IsAdmin(string user_id, string group_id);

         [OperationContract]
         ServiceEntities.GroupsList GetAllGroups();

         [OperationContract]
         List<ServiceEntities.Recommendation> PreviousRecommendations(ServiceEntities.PageObject pageObject);

         [OperationContract]
         bool GetGroupByName(string group_name);

         [OperationContract]
         List<ServiceEntities.Group> GetLocalGroups(string since);

         [OperationContract]
         List<ServiceEntities.Groups.SimpleRecommendation> GetRecommendations(string groupId, string since);
    }
}
