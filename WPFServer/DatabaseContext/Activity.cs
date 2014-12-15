using System;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration;

namespace WPFServer.DatabaseContext
{
    public enum ActivityType : int
    {
        Visit = 0,
        Rating = 10,
        Recommend = 20,
        Tag = 30,
        Share = 40,
        Register = 50,
        Search = 60,    // 70 is Live Stream Message
        Group = 80,
        None = 100
    }

    public enum ActivityAction : int
    {
        Create = 10,
        Read = 20,
        Update = 30,
        Delete = 40
    }

    public class Activity : DbContext
    {
        public Activity()
        {
            TimeStamp = DateTime.Now;
            TypeValue = (int)ActivityType.None;
            ActionValue = (int)ActivityAction.Read;
            Page = null;
            File = null;
            Group = null;
            Reported = false;
            Visible = true;
        }

        public int Id
        {
            get;
            set; 
        }

        public DateTime TimeStamp { get; set; }

        public string Message { get; set; }

        public int TypeValue { get; set; }
        public ActivityType Type
        {
            get { return (ActivityType)TypeValue; }
            set { TypeValue = (int)value; }
        }

        public int ActionValue { get; set; }

        public ActivityAction Action
        {
            get { return (ActivityAction)ActionValue; }
            set { ActionValue = (int)value; }
        }

        public int UserId { get; set; }

        public virtual User User { get; set; }

        public int? PageId { get; set; }

        public virtual Page Page { get; set; } 

        public int? GroupId { get; set; }

        public virtual Group Group { get; set; }

        public int? FileId { get; set; }

        public virtual SharedFile File { get; set; }

        public bool Reported
        {
            get;
            set;
        }

        public bool Visible { get; set; }
        public class EntityConfiguration : EntityTypeConfiguration<Activity>
        {
            public EntityConfiguration()
            {
                HasKey(n => n.Id);
                HasRequired(n => n.User).WithMany(u => u.Activities).HasForeignKey(n => n.UserId).WillCascadeOnDelete(false);
                HasOptional(n => n.Page).WithMany(u => u.Activities).HasForeignKey(n => n.PageId).WillCascadeOnDelete(false);
                HasOptional(n => n.File).WithMany(u => u.Activities).HasForeignKey(n => n.FileId).WillCascadeOnDelete(false);
                HasOptional(n => n.Group).WithMany(u => u.Activities).HasForeignKey(n => n.GroupId).WillCascadeOnDelete(false);
                Property(n => n.Message).IsRequired();
                Property(n => n.TypeValue).IsRequired();
                Property(n => n.ActionValue).IsRequired();
                Property(n => n.TimeStamp).IsRequired();
                Ignore(n => n.Type);
                Ignore(n => n.Action);
            }
        }
   
    }

    
}
