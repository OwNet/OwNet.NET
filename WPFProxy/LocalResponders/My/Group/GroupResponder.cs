using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Helpers;
using System.Web;
using HtmlAgilityPack;
using ServiceEntities;
using WPFProxy.LocalHelpers;
using WPFProxy.Proxy;
using System.Text.RegularExpressions;

namespace WPFProxy.LocalResponders.My.Group
{
    class GroupResponder : MyDomainResponder
    {
        protected override string ThisUrl { get { return "group/"; } }
        public GroupResponder()
            : base()
        {
        }

        protected override void InitRoutes()
        {
            // zadefinuj si vlastne cesty
            Routes.RegisterRoute("GET", "", Index);
            Routes.RegisterRoute("GET", "index", Index);
            Routes.RegisterRoute("POST", "create", Create);
            Routes.RegisterRoute("GET", "create", CreateForm);
            Routes.RegisterRoute("GET", "list/.*", List);
            Routes.RegisterRoute("GET", "join", Join);
            Routes.RegisterRoute("GET", "leave", Leave);
            Routes.RegisterRoute("GET", "show/.*", ShowGroup);
            Routes.RegisterRoute("GET", "delete/.*", DeleteGroup);
            Routes.RegisterRoute("GET", "recommended/.*", GroupRec);
            Routes.RegisterRoute("GET", "recommended/files/", Files);
            Routes.RegisterRoute("GET", "autocomplete/.*", Autocomplete);
            Routes.RegisterRoute("GET", "lastused/", GetLastUsed);
            Routes.RegisterRoute("GET", "validate_name/.*", ValidateName);
            Routes.RegisterRoute("GET", "validate_tags/.*", ValidateTags);
        }

        // GET: my.ownet/group/index.html
        private static ResponseResult Index(string method, string relativeUrl, RequestParameters parameters)
        {
            return new ResponseResult() { Data = LocalHelpers.MenuHelper.ReplyToPageWithMenu("groups.html") };
        }

        // GET: my.ownet/group/list/.*
        private static ResponseResult List(string method, string relativeUrl, RequestParameters parameters)
        {

            HtmlAgilityPack.HtmlDocument htmlDoc = new HtmlAgilityPack.HtmlDocument();
            if (LocalHelpers.LoginHelper.IncludeLoginRequiredMessage(htmlDoc) == false)
            {
                htmlDoc.LoadHtml("<div id=\"include\"></div>");

                HtmlNode root = htmlDoc.DocumentNode.SelectSingleNode("//div[@id=\"include\"]");
                if (root != null)
                {
                    HtmlNode resultsNode = htmlDoc.CreateElement("table");
                    resultsNode.SetAttributeValue("class", "table_links");
                    root.AppendChild(resultsNode);

                     IOrderedEnumerable<ServiceEntities.Group> groups;
                     TagBuilder tag = new TagBuilder();

                    if (parameters.GetValue("all") == "1")
                    {
                        groups = ServiceCommunicator.GetUserGroupsListAll().OrderBy(g => g.Name);
                        tag = new TagBuilder();
                        tag = tag.StartTag("h2")
                                .AddContent("All groups")
                            .EndTag();
                        resultsNode.InnerHtml += tag.ToString();
                    }
                    else
                    {
                        groups = ServiceCommunicator.GetUserGroupsList().OrderBy(g => g.Name);
                        tag = new TagBuilder();
                        tag = tag.StartTag("h2")
                                .AddContent("My groups")
                            .EndTag();
                        resultsNode.InnerHtml += tag.ToString();

                    }
              
                    foreach (ServiceEntities.Group group in groups)
                    {
                        tag = new TagBuilder();
                        tag = tag.StartTag("tr")
                           .AddAttribute("onmouseover", "Color_tr(this, '#fef2dc');")
                           .AddAttribute("onmouseout", "Color_tr_off(this);")

                           .StartTag("td")

                               .AddClass("table_group_name center")
                               .StartTag("a")
                               .AddClass("weblink")

                               .AddAttribute("href", "group/show/?group=" + Convert.ToString(group.Id))
                                  .StartTag("strong")
                                     .AddEncContent(group.Name)
                                  .EndTag()
                                .EndTag()//a
                             .EndTag()//td

                             .StartTag("td")
                               .AddClass("table_group_description")
                                  .StartTag("small")
                                     .AddEncContent(group.Description)
                                  .EndTag()
                               .EndTag()//td

                            .StartTag("td")
                                .AddClass("table_group_details center")

                               .StartTag("img")
                                    .AddAttribute("src", "graphics/group_info.png")
                                    .AddClass("img-8")
                               .EndTag()


                               .StartTag("a")
                               .AddClass("weblink")

                               .AddAttribute("href", "group/show/?group=" + Convert.ToString(group.Id))
                                     .AddContent("view details")
                                .EndTag()//a
                             .EndTag()//td

                           .EndTag();//tr


                        resultsNode.InnerHtml += tag.ToString();

                    }

                    
                }
            }
            return new ResponseResult() { Data = Encoding.GetEncoding("utf-8").GetBytes(htmlDoc.DocumentNode.OuterHtml) };
        }

