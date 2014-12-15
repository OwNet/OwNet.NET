using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;
using System.IO;

namespace WPFProxy.LocalHelpers
{
    class MenuHelper
    {
        internal static byte[] ReplyToPageWithMenu(string relativeUri)
        {
            HtmlDocument htmlDoc = new HtmlAgilityPack.HtmlDocument();
            htmlDoc.Load(Controller.GetAppResourcePath("Html/" + relativeUri), Encoding.UTF8);

            if (htmlDoc != null) IncludeLocalMenu(htmlDoc);
            return Encoding.GetEncoding("utf-8").GetBytes(htmlDoc.DocumentNode.OuterHtml);
        }

        internal static void IncludeLocalMenu(HtmlDocument htmlDoc)
        {
            HtmlNode menuNode = htmlDoc.DocumentNode.SelectSingleNode("//div[@id=\"menubar\"]");
            menuNode.RemoveAllChildren();
            StreamReader streamReader = null;
            string menu;
            if (Settings.IsLoggedIn())
            {
                using (streamReader = new StreamReader(Controller.GetAppResourcePath(@"Html/_menuIN.html")))
                {
                    menu = streamReader.ReadToEnd();
                    menu = String.Format(menu, Settings.UserID, Settings.UserFirstname + " " + Settings.UserSurname);
                    streamReader.Close();
                }
            }
            else
            {
                using (streamReader = new StreamReader(Controller.GetAppResourcePath(@"Html/_menuOUT.html")))
                {
                    menu = streamReader.ReadToEnd();
                    streamReader.Close();
                }
            }

            menuNode.InnerHtml = menu;
        }
    }
}
