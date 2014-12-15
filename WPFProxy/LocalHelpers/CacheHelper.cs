using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;
using ClientAndServerShared;
using WPFProxy.Database;

namespace WPFProxy.LocalHelpers
{
    class CacheHelper
    {
        internal static TagBuilder ExeptionsTableRow(TagBuilder tag, Blacklist item)
        {
            return tag
                .StartTag("tr")
                    .StartTag("td")
                        .AddContent(item.Title)
                    .EndTag() // td
                    .StartTag("td")
                        .AddContent(item.RegularExpression)
                    .EndTag() // td
                    .StartTag("td")
                        .StartTag("a")
                            .AddAttribute("href", "#")
                            .AddAttribute("onclick", String.Format("removeException({0}, this); return false;", Convert.ToString(item.Id)))
                            .StartTag("img")
                                .AddAttribute("src", "graphics/delete.png")
                                .AddAttribute("alt", "Remove")
                            .EndTag() // img
                        .EndTag() // a
                    .EndTag() // td
                .EndTag(); // tr
        }

        internal static void SettingsInit(HtmlDocument htmlDoc, List<Blacklist> blacklist)
        {
            if (Controller.DoNotCache)
                htmlDoc.DocumentNode.SelectSingleNode("//input[@name='caching' and @value='off']").SetAttributeValue("checked", "checked");
            else
                htmlDoc.DocumentNode.SelectSingleNode("//input[@name='caching' and @value='on']").SetAttributeValue("checked", "checked");

            TagBuilder tag = new TagBuilder();
            foreach (Blacklist item in blacklist)
            {
                tag = LocalHelpers.CacheHelper.ExeptionsTableRow(tag, item);
            }
            htmlDoc.DocumentNode.SelectSingleNode("//tbody[@id='exceptions_tbody']").InnerHtml = tag.ToString();
        }
    }
}