        // POST: my.ownet/group/create
        private static ResponseResult Create(string method, string relativeUrl, RequestParameters parameters)
        {

            string name = HttpUtility.UrlDecode(parameters.GetValue("name"));
            string tags = HttpUtility.UrlDecode(parameters.GetValue("tags"));
            string description = HttpUtility.UrlDecode(parameters.GetValue("description"));

            string status = "";
            if (!ValidateNameStatus(name, out status))
                return Index("GET", "", new RequestParameters(""));

            /* parameters.GetValue("name") hodnota formu s menom name */
            ServiceCommunicator.CreateGroup(name, description,
                TagHelper.GetTags(tags), parameters.GetValue("location"));
            return Index("GET", "", new RequestParameters(""));
        }

        // GET: my.ownet/group/create
        private static ResponseResult CreateForm(string method, string relativeUrl, RequestParameters parameters)
        {
            if (!Settings.IsLoggedIn())
            {
                return ReplyToRedirect("index");
            }
            return new ResponseResult() { Data = LocalHelpers.MenuHelper.ReplyToPageWithMenu("groupCreate.html") };
        }


        // POST: my.ownet/group/join
        private static ResponseResult Join(string method, string relativeUrl, RequestParameters parameters)
        {

            ServiceEntities.IdUG idUG = new ServiceEntities.IdUG();

            idUG.UserId = Settings.UserID;
            idUG.GroupId = Convert.ToInt32(parameters.GetValue("id"));


            if (!ServiceCommunicator.isInGroup(Settings.UserID, idUG.GroupId))
                ServiceCommunicator.JoinGroup(idUG);
            return Index("GET", "", new RequestParameters(""));
        }


        // POST: my.ownet/group/leave
        private static ResponseResult Leave(string method, string relativeUrl, RequestParameters parameters)
        {

            ServiceEntities.IdUG idUG = new ServiceEntities.IdUG();

            idUG.UserId = Settings.UserID;
            idUG.GroupId = Convert.ToInt32(parameters.GetValue("id"));


            if(idUG.GroupId != 1 )
            ServiceCommunicator.LeaveGroup(idUG);

            return Index("GET", "", new RequestParameters(""));
        }


        // GET: my.ownet/group/show/.*
        private static ResponseResult ShowGroup(string method, string relativeUrl, RequestParameters parameters)
        {
            byte[] data = null;
            
            data = Show(Convert.ToInt32(parameters.GetValue("group")));
            
            return new ResponseResult() { Data = data };
        }


