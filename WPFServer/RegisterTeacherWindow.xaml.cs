using System.Windows;
using System.Windows.Controls;
using Helpers;
using ServiceEntities;

namespace WPFServer
{
    /// <summary>
    /// Interaction logic for RegisterTeacherWindow.xaml
    /// </summary>
    public  partial class RegisterTeacherWindow : Window
    {
        public RegisterTeacherWindow()
        {
            InitializeComponent();
        }

        public string Firstname
        {
            get { return TextBoxFirstname.Text; }
            set { TextBoxFirstname.Text = value; }
        }
        public string Surname
        {
            get { return TextBoxSurname.Text; }
            set { TextBoxSurname.Text = value; }
        }
        public string Username
        {
            get { return TextBoxUsername.Text; }
            set { TextBoxUsername.Text = value; }
        }
        public string Password1
        {
            get { return PasswordBox.Password; }
            set { PasswordBox.Password = value; }
        }
        public string Password2
        {
            get { return PasswordBoxConfirm.Password; }
            set { PasswordBoxConfirm.Password = value; }
        }
        public bool IsMale
        {
            get { return RadioGenderMale.IsChecked ?? false; }
            set { RadioGenderMale.IsChecked = true; }
        }
        public string Email
        {
            get { return TextBoxEmail.Text; }
            set { TextBoxEmail.Text = value; }
        }

        private bool _reallyClose = false;
        private bool CancelRegister()
        {
            if (_reallyClose == true) return true;
            MessageBoxResult res = MessageBox.Show("Are you sure to cancel registration of teacher?", "Cancel Registration", MessageBoxButton.YesNo, MessageBoxImage.Question);
            switch (res)
            {
                case MessageBoxResult.Yes:
                    _reallyClose = true;
                    return true;
                case MessageBoxResult.No:
                case MessageBoxResult.Cancel:
                    break;
            }
            return false;
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ButtonRegister_Click(object sender, RoutedEventArgs e)
        {

            RegistrationCheck status = new RegistrationCheck(Username, Firstname, Surname, Password1, Password2, IsMale ? "male" : "female", Email);
            if (status.IsCorrect)
            {
                UserService userv = new UserService();
                UserRegisters teacher = new UserRegisters() { Username = Username, Firstname = Firstname, Surname = Surname, Email = Email, IsMale = IsMale, Password = Password1 };
                status.UserRegistered = userv.RegisterTeacher(teacher);
                if (status.WasSuccessful)
                {
                    MessageBox.Show("Teacher " + Firstname + " " + Surname + " has been successfully registered.\nUse your username and password to log in on homepage.", "Successful Registration", MessageBoxButton.OK, MessageBoxImage.Information);
                    _reallyClose = true;
                    Close();
                }
                else
                {
                    MessageBox.Show("User is already registered.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                string message = "Correct fields:\n";
                if (!status.CorrectUsername)
                {
                    message += "- Username is not valid.\n";
                }
                else if (status.IsCorrect && !status.UserRegistered)
                {
                    message += "- Username is already registered.\n";
                }
            
                if (!status.CorrectFirstname)
                { message += "- Firstname is not valid.\n";
                }
                if (!status.CorrectSurname)
                {
                    message += "- Surname is not valid.\n";
                }
            
            
                if (!status.CorrectGender)
                {
                    message += "- Choose gender.\n";
                }
           
                if (!status.CorrectEmail)
                {
                    message += "- E-mail is not valid.\n";
                }


                if (!status.CorrectPassword) {
                  message += "- Password is not valid.\n";
                }
                if (!status.CorrectPassword) {
                    message += "- Passwords do not match.\n";
                }

                    MessageBox.Show(message, "Correct fields", MessageBoxButton.OK, MessageBoxImage.Error);
                }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            
            if (CancelRegister() == false)
                e.Cancel = true;
        }
    }


    

  


}
