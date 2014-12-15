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
    public abstract class SharedItem : Helpers.ListBoxBaseItem
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public abstract bool IsFolder { get; }
    }
}
