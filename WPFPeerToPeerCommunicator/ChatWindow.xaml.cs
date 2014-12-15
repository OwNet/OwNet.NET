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

namespace WPFPeerToPeerCommunicator
{
    /// <summary>
    /// Interaction logic for ChatWindow.xaml
    /// </summary>
    public partial class ChatWindow : Window
    {
        public ChatWindow()
        {
            Controller.ChatWindow = this;
            Resources["ConnectedUsers"] = Controller.Communicator.Users;

            InitializeComponent();

            Controller.StartChat();
        }

        private void buttonSend_Click(object sender, RoutedEventArgs e)
        {
            Controller.SendBroadcastChatMessage(txtMessage.Text);
            txtMessage.Text = "";
        }

        public void ReceivedMessage(string from, string message)
        {
            listReceivedMessages.Dispatcher.Invoke(
                System.Windows.Threading.DispatcherPriority.Normal,
                new Action(delegate()
            {
                listReceivedMessages.Items.Add(from + ": " + message);
            }));
        }

        private void txtUsername_TextChanged(object sender, TextChangedEventArgs e)
        {
            Settings.Username = txtUsername.Text;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Controller.StopChat();
        }
    }
}
