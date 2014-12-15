using System;
using System.Linq;
using System.Text;
using System.Web;
using Helpers;
using HtmlAgilityPack;
using ServiceEntities;
using WPFProxy.Proxy;

namespace WPFProxy.LocalResponders.My.User
{
    class UserResponder : MyDomainResponder
    {
        protected override string ThisUrl { get { return "user/"; } }
        public UserResponder() : base() {
        }
         
        protected override void InitRoutes()
        {
            Routes.RegisterRoute("GET", "", Index)
                .RegisterRoute("GET", "index.html", Index) // ??
                .RegisterRoute("GET", "register.html", Register)
                .RegisterRoute("POST", "register.html", RegisterParams)
                .RegisterRoute("GET", "list/.*", List)
                .RegisterRoute("GET", "registered.html", Registered)
                .RegisterRoute("POST", "delete", Delete)
                .RegisterRoute("POST", "login", Login)
                .RegisterRoute("POST", "logout", Logout)
                .RegisterRoute("GET", "information/update", UserInformation)
                .RegisterRoute("POST", "information/update", UpdateUserInformation)
                .RegisterRoute("GET", "password/update", UserPassword)
                .RegisterRoute("POST", "password/update", UpdateUserPassword)
                .RegisterRoute("GET", "settings.html", UserSettings);
        }

        // GET: my.ownet/user/
        // GET: my.ownet/user/index.html
        private static ResponseResult Index(string method, string relativeUrl, RequestParameters parameters)
        {
            return new ResponseResult() { Data = LocalHelpers.MenuHelper.ReplyToPageWithMenu("users.html") };
        }

        // GET: my.ownet/user/list/{group}?page={page}
        private static ResponseResult List(string method, string relativeUrl, RequestParameters parameters)
        {
            byte[] bytes = null;
            if (RequestParameters.IsNullOrEmpty(parameters))
            {
                bytes = ReplyToUserlist(relativeUrl);
            }
            else
            {
                bytes = ReplyToUserlist(relativeUrl, parameters.GetValue("page"));
            }
            return new ResponseResult() { Data = bytes };
        }

        // GET: my.ownet/user/registered.html
        private static ResponseResult Registered(string method, string relativeUrl, RequestParameters parameters)
        {
            return new ResponseResult() { Data = ReplyToSuccessfulRegistration(parameters) };
        }

        // GET: my.ownet/user/register.html
        private static ResponseResult Register(string method, string relativeUrl, RequestParameters parameters)
        {
            byte[] bytes = null;
            if (Settings.IsLoggedIn())
            {
                return ReplyToRedirect("index.html");
            }
            else
            {
                bytes = LocalHelpers.MenuHelper.ReplyToPageWithMenu("register.html");
            }
            return new ResponseResult() { Data = bytes };
        }

        // POST: my.ownet/user/register.html
        private ResponseResult RegisterParams(string method, string relativeUrl, RequestParameters parameters)
        {
            RegistrationCheck registerret = ServiceCommunicator.RegisterUser(parameters);
            if (registerret.WasSuccessful)
            {
                return ReplyToRedirect("http://my.ownet/user/registered.html", "username=" + parameters.GetValue("username"));
            }
            else
            {
                return new ResponseResult() { Data = ReplyToFailedRegistration(parameters, registerret) };
            }
        }

        // POST: my.ownet/user/delete
        private static ResponseResult Delete(string method, string relativeUrl, RequestParameters parameters)
        {
            string did = parameters.GetValue("id");
            bool deluret = ServiceCommunicator.DeleteUser(did);
            if (deluret)
                return SimpleOKResponse();
            return SimpleOKResponse("{ \"status\" : \"FAILED\" }");
        }

