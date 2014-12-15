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

namespace PhoneApp.Controllers
{
    public class InitController
    {
        internal static void Init()
        {
            Database.DatabaseContext.Init();
            UsersController.Init();
            ServersController.Init();
        }
    }
}