        internal static byte[] Show(int group_id)
        {
            
            HtmlDocument htmlDoc = new HtmlAgilityPack.HtmlDocument();
            // filePath is a path to a file containing the html
            htmlDoc.Load(Controller.GetAppResourcePath("Html/groupShow.html"), Encoding.UTF8);
            LocalHelpers.MenuHelper.IncludeLocalMenu(htmlDoc);
            if (htmlDoc.DocumentNode == null)
                throw new Exception("No document node.");

            ServiceEntities.Group group = ServiceCommunicator.GetGroup(group_id);
            if (group != null)
            {

                HtmlNode tabNode = htmlDoc.DocumentNode.SelectSingleNode("//div[@id=\"tab_load\"]");
                TagBuilder tagB = new TagBuilder();

                /* pridat ajax prvy load*/
                tagB = tagB.StartTag("script")
                        .AddAttribute("type","text/javascript")
                        .AddContent("$(document).ready(function () { TabLoad('all', 'group/recommended/?group=" + group.Id.ToString() + "&page=1'); });")
                      .EndTag();
                
                tabNode.InnerHtml = tagB.ToString();

                /* pridat ajaxove linky */
                tabNode = htmlDoc.DocumentNode.SelectSingleNode("//ul[@id=\"tabs\"]");

                tabNode.InnerHtml = tagB.ToString();
                string groupName = HttpUtility.HtmlEncode(group.Name);

                HtmlNode group_nameNode = htmlDoc.DocumentNode.SelectSingleNode("//h1[@id=\"group_name\"]");
                group_nameNode.InnerHtml = String.Format("{0}{1}", "<img class=\"img-7\" alt=\"\" src=\"graphics/group.png\"/> ", groupName);

                HtmlNode groupTableNode = htmlDoc.DocumentNode.SelectSingleNode("//table[@class=\"table_login\"]");

                TagBuilder tagT = new TagBuilder();

                string tags = "";
                string type = "";

                if (group.Location)
                    type = "Global";
                else
                    type = "Local";

                foreach (var tag in group.Tags)
                    tags += tag + ", ";

                if (tags != "" && tags[tags.Length - 2] == ',') 
                    tags.Remove(tags.Length-2);

                tagT = tagT.StartTag("tr")
                        .StartTag("td")
                            .AddClass("table_header_reg")
                            .StartTag("strong")
                                .AddContent("Name")
                            .EndTag()	
                        .EndTag()
                        .StartTag("td")
                            .AddClass("table_content")
                                .AddEncContent(group.Name)
                        .EndTag()
                    .EndTag()//tr


                    .StartTag("tr")
                        .StartTag("td")
                            .AddClass("table_header_reg")
                            .StartTag("strong")
                                .AddContent("Description")
                            .EndTag()	
                        .EndTag()
                        .StartTag("td")
                            .AddClass("table_content")

                                .AddEncContent(group.Description)
                            
                        .EndTag()

                    .EndTag()//tr


                    .StartTag("tr")
                        .StartTag("td")
                            .AddClass("table_header_reg")
                            .StartTag("strong")
                                .AddContent("Tags")
                            .EndTag()
                        .EndTag()
                        .StartTag("td")
                            .AddClass("table_content")
                                .AddEncContent(tags)
                        .EndTag()

                    .EndTag()//tr


                        .StartTag("tr")
                            .StartTag("td")
                                .AddClass("table_header_reg")
                                .StartTag("strong")
                                    .AddContent("Type")
                                .EndTag()
                            .EndTag()
                            .StartTag("td")
                                .AddClass("table_content")
                                    .AddEncContent(type)
                            .EndTag()

                        .EndTag();//tr

                 groupTableNode.InnerHtml = tagT.ToString();

                /* odkazy vymazanie pridanie opustenie skupiny */
                 
                 HtmlNode groupOpNode = htmlDoc.DocumentNode.SelectSingleNode("//div[@id=\"operations\"]");

                 //ServiceCommunicator.isAdmin(Settings.UserID);
                 if (ServiceCommunicator.isInGroup(Settings.UserID, group.Id) && group.Id != 1)
                 {
                     tagT = new TagBuilder();
                     tagT = tagT.StartTag("a")
                             .AddAttribute("href", "group/leave?id=" + group.Id.ToString())
                             .AddClass("button_go")
                             .AddAttribute("style", "color: #2E2E2E;")

                             .StartTag("strong")
                                 .AddContent("Leave group")
                              .EndTag()



                         .EndTag();//a
                     groupOpNode.InnerHtml += tagT.ToString();
                 }
                     /* inak Join group */
                 else if (!ServiceCommunicator.isInGroup(Settings.UserID, group.Id) )
                 {
                     tagT = new TagBuilder();
                     tagT = tagT.StartTag("a")
                             .AddAttribute("href", "group/join?id=" + group.Id.ToString())
                             .AddClass("button_go")
                             .AddAttribute("style", "color: #2E2E2E;")

                             .StartTag("strong")
                                 .AddContent("Join group")
                              .EndTag()



                         .EndTag();//a
                     groupOpNode.InnerHtml += tagT.ToString();
                 }

                if ((ServiceCommunicator.isAdmin(Settings.UserID, group.Id) || Settings.IsTeacher()) && group.Id != 1 && group.Id != 2 && group.Id != 3 && group.Id != 4 && group.Id != 5)
                {
                    tagT = new TagBuilder();
                    tagT = tagT.StartTag("a")
                            .AddAttribute("href", "group/delete/?group="+ group.Id.ToString())
                            .AddClass("button_go")
                            .AddAttribute("style", "color: #2E2E2E;")

                            .StartTag("strong")
                                .AddContent("Delete group")
                             .EndTag()

                        .EndTag();//a


                    groupOpNode.InnerHtml += tagT.ToString();
                }

                /* pridavanie userov */
                List<ServiceEntities.User> users = ServiceCommunicator.groupUsers(group.Id);

                HtmlNode groupUsersNode = htmlDoc.DocumentNode.SelectSingleNode("//table[@class=\"table_links\"]");
                tagT = new TagBuilder();
                    
                if(users.Any()){
                    foreach(var user in users){
                        string role = "";
                    
                        if (user.IsTeacher)
                            role = "Teacher";
                        else
                            role = "Student";

                        string img;
                        if (user.IsMale)
                            img = "user_male";
                        else
                            img = "user_female";

                        tagT = tagT.StartTag("tr")
                                .AddAttribute("onmouseover", "Color_tr(this, '#fef2dc');")
                                .AddAttribute("onmouseout", "Color_tr_off(this);")
                                .AddAttribute("style:","font-weight: normal")

                                .StartTag("td")
                                    .AddClass(img)
                                .EndTag()

                                .StartTag("td")
                                        .AddClass("user_username")
                                        .AddEncContent(user.Username)
                                .EndTag()

                                .StartTag("td")
                                        .AddClass("user_name")
                                        .AddEncContent(user.Firstname + " " + user.Surname)
                                .EndTag()

                                .StartTag("td")
                                        .AddClass("user_email")
                                        .AddContent(user.Email)
                                .EndTag()

                                .StartTag("td")
                                        .AddClass("user_role")
                                        .AddContent(role)
                                .EndTag()

                            .EndTag();//tr

                        groupUsersNode.InnerHtml += tagT.ToString();
                    }
                }
            }

            return Encoding.GetEncoding("utf-8").GetBytes(htmlDoc.DocumentNode.OuterHtml);
        }

