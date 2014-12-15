using System;
using System.Text;
using HtmlAgilityPack;
using ServiceEntities;
using WPFProxy.Proxy;

namespace WPFProxy.LocalResponders.My.Files
{
    class FilesResponder : MyDomainResponder
    {

        protected override string ThisUrl { get { return "files/"; } }
        public FilesResponder() : base()
        {
        }
        protected override void InitRoutes()
        {
            Routes
                .RegisterRoute("GET", "", Index)
                .RegisterRoute("GET", "index.html", Index);
        }

        // GET: my.ownet/files/?id={id}
        // GET: my.ownet/files/index.html?id={id}
        private static ResponseResult Index(string method, string relativeUrl, RequestParameters parameters)
        {
            byte[] data = null;
            if (RequestParameters.IsNullOrEmpty(parameters))
            {
                data = Show();
            }
            else
            {
                data = Show(Convert.ToInt32(parameters.GetValue("id")));
            }
            return new ResponseResult() { Data = data };
        }

        internal static byte[] Show(int folderId = 0)
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
                            sharedFile.Title,
                            LocalHelpers.ActivitiesHelper.NlToBr(sharedFile.Description),
                            sharedFile.Username,
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
                    node.InnerHtml = folder.Name;

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
    }
}
