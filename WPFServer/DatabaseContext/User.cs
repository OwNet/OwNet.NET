using System;
using System.Collections.Generic;
using System.Data.Entity;
using Helpers;

namespace WPFServer.DatabaseContext
{
    public enum UserGender : byte
    {
        Male = 0,
        Female = 1
    }

    public class User : DbContext, DbFetch       // pouzivatel -- TODO
    {
        public User()
        {
            Firstname = "";
            Surname = "";
            Username = "Anonymous";
            IsTeacher = false;
            LastVisit = DateTime.Now;
            LastLogin = DateTime.Now;
            Registered = DateTime.Now;
            LastDisplayedRecom = DateTime.Now;
        }
        public int Id { get; set; }

        public string Firstname { get; set; }

        public string Surname { get; set; }

        public string Username { get; set; }

        public string Email { get; set; }

        public DateTime LastDisplayedRecom { get; set; }


        public int GenderValue
        {
            get;
            set; 
        }

        public UserGender Gender
        {
            get { return (UserGender)GenderValue; }
            set { GenderValue = (int)value; }
        }

        public string PasswordHash { get; set; } 

        public string PasswordSalt { get; set; }

        public string Password
        {
            get { return ""; }
            set
            {
                if (IsValidPassword(value))
                {
                    SaltedHash shash = new SaltedHash();
                    string hash;
                    string salt;
                    shash.GetHashAndSaltString(value, out hash, out salt);
                    this.PasswordHash = hash;
                    this.PasswordSalt = salt;
                }
            }
        }

        public bool IsTeacher
        { get; set; }


        public static bool IsValidPassword(string pass)
        {
            if (pass.Length < 4) return false;
            return true;
        }

        public bool VerifiyPassword(string pass)
        {
            SaltedHash hash = new SaltedHash();
            return hash.VerifyHashString(pass, this.PasswordHash, this.PasswordSalt);
        }

        public DateTime LastVisit { get; set; }

        public DateTime LastLogin { get; set; }

        public DateTime Registered { get; set; }

        public static int GetHashUsername(string username)
        {
            return username.ToLower().GetHashCode();
        }

        public virtual ICollection<UserVisitsPage> Visits { get; set; }
        public virtual ICollection<UserTraversesEdge> Traverses { get; set; }
		public virtual ICollection<GroupRecommendation> GroupRecommendations { get; set; }
        public virtual ICollection<Activity> Activities { get; set; }
        public virtual ICollection<SharedFile> SharedFiles { get; set; }
		public virtual ICollection<UserGroup> UserGroups { get; set; }
		public virtual ICollection<AdminGroup> AdminGroups { get; set; }

        public virtual ICollection<UserSimilarity> LeftNeighbors { get; set; }
        public virtual ICollection<UserSimilarity> RightNeighbors { get; set; }

        public virtual ICollection<Prediction> Predictions { get; set; }


        public Table GetTableType() { return Table.User; }
        public System.Linq.Expressions.Expression<Func<T, bool>> GetCreateConstraint<T>()
        {
            return y => this.Username.ToLower().Equals((y as User).Username.ToLower());
        }
        public void Update(DbFetch updateItem)
        {
            User item = updateItem as User;
            this.IsTeacher = item.IsTeacher;
            this.Gender = item.Gender;
            this.Firstname = item.Firstname;
            this.Surname = item.Surname;
            this.Email = item.Email;
            return;
        }

        public class EntityConfiguration : System.Data.Entity.ModelConfiguration.EntityTypeConfiguration<User>
        {
            public EntityConfiguration()
            {
                Property(m => m.Username).IsRequired();
                Property(m => m.PasswordHash).IsRequired();
                Property(m => m.PasswordSalt).IsRequired();
                Ignore(m => m.Password);
                HasKey(m => m.Id);
                Property(m => m.Id).HasDatabaseGeneratedOption(System.ComponentModel.DataAnnotations.DatabaseGeneratedOption.Identity);
                Ignore(m => m.Gender);
                Property(m => m.GenderValue).IsRequired();
            }
        }
    }

    
}
