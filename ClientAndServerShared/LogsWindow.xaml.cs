using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ClientAndServerShared
{
    /// <summary>
    /// Interaction logic for LogsWindow.xaml
    /// </summary>
    public partial class LogsWindow : Window
    {
        public LogsWindow()
        {
            InitializeComponent();

            listExceptions.ItemsSource = LogsController.ExceptionsList;
            listAccessLogs.ItemsSource = LogsController.PageAccessList;
            listMessages.ItemsSource = LogsController.MessagesList;
        }
    }
}
