using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using ServiceEntities.CentralService.v3;

namespace CentralServerShared.v3
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IPhoneAppService" in both code and config file together.
    [ServiceContract]
    public interface IPhoneAppService
    {
        [OperationContract]
        List<ServiceEntities.CentralService.v2.GroupReport> GetGroups(DateTime since, AuthenticatedUser authenticatedUser);

        [OperationContract]
        List<ServiceEntities.CentralService.v2.GroupRecommendationReport> GetGroupRecommendations(int groupId, DateTime since, AuthenticatedUser authenticatedUser);

        [OperationContract]
        WebsiteContent GetWebsiteContent(string url);

        [OperationContract]
        List<ServiceEntities.CentralService.v2.ServerInfo> GetAllServerInfos();

        [OperationContract]
        AuthenticateUserResult Authenticate(AuthenticateUserRequest request);
    }
}
