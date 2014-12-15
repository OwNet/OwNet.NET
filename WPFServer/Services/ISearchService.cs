using System.ServiceModel;
using System.Collections.Generic;

namespace WPFServer
{
    [ServiceContract(Namespace = @"http://imaginecup.fiit.stuba.sk/2012/")]
    public interface ISearchService
    {
        [OperationContract]
        ServiceEntities.ServerSearchResults Search(string query, string clients);

        [OperationContract]
        ServiceEntities.SearchResults SearchByTags(string query, string page);

        [OperationContract]
        List<int> CachedLinks(List<int> list);

        [OperationContract]
        ServiceEntities.PageObjectWithTags GetTags(string uri);

        [OperationContract]
        ServiceEntities.PageObjectWithTags SubmitTags(ServiceEntities.UserTagsPage tags);

        [OperationContract]
        ServiceEntities.TagCloud GetTagCloud();

        [OperationContract]
        bool DeleteTag(string tag);
    }
}
