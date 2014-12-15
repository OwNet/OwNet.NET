using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Text.RegularExpressions;

namespace WPFProxy.LocalHelpers
{
    public class TagHelper
    {

        public static List<string> GetTags(string val)
        {
            List<string> tags = new List<string>();

            foreach (Match match in new Regex(@"([\w\-\.]+)").Matches(HttpUtility.UrlDecode(val)))
            {
                if (!String.IsNullOrWhiteSpace(match.Value))
                {
                    tags.Add(match.Value);
                }
            }
           
            return tags;
        }

    }
}
