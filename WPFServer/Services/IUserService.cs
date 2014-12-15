using System.ServiceModel;

namespace WPFServer
{
    [ServiceContract(Namespace = @"http://imaginecup.fiit.stuba.sk/2012/")]
    public interface IUserService
    {
        [OperationContract]
        bool RegisterStudent(ServiceEntities.UserRegisters reg);

        [OperationContract]
        ServiceEntities.UserLoggedIn LogIn(ServiceEntities.UserLogsIn uwp);
        [OperationContract]
        ServiceEntities.RegisteredUsers List(string part);
        [OperationContract]
        ServiceEntities.RegisteredUsers ListGroup(string group, string part);
    //    [OperationContract]
    //    ServiceEntities.User ChangeRole(ServiceEntities.UserId uid);
        [OperationContract]
        bool DeleteUser(string id);
        [OperationContract]
        ServiceEntities.UserUpdated UpdateUser(string id, ServiceEntities.User uu);
        [OperationContract]
        bool ChangePassword(string id, ServiceEntities.UserUpdatesPassword uu);
        [OperationContract]
        ServiceEntities.User GetUser(string id);
    }
}
