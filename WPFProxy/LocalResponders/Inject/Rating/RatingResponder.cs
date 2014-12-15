using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;
using ServiceEntities;
using WPFProxy.Proxy;

namespace WPFProxy.LocalResponders.Inject.Rating
{
    class RatingResponder : InjectDomainResponder
    {
        protected override string ThisUrl { get { return "rating/"; } }

        public RatingResponder()
            : base()
        {
        }
        protected override void InitRoutes()
        {
            Routes
                .RegisterRoute("GET", "read", Read)
                .RegisterRoute("POST", "create", Create);
        }


       
        // POST: inject.ownet/rating/create
        private static ResponseResult Create(string method, string relativeUrl, RequestParameters parameters)
        {
            if (Controller.UseServer)
            {
                if (ServiceCommunicator.SendRating(parameters))
                    return SimpleOKResponse();
            }
            return SimpleOKResponse("{ \"status\" : \"FAILED\" }");
        }
        // GET: inject.ownet/rating/read?page={page}
        private static ResponseResult Read(string method, string relativeUrl, RequestParameters parameters)
        {
            double avgrating = 0.0;
            int rating = 0;
            bool ret = ServiceCommunicator.ReceiveRating(parameters.GetValue("page"), out rating, out avgrating);
            string json = "{ \"rating\" : \"" + rating + "\", \"avgrating\" : \"" + Convert.ToString(avgrating).Replace(',', '.') + "\"}";
            return SimpleOKResponse(json);
        }

        
    }
}
