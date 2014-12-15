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
using WPFProxy.Proxy;

namespace WPFProxy
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = Settings.LogIn(txtUsername.Text, txtPassword.Password);
            this.Close();
        }

        private void btnRegister_Click(object sender, RoutedEventArgs e)
        {
            AppHelpers.OpenProxyUrl(HttpLocalResponder.RegisterUrl);
        }
    }
}
