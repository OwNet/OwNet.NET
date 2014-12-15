using WPFProxy.Proxy;

namespace WPFProxy.LocalResponders.Inject.User
{
    class UserResponder : InjectDomainResponder
    {
        protected override string ThisUrl { get { return "user/"; } }
        public UserResponder() : base() {
        }
         
        protected override void InitRoutes()
        {
            Routes.RegisterRoute("POST", "login", Login)
                .RegisterRoute("POST", "logout", Logout);
        }

       

        // POST: inject.ownet/user/login
        private static ResponseResult Login(string method, string relativeUrl, RequestParameters parameters)
        {
            return LocalResponders.My.User.UserResponder.Login(method, relativeUrl, parameters);
        }

        // POST: inject.ownet/user/logout
        private static ResponseResult Logout(string method, string relativeUrl, RequestParameters parameters)
        {
            return LocalResponders.My.User.UserResponder.Logout(method, relativeUrl, parameters);
        }


    
    }
}
