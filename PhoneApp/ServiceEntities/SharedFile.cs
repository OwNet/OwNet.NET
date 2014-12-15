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

namespace PhoneApp.ServiceEntities
{
    public class SharedFile : SharedItem
    {
        public int FileObjectId { get; set; }

        public string Username { get; set; }

        public override bool IsFolder { get { return false; } }
    }
}