        // POST: my.ownet/group/delete/.*
        private static ResponseResult DeleteGroup(string method, string relativeUrl, RequestParameters parameters)
        {
            int group_id = Convert.ToInt32(parameters.GetValue("group"));
            
            if((ServiceCommunicator.isAdmin(Settings.UserID, group_id) || Settings.IsTeacher()) && group_id != 1 && group_id != 2 && group_id != 3 && group_id != 4 && group_id != 5)
            ServiceCommunicator.DeleteGroup(group_id);
            
            return Index("GET", "", new RequestParameters(""));
        }

        // POST: my.ownet/group/recommended/.*
        private static ResponseResult GroupRec(string method, string relativeUrl, RequestParameters parameters)
        {

            byte[] data = null;
            if (RequestParameters.IsNullOrEmpty(parameters))
            {
                return LoadProxyLocalError404();
            }
            else
            {
                string s = parameters.GetValue("page");
                if (s == "")
                    s = "1";
                data = ReplyToRecommended(parameters.GetValue("group"), s);
            }
            return new ResponseResult() { Data = data };
            
        }

        private static byte[] ReplyToRecommended(string group, string page = "1")
        {
            HtmlAgilityPack.HtmlDocument htmlDoc = new HtmlAgilityPack.HtmlDocument();
            if (LocalHelpers.LoginHelper.IncludeLoginRequiredMessage(htmlDoc) == false)
            {
                htmlDoc.LoadHtml("<div id=\"include\"></div>");

                int pag = Convert.ToInt32(page);
                UserRecommendations recs;
                
               recs = ServiceCommunicator.ReceiveGroupRecommendations(group, page);
               

                IncludeRecommendedPages(htmlDoc, recs);
                IncludePaging(htmlDoc, recs.TotalPages, recs.CurrentPage, "all", "group/recommend/", string.Format("group={0}", group));
            }
            return Encoding.GetEncoding("utf-8").GetBytes(htmlDoc.DocumentNode.OuterHtml);
        }
        private static void IncludeRecommendedPages(HtmlDocument htmlDoc, UserRecommendations recs)
        {
            if (recs == null || !recs.Recommendations.Any())
            {
                IncludeNoResultsMessage(htmlDoc, "No page has been recommended yet.");
                return;
            }
            HtmlNode root = htmlDoc.DocumentNode.SelectSingleNode("//div[@id=\"include\"]");
            if (root == null) return;
            HtmlNode resultsNode = htmlDoc.CreateElement("table");
            resultsNode.SetAttributeValue("class", "table_links");
            root.AppendChild(resultsNode);
            HtmlNode linkNode;

            string tabletd = "<td class=\"table_star\">"
                    + "	<ul class=\"star-rating tabsmargin\" id=\"star-rating-1\">"
                        + "	<li class=\"current-rating\" id=\"current-rating-1\" style=\"width: {0}%;\"></li>"
                        + "</ul>"
                    + "</td>"
                    + "<td class=\"table_link\">"
                        + "<a href=\"{1}\" target=\"_blank\" class=\"weblink{9}\" onclick=\"return useRecommendation({10});\">{2}</a><br />"
                        + "<small>{3}<small>"
                    + "</td>"
                    + "<td class=\"table_date\"><span class=\"activity_time\">{4} {5}</span><br />"
                    + "<small{7}>{6}</small></td>"
                    + ((Settings.IsTeacher()) ? "<td class=\"\" style=\"width: 20px;\"><a href=\"javascript:void(0);\" onclick=\"var that = this; confirm('Do you want to remove recommendation <strong>{2}</strong>?', function () {{ deleteRecommendation({10}, {11}, that); }});\"><img src=\"graphics/delete.png\" class=\"img-4\" alt=\"Remove recommendation\" title=\"Remove recommendation\"/></a></td>" : "");

            foreach (UserRecommendedPageWithRating rec in recs.Recommendations)
            {
                linkNode = htmlDoc.CreateElement("tr");

                string title = HttpUtility.HtmlEncode(rec.Recommendation.Title);
                string description = HttpUtility.HtmlEncode(rec.Recommendation.Description);
                linkNode.InnerHtml = String.Format(tabletd,
                                Convert.ToInt32(rec.Page.AvgRating * 20),
                                rec.Page.AbsoluteURI,
                                (title.Length > 70 ? title.Substring(0, 65) + "&hellip;" : title),
                               (description.Length > 300 ? description.Substring(0, 295) + "&hellip;" : description),
                    //rec.User != null ? 
                                (rec.RecommendationTimeStamp.Date.Equals(DateTime.Today) ? "today" : ""),
                                rec.RecommendationTimeStamp.ToString("dd.MM.yyyy HH:mm"),// : "",
                    //rec.User != null ? 
                                "Recommended by " + HttpUtility.HtmlEncode(rec.User.Firstname) + " " + HttpUtility.HtmlEncode(rec.User.Surname) + ".",
                                rec.User.IsTeacher ? "class=\"teacher\"" : "",
                                HttpUtility.UrlEncode(rec.Page.AbsoluteURI),
                                (rec.Visited ? " visited" : ""),
                                rec.Page.Id,
                                rec.Recommendation.Group.Id);// : "Not yet recommeded.");

                resultsNode.AppendChild(linkNode);
            }


        }


