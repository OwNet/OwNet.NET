using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServiceEntities;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using System.Web;
using WPFProxy.Proxy;

namespace WPFProxy.LocalResponders.Inject.Recommendation
{
    class RecommendationResponder : InjectDomainResponder
    {
        protected override string ThisUrl { get { return "recommend/"; } }
        public RecommendationResponder()
            : base()
        {
        }
        protected override void InitRoutes()
        {
            Routes
                .RegisterRoute("GET", "read", Read)
                .RegisterRoute("POST", "create", Create)
                .RegisterRoute("GET", "groupAutocomplete/", Autocomplete )
                .RegisterRoute("GET", "not_displayed", GetCountOfNotDisplayedRecomms)
                .RegisterRoute("GET", "js_not_displayed", GetCountNotDisplayedJs);
        }

        // POST: inject.ownet/recommend/create
        private static ResponseResult Create(string method, string relativeUrl, RequestParameters parameters)
        {
            if (Controller.UseServer)
            {
                int type = ServiceCommunicator.SendExplicitRecommendation(parameters);
                    if(type == 1)
                        return SimpleOKResponse("{ \"status\" : \"OK\" }");
                    else if(type == 2)
                        return SimpleOKResponse("{ \"status\" : \"UPDATED\" }");
            }
            return SimpleOKResponse("{ \"status\" : \"FAILED\" }");
        }



        // POST: inject.ownet/recommend/groupAutocomplete
        private static ResponseResult Autocomplete(string method, string relativeUrl, RequestParameters parameters)
        {
            List<ServiceEntities.AutocompletedGroup> data = null;

            int x = 0;

            string json = "[ ";

            if (RequestParameters.IsNullOrEmpty(parameters))
            {
                // toto domysliet !!
                return SimpleOKResponse("{ \"status\" : \"FAILED\" }");
            }
            else
            {
                string term = parameters.GetValue("term");
                data = ServiceCommunicator.GetGroupAutocomplete(term);


                foreach (ServiceEntities.AutocompletedGroup ag in data)
                {
                    x++;
                    json += "{ \"id\": \"" + ag.GroupId.ToString() + "\", \"label\": \"" + ag.GroupName + "\", \"value\": \"" + ag.GroupName + "\" }";

                    if (x != data.Count)
                        json += ", ";

                }

            }
            json += " ]";

            

            return SimpleOKResponse(json);
        }


        // GET: inject.ownet/recommend/read
        private static ResponseResult Read(string method, string relativeUrl, RequestParameters parameters)
        {
            string rpage = parameters.GetValue("page");
            //UserRecommendedPage recommend = ServiceCommunicator.ReceiveExplicitRecommendation(rpage);
            string rjson;
            Regex rreg = new Regex(@"[\r\n\t\'""]", RegexOptions.Compiled);
            //if (recommend == null || recommend.Recommendation.IsSet == false)
            //{
                Tuple<string, string> titleAndDescription = HtmlResponder.GetWebsiteTitleAndDescription(System.Web.HttpUtility.UrlDecode(rpage));
                string title = titleAndDescription.Item1;
                title = rreg.Replace(title, " ");
                title = (title.Length > 70) ? title.Substring(0, 70) : title;
                string desc = titleAndDescription.Item2;
                desc = rreg.Replace(desc, " ");
                desc = (desc.Length > 300) ? desc.Substring(0, 300) : desc;
                rjson = "{ \"set\" : \"0\", \"title\" : \"" + title + "\", \"desc\" : \"" + desc + "\", \"user\" : \"\", \"group\" : \"\", \"edit\" : \"0\" }";

            //}
            //else
            //{
            //    bool edit = false;
            //    string user = recommend.User.Firstname + " " + recommend.User.Surname;
            //    if (Settings.UserName.Equals(recommend.User.Username))
            //    {
            //        user = "you";
            //        edit = true;
            //    }
            //    if (Settings.UserTeacher)
            //    {
            //        edit = true;
            //    }// TODO: ak je string so specialnymi znakmi \r\n a pod., tak je to zly json -> Encode/Decode
            //    rjson = "{ \"set\" : \"1\", \"title\" : \"" + rreg.Replace(recommend.Recommendation.Title, " ") + "\", \"user\" : \"" + user + "\", \"desc\" : \"" + rreg.Replace(recommend.Recommendation.Description, " ") + "\", \"group\" : \"" + recommend.Recommendation.Group.Name + "\", \"edit\" : \"" + ((edit) ? "1" : "0") + "\"}";
            //}
            return SimpleOKResponse(rjson);
        }

        private static ResponseResult GetCountOfNotDisplayedRecomms(string method, string relativeUrl, RequestParameters parameters)
        {
            if (Settings.IsLoggedIn())
            {
                int count = ServiceCommunicator.ReceiveCountNotDisplayedRecomms();
                string rjson = "{ \"count\" : \"" + count.ToString() + "\"}";
                return SimpleOKResponse(rjson);
            }
            return SimpleOKResponse();
        }


        // GET: inject.ownet/cache/links?page={page}
        private static ResponseResult GetCountNotDisplayedJs(string method, string relativeUrl, RequestParameters parameters)
        {
            int recs = ServiceCommunicator.ReceiveCountNotDisplayedRecomms();
           
            return SimpleOKResponse("owNetNEWRECOMMS = " + recs.ToString(), "js");
        }

        

    }
}
