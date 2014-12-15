using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;

namespace WPFProxy.LocalHelpers
{
    class LoginHelper
    {
        internal static bool IncludeLoginRequiredMessage(HtmlDocument htmlDoc, string message = null)
        {
            if (!Settings.IsLoggedIn())
            {
                string msg;
                if (message == null)
                {
                    msg = "<!-- Error --><div class=\"message_status\"> <div class=\"message_error\">You have to be logged in to use this feature.</div></div>";
                }
                else
                {
                    msg = "<!-- Error --><div class=\"message_status\"> <div class=\"message_error\">" + message + "</div></div>";
                }
                htmlDoc.LoadHtml(msg);
                return true;
            }
            return false;
        }
    }
}
