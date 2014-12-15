using System.ServiceModel;

namespace WPFServer
{
    [ServiceContract(Namespace = @"http://imaginecup.fiit.stuba.sk/2012/")]
    public interface IHistoryService
    {
        [OperationContract]
        void Create(ServiceEntities.UserVisitsPage move);

        [OperationContract]
        void CreateEdge(ServiceEntities.UserTraversesLink move);

        [OperationContract]
        void PrefetchCompleted(ServiceEntities.PageObject page);

        [OperationContract]
        ServiceEntities.PrefetchLinks PredictPersonalized(string usersid, string uri);

        [OperationContract]
        ServiceEntities.PrefetchLinks PredictNotPersonalized(string uri);

        [OperationContract]
        ServiceEntities.VisitedPages VisitedToday(string period, string usersid, string part);

        [OperationContract]
        bool Delete(string usersid, string pageid);
    }
}