        // GET: my.ownet/user/settings.html?userid={userid}
        private static ResponseResult UserSettings(string method, string relativeUrl, RequestParameters parameters)
        {
            string userid = null;
            if (RequestParameters.IsNullOrEmpty(parameters))
            {
                userid = null;
            }
            else
            {
                userid = parameters.GetValue("userid");
            }

            int id = 0;
            if (!Settings.IsLoggedIn())
            {
                return ReplyToRedirect("index.html");
            }
            if (String.IsNullOrWhiteSpace(userid)) id = Settings.UserID;
            else id = Convert.ToInt32(userid);

            if (id != Settings.UserID && Settings.IsTeacher() == false)
            {
                return ReplyToRedirect("index.html");
            }
            HtmlDocument htmlDoc = new HtmlAgilityPack.HtmlDocument();
            htmlDoc.Load(Controller.GetAppResourcePath("Html/settings.html"));
            LocalHelpers.MenuHelper.IncludeLocalMenu(htmlDoc);
            HtmlNode node = htmlDoc.DocumentNode.SelectSingleNode("//a[@id=\"auserinfo\"]");
            node.SetAttributeValue("onclick", String.Format("TabLoad('userinfo', 'user/information/update?userid={0}');", id));
            node = htmlDoc.DocumentNode.SelectSingleNode("//a[@id=\"auserpass\"]");
            node.SetAttributeValue("onclick", String.Format("TabLoad('userpass', 'user/password/update?userid={0}');", id));

            return new ResponseResult() { Data = Encoding.GetEncoding("utf-8").GetBytes(htmlDoc.DocumentNode.OuterHtml) };
        }

        // POST: my.ownet/user/login
        public static ResponseResult Login(string method, string relativeUrl, RequestParameters parameters)
        {
            string nick = parameters.GetValue("username");
            nick = HttpUtility.UrlDecode(nick);
            string pass = parameters.GetValue("password");
            pass = HttpUtility.UrlDecode(pass);

            bool loginret = WPFProxy.Settings.LogIn(nick, pass);
            if (loginret)
                return SimpleOKResponse();
            return SimpleOKResponse("{ \"status\" : \"FAILED\" }");
        }

        // POST: my.ownet/user/logout
        public static ResponseResult Logout(string method, string relativeUrl, RequestParameters parameters)
        {
            WPFProxy.Settings.UserHasLoggedOut();
            return SimpleOKResponse();
        }

        // GET: my.ownet/user/information/update
        private static ResponseResult UserInformation(string method, string relativeUrl, RequestParameters parameters)
        {
            return new ResponseResult() { Data = ReplyToUpdate(parameters) };
        }
        // POST: my.ownet/user/information/update
        private static ResponseResult UpdateUserInformation(string method, string relativeUrl, RequestParameters parameters)
        {
            byte[] bytes = null;
            UpdateCheck updateret = ServiceCommunicator.UpdateUser(parameters);
            if (updateret.WasSuccessful)
            {
                bytes = ReplyToUpdate(parameters, true);//??
            }
            else
            {
                bytes = ReplyToFailedUpdate(parameters, updateret);
            }
            return new ResponseResult() { Data = bytes };
        }
        
        
        // GET: my.ownet/user/password/update
        private static ResponseResult UserPassword(string method, string relativeUrl, RequestParameters parameters)
        {
            return new ResponseResult() { Data = ReplyToChangePassword(parameters) };
        }

        // POST: my.ownet/user/password/update
        private static ResponseResult UpdateUserPassword(string method, string relativeUrl, RequestParameters parameters)
        {
            byte[] bytes = null;
            ChangePasswordCheck passret = ServiceCommunicator.ChangeUserPassword(parameters);
            if (passret.WasSuccessful)
            {
                bytes = ReplyToChangePassword(parameters, true);
            }
            else
            {
                bytes = ReplyToFailedChangePassword(parameters, passret);
            }
            return new ResponseResult() { Data = bytes };
        }

