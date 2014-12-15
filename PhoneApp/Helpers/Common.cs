using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace PhoneApp.Helpers
{
    public class Common
    {
        public static string GetFullString(DateTime datetime)
        {
            return datetime.ToString("yyyy'-'MM'-'dd'T'HH'_'mm'_'ss.fffffffK");
        }
    }
}
