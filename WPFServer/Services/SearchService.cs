using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Web;
using System.Windows.Documents;
using ServiceEntities;
using WPFServer.DatabaseContext;
using WPFServer.Clients;
using ClientAndServerShared;

namespace WPFServer
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(UseSynchronizationContext = false, InstanceContextMode = InstanceContextMode.PerCall)]
    public class SearchService : ISearchService
    {

        [WebGet(UriTemplate = "?query={query}&clients={clients}")]
        public ServerSearchResults Search(string query, string clients)
        {
            Dictionary<string, int> searchedClients = null;
            if (clients == "")
            {
                searchedClients = new Dictionary<string, int>();
                foreach (string clientName in ClientsController.GetAvailableClients())
                    searchedClients[clientName] = 0;
            }
            else
            {
                searchedClients = Helpers.ConvertDictionary.GetDict(clients);
            }

            ServerSearchResults results = ClientSearch.SearchOnClients(query, searchedClients);
            results.SearchedClients = searchedClients;
            return results ?? new ServerSearchResults();
        }

        [WebGet(UriTemplate = "tag/?query={query}&page={page}")]
        public SearchResults SearchByTags(string query, string page)
        {
            SearchResults results = new SearchResults();
            try
            {
                int pag = Convert.ToInt32(page);
                PageObjectWithContent pobj;
                query = HttpUtility.UrlDecode(query);
                string[] tags = query.Split(' ');
                List<string> ids = new List<string>();
                string id;
                foreach (string tag in tags)
                {
                    id = tag.ToLower();
                    if (!ids.Contains(id)) ids.Add(id);
                   // id = DatabaseContext.TagItem.GetHashTag(tag);
                   // if (!ids.Contains(id)) ids.Add(id);
                    
                }

                using (DatabaseContext.MyDBContext con = new DatabaseContext.MyDBContext())
                {
                    try
                    {
                        IQueryable<DatabaseContext.Tag> tagItems = con.Fetch<DatabaseContext.Tag>(t => ids.Contains(t.Keyword.ToLower()));
                        List<int> ints = new List<int>();
                        foreach (DatabaseContext.Tag tagid in tagItems)
                        {
                            ints.Add(tagid.Id);
                        }
                        IQueryable<DatabaseContext.TagPage> tagPageItems = con.Fetch<DatabaseContext.TagPage>(t => ints.Contains(t.TagId));
                        IQueryable<DatabaseContext.Page> pagesQuery = tagPageItems.Select(p => p.Page).Distinct().OrderByDescending(p => p.Tags.Count);

                        int currentPage = pag;
                        int totalPages = pag;

                        Helpers.Search.ProcessPages(pagesQuery.Count(), pag, out totalPages, out currentPage);

                        results.TotalPages = totalPages;
                        results.CurrentPage = currentPage;

                        Helpers.Search.ExtractPage(ref pagesQuery, currentPage);

                        List<DatabaseContext.Page> pages = pagesQuery.ToList();
                        
                        foreach (DatabaseContext.Page item in pages)
                        {
                            pobj = new PageObjectWithContent();
                            pobj.AbsoluteURI = item.AbsoluteURI;
                            pobj.Title = item.Title;
                            pobj.Content = "Tags:";
                            pobj.Id = item.Id;

                            IQueryable<DatabaseContext.Tag> pageTags = tagPageItems.Where(t => t.PageId == item.Id).Select(t => t.Tag);
                            foreach (DatabaseContext.Tag tag in pageTags)
                            {
                                pobj.Content += " " + tag.Keyword + ",";
                            }
                            pobj.Content = pobj.Content.TrimEnd(',');

                            results.Results.Add(pobj);
                        }
                    }
                    catch (Exception) { }
                }
            }
            catch (Exception e)
            {
                throw new WebFaultException<ErrorHandler>(new ErrorHandler { cause = e.Message, inner = e.InnerException != null ? e.InnerException.Message : "", errorCode = 500 }, System.Net.HttpStatusCode.InternalServerError);
            }
            return results;
        }
        
        [WebInvoke(UriTemplate = "links/", Method = "POST")]
        public List<int> CachedLinks(List<int> list)
        {
            List<int> retList = null;
            try
            {
                using (MyDBContext context = new MyDBContext())
                {
                    var query = (from p in context.FetchSet<CacheLink>()
                                 where list.Contains(p.Id)
                                 select p.Id);
                    retList = query.ToList();
                }
            }
            catch (Exception ex)
            {
                LogsController.WriteException("GetCachedLinks", ex.Message);
                return new List<int>();
            }
            return retList;
        }

        [WebGet(UriTemplate = "tag/list/?page={uri}")]
        public PageObjectWithTags GetTags(string uri)
        {
           
            PageObjectWithTags ret = new PageObjectWithTags();
            ret.Tags = new List<string>();
            ret.AbsoluteURI = "";

            try
            {
                using (DatabaseContext.MyDBContext con = new DatabaseContext.MyDBContext())
                {
                    try
                    {
                        IQueryable<DatabaseContext.Page> pages = con.Fetch<DatabaseContext.Page>(new DatabaseContext.Page() { AbsoluteURI = uri });
                        if (pages.Any())
                        {
                            DatabaseContext.Page page = pages.First();
                            ret.AbsoluteURI = page.AbsoluteURI;
                            ret.Id = page.Id;
                            ICollection<DatabaseContext.TagPage> tagpagesQuery = page.Tags;
                            List<DatabaseContext.TagPage> tagpages = tagpagesQuery.ToList();

                            foreach (DatabaseContext.TagPage tagpage in tagpages)
                            {
                                ret.Tags.Add(tagpage.Tag.Keyword);
                            }
                        }
                    }
                    catch (Exception) { }
                }
            }
            catch (Exception e)
            {
                throw new WebFaultException<ErrorHandler>(new ErrorHandler { cause = e.Message, inner = e.InnerException != null ? e.InnerException.Message : "", errorCode = 500 }, System.Net.HttpStatusCode.InternalServerError);
            }
          
            return ret;
        }

        [WebInvoke(UriTemplate = "tag/list/", Method="POST")]
        public PageObjectWithTags SubmitTags(UserTagsPage tags)
        {
            PageObjectWithTags ret = new PageObjectWithTags();
            ret.Tags = new List<string>();
            ret.AbsoluteURI = tags.Page.AbsoluteURI;
            ret.Id = tags.Page.Id;
            string msg = "";
            try
            {

                using (DatabaseContext.MyDBContext con = new DatabaseContext.MyDBContext())
                {
                    try
                    {
                        IQueryable<DatabaseContext.User> users = con.Fetch<DatabaseContext.User>(u => u.Id == tags.User.Id);
                        if (!users.Any())
                            return ret;
                        DatabaseContext.User user = users.First();
                        DatabaseContext.Page page = con.FetchOrCreate<DatabaseContext.Page>(new DatabaseContext.Page() { AbsoluteURI = tags.Page.AbsoluteURI, Title = tags.Page.Title }, true);
                        bool tagged = false;
                        foreach (string tag in tags.Page.Tags)
                        {
                            DatabaseContext.Tag tagItem = con.FetchOrCreate<DatabaseContext.Tag>(new DatabaseContext.Tag() { Keyword = tag });
                            if (!con.Fetch<DatabaseContext.TagPage>(u => u.TagId == tagItem.Id && u.PageId == page.Id).Any())
                            {
                                DatabaseContext.TagPage tagPageItem = con.FetchOrCreate<DatabaseContext.TagPage>(new DatabaseContext.TagPage() { Tag = tagItem, Page = page });
                                msg += " " + tag + ",";
                                tagged = true;
                            }
                        }
                        if (tagged)
                        {
                            DatabaseContext.Activity act = con.CreateActivityItem(DatabaseContext.ActivityType.Tag, DatabaseContext.ActivityAction.Create, msg.TrimStart(' ').TrimEnd(','), user,true, page);
                            con.RegisterActivity(act);
                            con.SaveChanges();
                        }
                        if (page.Tags.Any())
                        {
                            IQueryable<DatabaseContext.TagPage> tagpagesQuery = con.Fetch<DatabaseContext.TagPage>(y => y.PageId == page.Id);
                            List<DatabaseContext.TagPage> tagpages = tagpagesQuery.ToList();
                            foreach (DatabaseContext.TagPage tagpage in tagpages)
                            {
                                ret.Tags.Add(tagpage.Tag.Keyword);
                            }
                        }
                    }
                    catch (Exception) { }
                }
            }
            catch (Exception e)
            {
                throw new WebFaultException<ErrorHandler>(new ErrorHandler { cause = e.Message, inner = e.InnerException != null ? e.InnerException.Message : "", errorCode = 500 }, System.Net.HttpStatusCode.InternalServerError);
            }
            return ret;
        }

        [WebInvoke(UriTemplate = "tag/{tag}/", Method = "DELETE")]
        public bool DeleteTag(string tag)
        {
            try
            {
                using (DatabaseContext.MyDBContext con = new DatabaseContext.MyDBContext())
                {
                    try
                    {
                        IQueryable<DatabaseContext.Tag> tags = con.Fetch<DatabaseContext.Tag>(new DatabaseContext.Tag() { Keyword = tag });
                        if (tags.Any())
                        {
                            DatabaseContext.Tag tagItem = tags.First();

                            con.Remove<DatabaseContext.Tag>(tagItem);

                            return true;
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            catch (DatabaseContext.MyDBContextException e)
            {
                throw new WebFaultException<ServiceEntities.ErrorHandler>(new ServiceEntities.ErrorHandler { cause = e.Message, inner = e.InnerException != null ? e.InnerException.Message : "", errorCode = 500 }, System.Net.HttpStatusCode.InternalServerError);
            }
            catch (Exception e)
            {
                throw new WebFaultException<ServiceEntities.ErrorHandler>(new ServiceEntities.ErrorHandler { cause = e.Message, inner = e.InnerException != null ? e.InnerException.Message : "", errorCode = 400 }, System.Net.HttpStatusCode.BadRequest);
            }
            return false;
        }

        [WebGet(UriTemplate = "tag/cloud/")]
        public TagCloud GetTagCloud()
        {
            TagCloud ret = new TagCloud();
            try
            {
                using (DatabaseContext.MyDBContext con = new DatabaseContext.MyDBContext())
                {
                    try
                    {
                        var tagsQuery = con.FetchSet<DatabaseContext.Tag>().Select(t => new { tag = t.Keyword, count = t.Pages.Count }).OrderByDescending(t => t.count).Take(40).OrderBy(t => t.tag);

                        var tags = tagsQuery.ToList();

                        foreach (var tag in tags)
                        {
                            ret.Add(tag.tag, tag.count);
                        }
                    }
                    catch (Exception) { }
                }
            }
            catch (Exception e)
            {
                throw new WebFaultException<ErrorHandler>(new ErrorHandler { cause = e.Message, inner = e.InnerException != null ? e.InnerException.Message : "", errorCode = 500 }, System.Net.HttpStatusCode.InternalServerError);
            }
            return ret;
        }
    }
}