        private static byte[] ReplyToFailedRegistration(RequestParameters parameters, RegistrationCheck status)
        {
            HtmlDocument htmlDoc = new HtmlAgilityPack.HtmlDocument();
            htmlDoc.Load(Controller.GetAppResourcePath("Html/register.html"));
            LocalHelpers.MenuHelper.IncludeLocalMenu(htmlDoc);
            HtmlNode node = htmlDoc.DocumentNode.SelectSingleNode("//div[@id=\"registerform\"]");
            HtmlNode infoNode = htmlDoc.CreateElement("div");
            infoNode.SetAttributeValue("class", "message_error");
            infoNode.InnerHtml = "Registration failed, correct highlighted fields.<br />";

            node.PrependChild(infoNode);
            node = htmlDoc.DocumentNode.SelectSingleNode("//input[@id=\"username\"]");
            node.SetAttributeValue("value", HttpUtility.UrlDecode(parameters.GetValue("username")));
            if (!status.CorrectUsername)
            {
                node.SetAttributeValue("class", "input_register invalid");
                infoNode.InnerHtml += "Username is not valid. ";
            }
            else if (status.IsCorrect && !status.UserRegistered)
            {
                node.SetAttributeValue("class", "input_register invalid");
                infoNode.InnerHtml += "Username is already registered. ";
            }
            node = htmlDoc.DocumentNode.SelectSingleNode("//input[@id=\"firstname\"]");
            node.SetAttributeValue("value", HttpUtility.UrlDecode(parameters.GetValue("firstname")));
            if (!status.CorrectFirstname)
            {
                node.SetAttributeValue("class", "input_register invalid");
                infoNode.InnerHtml += "Firstname is not valid. ";
            }
            node = htmlDoc.DocumentNode.SelectSingleNode("//input[@id=\"surname\"]");
            node.SetAttributeValue("value", HttpUtility.UrlDecode(parameters.GetValue("surname")));
            if (!status.CorrectSurname)
            {
                node.SetAttributeValue("class", "input_register invalid");
                infoNode.InnerHtml += "Surname is not valid. ";
            }
            string gender = parameters.GetValue("gender").ToLower();
            if (gender.Equals("male"))
            {
                node = htmlDoc.DocumentNode.SelectSingleNode("//input[@id=\"gendermale\"]");
                node.SetAttributeValue("checked", "checked");
            }
            else if (gender.Equals("female"))
            {
                node = htmlDoc.DocumentNode.SelectSingleNode("//input[@id=\"genderfemale\"]");
                node.SetAttributeValue("checked", "checked");
            }
            if (!status.CorrectGender)
            {
                node = htmlDoc.DocumentNode.SelectSingleNode("//span[@id=\"genderselect\"]");
                node.SetAttributeValue("class", "invalid");
                infoNode.InnerHtml += "Choose gender. ";
            }
            node = htmlDoc.DocumentNode.SelectSingleNode("//input[@id=\"email\"]");
            node.SetAttributeValue("value", HttpUtility.UrlDecode(parameters.GetValue("email")));
            if (!status.CorrectEmail)
            {
                node.SetAttributeValue("class", "input_register invalid");
                infoNode.InnerHtml += "E-mail is not valid. ";
            }


            if (!status.CorrectPassword || !status.PasswordMatch)
            {
                node = htmlDoc.DocumentNode.SelectSingleNode("//input[@id=\"password\"]");
                node.SetAttributeValue("class", "input_register invalid");
                node = htmlDoc.DocumentNode.SelectSingleNode("//input[@id=\"password2\"]");
                node.SetAttributeValue("class", "input_register invalid");
                if (!status.CorrectPassword)
                {
                    infoNode.InnerHtml += "Password is not valid. ";
                }
                if (!status.PasswordMatch)
                {
                    infoNode.InnerHtml += "Passwords do not match. ";
                }
            }

            return Encoding.GetEncoding("utf-8").GetBytes(htmlDoc.DocumentNode.OuterHtml);
        }
        