        /* autocomplete */
        private static ResponseResult  Autocomplete(string method, string relativeUrl, RequestParameters parameters)
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
                    json += "{ \"id\": \"" + Convert.ToString(ag.GroupId) + "\", \"label\": \"" + HttpUtility.HtmlEncode(ag.GroupName) + "\", \"value\": \"" + Convert.ToString(ag.GroupId) + "\" }";

                    if (x != data.Count)
                        json += ", ";

                }

            }
            json += " ]";
            return SimpleOKResponse(json);
        }

        /* files zatial nic */
        private static ResponseResult Files(string method, string relativeUrl, RequestParameters parameters)
        {
            byte[] data = null;
            if (RequestParameters.IsNullOrEmpty(parameters))
            {
                data = ShowFiles();
            }
            else
            {
                data = ShowFiles(Convert.ToInt32(parameters.GetValue("id")));
            }
            return new ResponseResult() { Data = data };
        }

        internal static byte[] ShowFiles(int folderId = 0)
        {
            HtmlDocument htmlDoc = new HtmlAgilityPack.HtmlDocument();
            string baseUrl = "files/";
            // filePath is a path to a file containing the html
            htmlDoc.Load(Controller.GetAppResourcePath("Html/files.html"), Encoding.UTF8);
            LocalHelpers.MenuHelper.IncludeLocalMenu(htmlDoc);
            if (htmlDoc.DocumentNode == null)
                throw new Exception("No document node.");

            SharedFolder folder = ServiceCommunicator.GetSharedFolder(folderId);
            if (folder != null)
            {
                HtmlNode foldersTableNode = htmlDoc.DocumentNode.SelectSingleNode("//table[@id=\"table_folders\"]");

                HtmlNode foldersTableRowNode = htmlDoc.CreateElement("tr");

                foldersTableRowNode.InnerHtml = String.Format(
                    "<td class=\"files_folder\"><img src=\"graphics/files/up.png\" class=\"img-4\" alt=\"\"/>" +
                        "<a href=\"{0}\" class=\"weblink\">up</a>" +
                    "</td>",
                    GetSharedFolderPath(baseUrl, folder.ParentFolderId));

                foldersTableNode.ChildNodes.Add(foldersTableRowNode);

                if (folder.ChildFolders != null)
                {
                    foreach (SharedFolder sharedFolder in folder.ChildFolders)
                    {
                        foldersTableRowNode = htmlDoc.CreateElement("tr");

                        foldersTableRowNode.InnerHtml = String.Format(
                            "<td class=\"files_folder\"><img src=\"graphics/files/folder.png\" class=\"img-4\" alt=\"\"/>" +
                                "<a href=\"{0}\" class=\"weblink\">{1}</a>" +
                            "</td>",
                            GetSharedFolderPath(baseUrl, sharedFolder),
                            sharedFolder.Name);

                        foldersTableNode.ChildNodes.Add(foldersTableRowNode);
                    }
                }

                HtmlNode filesTableNode = htmlDoc.DocumentNode.SelectSingleNode("//table[@id=\"table_files\"]");

                if (folder.Files != null && folder.Files.Count > 0)
                {
                    foreach (SharedFile sharedFile in folder.Files)
                    {
                        HtmlNode filesTableRowNode = htmlDoc.CreateElement("tr");
                        filesTableRowNode.SetAttributeValue("class", sharedFile.IsTeacher ? "from_teacher" : "from_student");

                        filesTableRowNode.InnerHtml = String.Format(
                            "<td class=\"files_file\">" +
                                "<img src=\"{0}\" class=\"img-4 left\" alt=\"\"/>" +
                                "<a href=\"{1}\" target=\"_blank\"  onclick=\"return useShared(this);\" class=\"weblink\">{2}</a>" +
                                "<div class=\"files_desc\">{3}</div>" +
                            "</td>" +
                            "<td class=\"files_role\">{4}</td>" +
                            "<td class=\"table_date\"><span class=\"activity_time\">{5} {6}</span></td>",
                            AppHelpers.GetFileIconRelativePath(sharedFile.FileName),
                            HttpLocalResponder.GetSharedFilePath(sharedFile),
                            HttpUtility.HtmlEncode(sharedFile.Title),
                            LocalHelpers.ActivitiesHelper.NlToBr(HttpUtility.HtmlEncode(sharedFile.Description)),
                            HttpUtility.HtmlEncode(sharedFile.Username),
                            (sharedFile.Created.Equals(DateTime.Today) ? "today" : ""),
                            sharedFile.Created.ToString("dd.MM.yyyy HH:mm")
                                );

                        filesTableNode.ChildNodes.Add(filesTableRowNode);
                    }
                }
                else
                {
                    filesTableNode = htmlDoc.DocumentNode.SelectSingleNode("//div[@id=\"include_files\"]");
                    filesTableNode.InnerHtml = "<div class=\"message_status\"><div class=\"message_info\">There are no files in this folder.</div></div>";
                }
                HtmlNode node = htmlDoc.DocumentNode.SelectSingleNode("//span[@id=\"folder_name\"]");
                if (node != null)
                    node.InnerHtml = HttpUtility.HtmlEncode(folder.Name);

                node = htmlDoc.DocumentNode.SelectSingleNode("//span[@id=\"folder_full_path\"]");
                if (node != null)
                    node.InnerHtml = String.Format("<a href=\"{0}\" class=\"files_act_folder\">{1}</a>",
                        GetSharedFolderPath(baseUrl, folder.Id),
                        folder.FullPath);
            }

            return Encoding.GetEncoding("utf-8").GetBytes(htmlDoc.DocumentNode.OuterHtml);
        }
        public static string GetSharedFolderPath(string baseUrl, ServiceEntities.SharedFolder folder)
        {
            return GetSharedFolderPath(baseUrl, folder.Id);
        }

        public static string GetSharedFolderPath(string baseUrl, int id)
        {
            return String.Format(String.Format(baseUrl + "index.html?id={0}", id));
        }

        // lastused/
        private static ResponseResult GetLastUsed(string method, string relativeUrl, RequestParameters parameters)
        {
            ServiceEntities.GroupsList groups = null;

            groups = ServiceCommunicator.GetLastUsedGroups();

            return new ResponseResult();
        }

        // GET group/validate_name/.*
        private static ResponseResult ValidateName(string method, string relativeUrl, RequestParameters parameters)
        {
            string name = parameters.GetValue("name");
            name = HttpUtility.UrlDecode(name);
            string status = "";
            ValidateNameStatus(name, out status);
            return SimpleOKResponse(status);
        }

        private static bool ValidateNameStatus(string name, out string status)
        {
            var regex = new Regex(@"^[\w\s,\.\-_]*$");
            if (name.Length < 3)
                status = "{ \"status\" : \"FAIL\" }";
            else if (ServiceCommunicator.ValidateName(name))
                status = "{ \"status\" : \"NAME\" }";
            else if (regex.IsMatch(name))
            {
                status = "{ \"status\" : \"OK\" }";
                return true;
            }
            else
                status = "{ \"status\" : \"CHARS\" }";
            return false;
        }

        // GET group/validate_tags/.*
        private static ResponseResult ValidateTags(string method, string relativeUrl, RequestParameters parameters)
        {
            string tags = parameters.GetValue("tags");
            tags = HttpUtility.UrlDecode(tags);
            var regex = new Regex(@"^[\w\s,\.\-_]*$");
            if (regex.IsMatch(tags))
                return SimpleOKResponse("{ \"status\" : \"OK\" }");
            else
                return SimpleOKResponse("{ \"status\" : \"CHARS\" }");
        }
    }
}
