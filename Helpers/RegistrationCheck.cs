using System;
using System.Text.RegularExpressions;

namespace Helpers
{


    public partial class Check
    {
        protected static Regex UsernameRegex = new Regex(@"^[a-zA-Z0-9_-]{4,100}$", RegexOptions.Compiled);
        protected static Regex NameRegex = new Regex(@"^[a-zA-Z0-9 .,]{2,100}$", RegexOptions.Compiled);
        protected static Regex PasswordRegex = new Regex(@"^[a-zA-Z0-9]{4,100}$", RegexOptions.Compiled);

        public static int UsernameMinLength { get { return 4; } }

        public Check()
        {
            CorrectUsername = false;
            CorrectFirstname = false;
            CorrectSurname = false;
            CorrectEmail = false;
            CorrectGender = false;
        }

        public static string UsernameConstraintMessage
        {
            get { return "Username: min 4 letters and numbers, dashes, underscores."; }
        }
        public static string NameConstraintMessage
        {
            get { return "Firstname: min 2 letters and numbers, dots, commas."; }
        }
        public static string PasswordConstraintMessage
        {
            get { return "Password: min 4 letters and numbers."; }
        }

        public bool WasSuccessful { get { return false; } }
        public bool IsCorrect { get { return false; } }

        protected bool CheckInput(Regex regex, string input)
        {
            if (input == null) return false;

            return regex.IsMatch(input);
        }

        public bool CorrectGender { get; protected set; }

        public bool CorrectUsername { get; protected set; }
        public bool CorrectFirstname { get; protected set; }
        public bool CorrectSurname { get; protected set; }
        public bool CorrectEmail { get; protected set; }

    }

    public class RegistrationCheck : Check
    {
        public RegistrationCheck() : base()
        {
            UserRegistered = false;
            CorrectPassword = false;
            PasswordMatch = false;
        }
        public RegistrationCheck(string username, string firstname, string surname, string password1, string password2, string gender, string email) : this()
        {
            if (CheckInput(UsernameRegex, username))
                CorrectUsername = true;
            if (CheckInput(NameRegex, firstname))
                CorrectFirstname = true;
            if (CheckInput(NameRegex, surname))
                CorrectSurname = true;
            if (CheckInput(PasswordRegex, password1))
                CorrectPassword = true;
            if (CheckInput(PasswordRegex, password2) && password1.Equals(password2))
                PasswordMatch = true;
            try
            {
                if (!String.IsNullOrWhiteSpace(email))
                {
                    var addr = new System.Net.Mail.MailAddress(email);
                }
                CorrectEmail = true;
            }
            catch (Exception)
            {
                CorrectEmail = false;
            }

            if (gender.ToLower() == "male" || gender.ToLower() == "female")
                CorrectGender = true;
        }
        public bool CorrectPassword { get; protected set; }
        public bool PasswordMatch { get; protected set; }

        public new bool WasSuccessful { get { return IsCorrect && UserRegistered; } }

        public new bool IsCorrect { get { return CorrectGender && CorrectFirstname && CorrectUsername && CorrectSurname && CorrectPassword && CorrectEmail && PasswordMatch; } }

        
        public bool UserRegistered { get; set; }
    }

    public class UpdateCheck : Check
    {
        public UpdateCheck() : base()
        {
        }
        public UpdateCheck(string firstname, string surname, string gender, string email) : this()
        {
            if (CheckInput(NameRegex, firstname))
                CorrectFirstname = true;
            if (CheckInput(NameRegex, surname))
                CorrectSurname = true;
            try
            {
                if (!String.IsNullOrWhiteSpace(email))
                {
                    var addr = new System.Net.Mail.MailAddress(email);
                }
                CorrectEmail = true;
            }
            catch (Exception)
            {
                CorrectEmail = false;
            }

            if (gender.ToLower() == "male" || gender.ToLower() == "female")
                CorrectGender = true;
        }

        public new bool WasSuccessful { get { return IsCorrect && UserUpdated; } }

        public new bool IsCorrect { get { return CorrectGender && CorrectFirstname && CorrectSurname && CorrectEmail; } }

        public bool UserUpdated { get; set; }
    }

    public class ChangePasswordCheck : Check
    {
        public ChangePasswordCheck()
            : base()
        {
            CorrectOldPassword = false;
            CorrectPassword = false;
            PasswordMatch = false;
            PasswordChanged = false;
        }
        public ChangePasswordCheck(string oldPassword, string newPassword, string newPassword2)
            : this()
        {
            if (CheckInput(PasswordRegex, oldPassword))
                CorrectOldPassword = true;
            if (CheckInput(PasswordRegex, newPassword))
                CorrectPassword = true;
            if (CheckInput(PasswordRegex, newPassword2) && newPassword.Equals(newPassword2))
                PasswordMatch = true;
        }

        public new bool WasSuccessful { get { return IsCorrect && PasswordChanged; } }
        public bool CorrectOldPassword { get; protected set; }
        public new bool IsCorrect { get { return CorrectOldPassword && CorrectPassword && PasswordMatch; } }
        public bool CorrectPassword { get; protected set; }
        public bool PasswordMatch { get; protected set; }
        public bool PasswordChanged { get; set; }
    }
   
}