        private static byte[] ReplyToUpdate(RequestParameters parameters, bool showSuccess = false)
        {
            HtmlDocument htmlDoc = new HtmlAgilityPack.HtmlDocument();
            htmlDoc.Load(Controller.GetAppResourcePath("Html/_updateinfo.html"));
            HtmlNode node;
            string id = parameters.GetValue("userid");
            if (Settings.IsLoggedIn() && !String.IsNullOrWhiteSpace(id) && (Convert.ToInt32(id) == Settings.UserID || Settings.UserTeacher))
            {

                ServiceEntities.User user = ServiceCommunicator.ReceiveUserInformation(id);
                if (user != null)
                {
                    if (showSuccess)
                    {
                        node = htmlDoc.DocumentNode.SelectSingleNode("//div[@id=\"updateformdiv\"]");
                        HtmlNode infoNode = htmlDoc.CreateElement("div");
                        infoNode.SetAttributeValue("class", "message_success");
                        infoNode.InnerHtml = "Information successfully updated.<br />";
                        node.PrependChild(infoNode);
                    }
                    node = htmlDoc.DocumentNode.SelectSingleNode("//input[@id=\"userid\"]");
                    node.SetAttributeValue("value", id);
                    node = htmlDoc.DocumentNode.SelectSingleNode("//strong[@id=\"usernamestr\"]");
                    node.InnerHtml = user.Username;
                    node = htmlDoc.DocumentNode.SelectSingleNode("//input[@id=\"username\"]");
                    node.SetAttributeValue("value", user.Username);
                    node = htmlDoc.DocumentNode.SelectSingleNode("//input[@id=\"firstname\"]");
                    node.SetAttributeValue("value", user.Firstname);
                    node = htmlDoc.DocumentNode.SelectSingleNode("//input[@id=\"surname\"]");
                    node.SetAttributeValue("value", user.Surname);
                    if (user.IsMale)
                    {
                        node = htmlDoc.DocumentNode.SelectSingleNode("//input[@id=\"gendermale\"]");
                        node.SetAttributeValue("checked", "checked");
                    }
                    else
                    {
                        node = htmlDoc.DocumentNode.SelectSingleNode("//input[@id=\"genderfemale\"]");
                        node.SetAttributeValue("checked", "checked");
                    }
                    node = htmlDoc.DocumentNode.SelectSingleNode("//input[@id=\"email\"]");
                    node.SetAttributeValue("value", user.Email);
                    if (Settings.UserTeacher && Settings.UserID != user.Id)
                    {
                        if (user.IsTeacher)
                        {
                            node = htmlDoc.DocumentNode.SelectSingleNode("//input[@id=\"roleteacher\"]");
                            node.SetAttributeValue("checked", "checked");
                        }
                        else
                        {
                            node = htmlDoc.DocumentNode.SelectSingleNode("//input[@id=\"rolestudent\"]");
                            node.SetAttributeValue("checked", "checked");
                        }
                    }
                    else
                    {
                        node = htmlDoc.DocumentNode.SelectSingleNode("//table[@id=\"updateformtable\"]");
                        node.RemoveChild(htmlDoc.DocumentNode.SelectSingleNode("//tr[@id=\"roleselect\"]"));
                    }
                }
            }
            return Encoding.GetEncoding("utf-8").GetBytes(htmlDoc.DocumentNode.OuterHtml);

        }
        private static byte[] ReplyToFailedUpdate(RequestParameters parameters, UpdateCheck status)
        {
            HtmlDocument htmlDoc = new HtmlAgilityPack.HtmlDocument();
            htmlDoc.Load(Controller.GetAppResourcePath("Html/_updateinfo.html"));
            string id = parameters.GetValue("userid");
            if (Settings.IsLoggedIn() && !String.IsNullOrWhiteSpace(id) && (Convert.ToInt32(id) == Settings.UserID || Settings.UserTeacher))
            {

                HtmlNode node = htmlDoc.DocumentNode.SelectSingleNode("//div[@id=\"updateformdiv\"]");
                HtmlNode infoNode = htmlDoc.CreateElement("div");
                infoNode.SetAttributeValue("class", "message_error");
                infoNode.InnerHtml = "Update failed, correct highlighted fields.<br />";

                node.PrependChild(infoNode);

                node = htmlDoc.DocumentNode.SelectSingleNode("//input[@id=\"username\"]");
                node.SetAttributeValue("value", HttpUtility.UrlDecode(parameters.GetValue("username")));
                node = htmlDoc.DocumentNode.SelectSingleNode("//strong[@id=\"usernamestr\"]");
                node.InnerHtml = HttpUtility.UrlDecode(parameters.GetValue("username"));

                node = htmlDoc.DocumentNode.SelectSingleNode("//input[@id=\"firstname\"]");
                node.SetAttributeValue("value", HttpUtility.UrlDecode(parameters.GetValue("firstname")));
                if (!status.CorrectFirstname)
                {
                    node.SetAttributeValue("class", "input_register invalid");
                    infoNode.InnerHtml += "Firstname is not valid. ";
                }
                node = htmlDoc.DocumentNode.SelectSingleNode("//input[@id=\"surname\"]");
                node.SetAttributeValue("value", HttpUtility.UrlDecode(parameters.GetValue("surname")));
                if (!status.CorrectSurname)
                {
                    node.SetAttributeValue("class", "input_register invalid");
                    infoNode.InnerHtml += "Surname is not valid. ";
                }
                string gender = parameters.GetValue("gender").ToLower();
                if (gender.Equals("male"))
                {
                    node = htmlDoc.DocumentNode.SelectSingleNode("//input[@id=\"gendermale\"]");
                    node.SetAttributeValue("checked", "checked");
                }
                else if (gender.Equals("female"))
                {
                    node = htmlDoc.DocumentNode.SelectSingleNode("//input[@id=\"genderfemale\"]");
                    node.SetAttributeValue("checked", "checked");
                }
                if (!status.CorrectGender)
                {
                    node = htmlDoc.DocumentNode.SelectSingleNode("//span[@id=\"genderselect\"]");
                    node.SetAttributeValue("class", "invalid");
                    infoNode.InnerHtml += "Choose gender. ";
                }
                node = htmlDoc.DocumentNode.SelectSingleNode("//input[@id=\"email\"]");
                node.SetAttributeValue("value", HttpUtility.UrlDecode(parameters.GetValue("email")));
                if (!status.CorrectEmail)
                {
                    node.SetAttributeValue("class", "input_register invalid");
                    infoNode.InnerHtml += "E-mail is not valid. ";
                }
                string role = parameters.GetValue("role").ToLower();
                if (Settings.UserTeacher && Settings.UserID != Convert.ToInt32(id))
                {
                    if (role.Equals("student"))
                    {
                        node = htmlDoc.DocumentNode.SelectSingleNode("//input[@id=\"rolestudent\"]");
                        node.SetAttributeValue("checked", "checked");
                    }
                    else if (role.Equals("teacher"))
                    {
                        node = htmlDoc.DocumentNode.SelectSingleNode("//input[@id=\"roleteacher\"]");
                        node.SetAttributeValue("checked", "checked");
                    }
                }
                else
                {
                    node = htmlDoc.DocumentNode.SelectSingleNode("//table[@id=\"updateformtable\"]");
                    node.RemoveChild(htmlDoc.DocumentNode.SelectSingleNode("//tr[@id=\"roleselect\"]"));
                }
                node = htmlDoc.DocumentNode.SelectSingleNode("//input[@id=\"userid\"]");
                node.SetAttributeValue("value", parameters.GetValue("userid"));

            }
            return Encoding.GetEncoding("utf-8").GetBytes(htmlDoc.DocumentNode.OuterHtml);
        }

