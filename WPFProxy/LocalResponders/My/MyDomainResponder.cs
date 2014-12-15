using WPFProxy.Proxy;

namespace WPFProxy.LocalResponders.My
{
    class MyDomainResponder : LocalResponder
    {
        protected override string ThisUrl  { get { return "my.ownet/"; } }


        public MyDomainResponder() : base()
        {
        }

        protected override void InitRoutes()
        {
            Routes.RegisterRoute(null, "history/.*", new History.HistoryResponder().Respond)
                .RegisterRoute(null, "rating/.*", new Rating.RatingResponder().Respond)
                .RegisterRoute(null, "recommend/.*", new Recommendation.RecommendationResponder().Respond)
                .RegisterRoute(null, "prefetch/.*", new Prefetch.PrefetchResponder().Respond)
                .RegisterRoute(null, "search/.*", new Search.SearchResponder().Respond)
                .RegisterRoute(null, "tag/.*", new Tag.TagResponder().Respond)
                .RegisterRoute(null, "user/.*", new User.UserResponder().Respond)
                .RegisterRoute(null, "live/.*", new Live.LiveResponder().Respond)
                .RegisterRoute(null, "files/.*", new Files.FilesResponder().Respond)
                .RegisterRoute(null, "activity/.*", new Activity.ActivityResponder().Respond)
                .RegisterRoute(null, "group/.*", new Group.GroupResponder().Respond)
                .RegisterRoute(null, "cache/.*", new Cache.CacheResponder().Respond)
                .RegisterRoute("GET", "", Index)
                .RegisterRoute("GET", "index.html", Index);
            InitContentRoutes();
        }

        private static ResponseResult Index(string method, string relativeUrl, RequestParameters parameters)
        {
            return new ResponseResult() { Filename = "index.html", Data = LocalHelpers.MenuHelper.ReplyToPageWithMenu("index.html") };
        }

       


       
    }
}
