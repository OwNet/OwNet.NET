using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;
using WPFProxy.LocalResponders;

namespace WPFProxy.LocalHelpers
{
    class FrameHelper
    {
        internal static ResponseResult FrameLogin()
        {
            HtmlDocument htmlDoc = HtmlHelper.HtmlDocumentWithLayout("iframeLogin.html", LocalResponders.LocalResponder.LocalLayout.Frame);
            return new ResponseResult() { Filename = "iframeLogin.html", Data = Encoding.GetEncoding("utf-8").GetBytes(htmlDoc.DocumentNode.OuterHtml) };
        }

        internal static HtmlDocument GetHtmlDocument(string fileName, string parentUri)
        {
            HtmlDocument htmlDoc = LocalHelpers.HtmlHelper.HtmlDocumentWithLayout(fileName, LocalResponders.LocalResponder.LocalLayout.Frame);
            HtmlNode node = htmlDoc.DocumentNode.SelectSingleNode("//input[@id=\"parent-uri\"]");
            node.SetAttributeValue("value", parentUri);
            node = htmlDoc.DocumentNode.SelectSingleNode("//a[@id=\"login-username\"]");
            node.InnerHtml = Settings.UserFirstname + " " + Settings.UserSurname;
            if (Settings.UserTeacher)
            {
                node.InnerHtml += " (teacher)";
            }
            else
            {
                node.InnerHtml += " (student)";
            }
            return htmlDoc;
        }
    }
}