        private static byte[] ReplyToChangePassword(RequestParameters parameters, bool showSuccess = false)
        {
            HtmlDocument htmlDoc = new HtmlAgilityPack.HtmlDocument();
            htmlDoc.Load(Controller.GetAppResourcePath("Html/_updatepass.html"));
            HtmlNode node;
            string id = parameters.GetValue("userid");
            if (Settings.IsLoggedIn() && !String.IsNullOrWhiteSpace(id) && (Convert.ToInt32(id) == Settings.UserID || Settings.UserTeacher))
            {

                ServiceEntities.User user = ServiceCommunicator.ReceiveUserInformation(id);
                if (user != null)
                {
                    if (showSuccess)
                    {
                        node = htmlDoc.DocumentNode.SelectSingleNode("//div[@id=\"updateformdiv\"]");
                        HtmlNode infoNode = htmlDoc.CreateElement("div");
                        infoNode.SetAttributeValue("class", "message_success");
                        infoNode.InnerHtml = "Password successfully changed.<br />";
                        node.PrependChild(infoNode);
                    }

                    node = htmlDoc.DocumentNode.SelectSingleNode("//input[@id=\"userid\"]");
                    node.SetAttributeValue("value", id);
                    node = htmlDoc.DocumentNode.SelectSingleNode("//strong[@id=\"usernamestr\"]");
                    node.InnerHtml = user.Username;
                    node = htmlDoc.DocumentNode.SelectSingleNode("//input[@id=\"username\"]");
                    node.SetAttributeValue("value", user.Username);

                    if (Settings.UserTeacher && user.Id != Settings.UserID)
                    {
                        node = htmlDoc.DocumentNode.SelectSingleNode("//table[@id=\"updateformtable\"]");
                        node.RemoveChild(htmlDoc.DocumentNode.SelectSingleNode("//tr[@id=\"oldpass\"]"));
                    }
                }
            }
            return Encoding.GetEncoding("utf-8").GetBytes(htmlDoc.DocumentNode.OuterHtml);


        }



