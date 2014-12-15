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
using System.Collections.Generic;

namespace PhoneApp.ServiceEntities
{
    public class SharedFolder : SharedItem
    {
        public SharedFolder()
        {
            ChildFolders = new List<SharedFolder>();
            Files = new List<SharedFile>();
        }

        public int Id { get; set; }

        public List<SharedFolder> ChildFolders { get; set; }

        public List<SharedFile> Files { get; set; }

        public override bool IsFolder { get { return true; } }

        public List<SharedItem> AllItems
        {
            get
            {
                var items = new List<SharedItem>();
                foreach (SharedItem item in ChildFolders)
                    items.Add(item);
                foreach (SharedItem item in Files)
                    items.Add(item);
                return items;
            }
        }
    }
}
