using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;
using System.IO;
using ClientAndServerShared;

namespace WPFProxy.LocalHelpers
{
    class HtmlHelper
    {
        internal static HtmlDocument HtmlDocumentWithLayout(string relFileName, LocalResponders.LocalResponder.LocalLayout layout)
        {
            string layoutName = "";
            relFileName = String.Format("Html/{0}", relFileName);
            HtmlDocument htmlDoc = new HtmlAgilityPack.HtmlDocument();

            switch (layout)
            {
                case LocalResponders.LocalResponder.LocalLayout.Frame:
                    layoutName = "frame";
                    break;

                default:
                    htmlDoc.Load(Controller.GetAppResourcePath(relFileName));
                    return htmlDoc;
            }

            string content = "";
            try
            {
                using (StreamReader rdr = File.OpenText(Controller.GetAppResourcePath(relFileName)))
                {
                    content = rdr.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                LogsController.WriteException(ex.Message);
            }

            htmlDoc.Load(Controller.GetAppResourcePath(String.Format("Html/layouts/{0}.html", layoutName)));
            HtmlNode includeNode = htmlDoc.DocumentNode.SelectSingleNode("//div[@id=\"layout_include\"]");

            if (includeNode != null)
                includeNode.InnerHtml = content;

            return htmlDoc;
        }
    }
}