        private static byte[] ReplyToFailedChangePassword(RequestParameters parameters, ChangePasswordCheck status)
        {
            HtmlDocument htmlDoc = new HtmlAgilityPack.HtmlDocument();
            htmlDoc.Load(Controller.GetAppResourcePath("Html/_updatepass.html"));
            string id = parameters.GetValue("userid");
            if (Settings.IsLoggedIn() && !String.IsNullOrWhiteSpace(id) && (Convert.ToInt32(id) == Settings.UserID || Settings.UserTeacher))
            {

                HtmlNode node = htmlDoc.DocumentNode.SelectSingleNode("//div[@id=\"updateformdiv\"]");
                HtmlNode infoNode = htmlDoc.CreateElement("div");
                infoNode.SetAttributeValue("class", "message_error");
                infoNode.InnerHtml = "Change password failed, correct highlighted fields.<br />";
                node.PrependChild(infoNode);
                ServiceEntities.User user = ServiceCommunicator.ReceiveUserInformation(id);
                if (user != null)
                {
                    node = htmlDoc.DocumentNode.SelectSingleNode("//input[@id=\"userid\"]");
                    node.SetAttributeValue("value", id);
                    node = htmlDoc.DocumentNode.SelectSingleNode("//strong[@id=\"usernamestr\"]");
                    node.InnerHtml = user.Username;
                    node = htmlDoc.DocumentNode.SelectSingleNode("//input[@id=\"username\"]");
                    node.SetAttributeValue("value", user.Username);

                    if (!status.CorrectOldPassword)
                    {
                        node = htmlDoc.DocumentNode.SelectSingleNode("//input[@id=\"password0\"]");
                        node.SetAttributeValue("class", "input_register invalid");
                        infoNode.InnerHtml += "Old password is not valid. ";
                    }
                    if (!status.CorrectPassword || !status.PasswordMatch)
                    {
                        node = htmlDoc.DocumentNode.SelectSingleNode("//input[@id=\"password1\"]");
                        node.SetAttributeValue("class", "input_register invalid");
                        node = htmlDoc.DocumentNode.SelectSingleNode("//input[@id=\"password2\"]");
                        node.SetAttributeValue("class", "input_register invalid");
                        if (!status.CorrectPassword)
                        {
                            infoNode.InnerHtml += "New password is not valid. ";
                        }
                        if (!status.PasswordMatch)
                        {
                            infoNode.InnerHtml += "New passwords do not match. ";
                        }
                    }
                    if (Settings.UserTeacher && user.Id != Settings.UserID)
                    {
                        node = htmlDoc.DocumentNode.SelectSingleNode("//table[@id=\"updateformtable\"]");
                        node.RemoveChild(htmlDoc.DocumentNode.SelectSingleNode("//tr[@id=\"oldpass\"]"));
                    }
                }
            }
            return Encoding.GetEncoding("utf-8").GetBytes(htmlDoc.DocumentNode.OuterHtml);

        }


        private static byte[] ReplyToSuccessfulRegistration(RequestParameters parameters)
        {
            HtmlDocument htmlDoc = new HtmlAgilityPack.HtmlDocument();
            htmlDoc.Load(Controller.GetAppResourcePath("Html/registered.html"));
            LocalHelpers.MenuHelper.IncludeLocalMenu(htmlDoc);
            HtmlNode node = htmlDoc.DocumentNode.SelectSingleNode("//span[@id=\"susername\"]");
            node.InnerHtml = parameters.GetValue("username");
           
            return Encoding.GetEncoding("utf-8").GetBytes(htmlDoc.DocumentNode.OuterHtml);
        }


