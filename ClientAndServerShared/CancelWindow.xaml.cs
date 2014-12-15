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
    /// Interaction logic for CancelWindow.xaml
    /// </summary>
    public partial class CancelWindow : Window, INotifyFinished
    {
        CancelObject _cancelObject = null;
        private bool _canceled = false;

        public CancelWindow(CancelObject cancelObject, bool cancelEnabled, string customMessage)
        {
            _cancelObject = cancelObject;
            _cancelObject.NotifyFinished = this;

            InitializeComponent();
            btnCancel.IsEnabled = cancelEnabled;
            if (customMessage != "")
                lblMessage.Content = customMessage;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            btnCancel.IsEnabled = false;
            _canceled = true;
            _cancelObject.IsCanceled = true;
            _cancelObject.WaitUntilFinished();
        }

        public void Finished()
        {
            if (!_canceled)
            {
                Action act = () =>
                {
                    Close();
                };
                this.Dispatcher.BeginInvoke(act);
            }
        }
    }
}