        private static byte[] ReplyToUserlist(string relativeUrl, string page = "1")
        {
            HtmlDocument htmlDoc = new HtmlAgilityPack.HtmlDocument();
            if (LocalHelpers.LoginHelper.IncludeLoginRequiredMessage(htmlDoc) == false)
            {
                htmlDoc.LoadHtml("<div id=\"include\"></div>");

                int pag = Convert.ToInt32(page);
                RegisteredUsers reg = null;
                string group = System.Text.RegularExpressions.Regex.Replace(relativeUrl, "^list/", "").ToLower();
                if (group.Equals("everyone"))
                {
                    reg = ServiceCommunicator.ReceiveCompleteUserlist(pag);
                }
                else if (group.Equals("teacher") || group.Equals("student"))
                {
                    reg = ServiceCommunicator.ReceiveUserlist(group, pag);
                }
                IncludeUserlist(htmlDoc, reg);
                IncludePaging(htmlDoc, reg.TotalPages, reg.CurrentPage, group, "user/" + relativeUrl);
            }
            return Encoding.GetEncoding("utf-8").GetBytes(htmlDoc.DocumentNode.OuterHtml);

        }

        private static void IncludeUserlist(HtmlDocument htmlDoc, RegisteredUsers recs)
        {
            if (recs == null || !recs.Users.Any())
            {
                IncludeNoResultsMessage(htmlDoc, "No user has registered yet.");
                return;
            }

            HtmlNode root = htmlDoc.DocumentNode.SelectSingleNode("//div[@id=\"include\"]");
            if (root == null) return;
            HtmlNode resultsNode = htmlDoc.CreateElement("table");
            resultsNode.SetAttributeValue("class", "table_links");
            resultsNode.SetAttributeValue("style", "margin-top: 20px");
            root.AppendChild(resultsNode);
            HtmlNode linkNode;
            linkNode = htmlDoc.CreateElement("tr");
            linkNode.InnerHtml =
                        "<th style=\"width: 25px; text-align: left; padding-left: 15px;\">&nbsp;</th>"
                        + "<th style=\"width: 125px; text-align: left; padding-left: 15px;\">Username</th>"
                        + "<th style=\"width: auto; text-align: left; padding-left: 15px;\">Name and surname</th>"
                        + "<th style=\"width: 180px; text-align: left; padding-left: 15px;\">E-mail</th>"
                        + "<th style=\"width: 60px; text-align: left; padding-left: 15px;\">Role</th>"
                        + "<th style=\"width: 80px; text-align: left; padding-left: 15px;\">Registered</th>"
                        + (Settings.IsTeacher() ? "<th style=\"width: 60px; text-align: center;\">Manage</th>" : "");
            resultsNode.AppendChild(linkNode);

            string tabletd = "<td class=\"user_{0}\" title=\"{1}\"></td>"
                        + "<td class=\"user_username\">{2}</td>"
                        + "<td class=\"user_name\">{3} {4}</td>"
                        + "<td class=\"user_email\"><a href=\"mailto:{5}\">{5}</a></td>"
                        + "<td class=\"user_role\">"
                        + "{6}" //  ((Settings.IsTeacher()) ? "<a href=\"javascript:void(0);\" onclick=\"changeUserRole({8},this);\">{6}</a>" 
                        + "</td><td class=\"table_date\"><span class=\"activity_time\">{7}</span></td>"
                        + ((Settings.IsTeacher()) ? "<td class=\"\" style=\"width: 20px; text-align: center;\"><a href=\"user/settings.html?userid={8}\"><img src=\"graphics/user_edit.png\" class=\"img-4\" alt=\"Edit\" title=\"Edit user\"/></a><a href=\"javascript:void(0);\" onclick=\"var that = this; confirm('Do you want to remove user <strong>{2}</strong>?',function() {{deleteUser({8}, that);}})\"><img src=\"graphics/delete.png\" class=\"img-4\" alt=\"Remove user\" title=\"Remove user\"/></a></td>" : "");

            string gender, role;
            foreach (ServiceEntities.User rec in recs.Users)
            {
                linkNode = htmlDoc.CreateElement("tr");
                linkNode.SetAttributeValue("id", "user" + rec.Id);
                gender = rec.IsMale ? "Male" : "Female";
                role = rec.IsTeacher ? "Teacher" : "Student";

                linkNode.InnerHtml = String.Format(tabletd,
                        gender.ToLower(),
                        gender,
                        rec.Username,
                        rec.Firstname,
                        rec.Surname,
                        rec.Email,
                        role,
                        rec.Registered.ToString("dd.MM.yyyy HH:mm"),
                        rec.Id);
                resultsNode.AppendChild(linkNode);
            }
        }


    
    }
}
