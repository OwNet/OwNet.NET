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
using System.Data.Linq;
using Microsoft.Phone.Data.Linq;

namespace PhoneApp.Database
{
    public class DatabaseContext : DataContext
    {
        public static string DBConnectionString = "Data Source=isostore:/Data.sdf";
        private static int _dbSchemaVersion = 6;

        public DatabaseContext(string connectionString) : base(connectionString) { }

        internal static void Init()
        {
            using (DatabaseContext db = new DatabaseContext("isostore:/Data.sdf"))
            {
                if (db.DatabaseExists() == false)
                    db.CreateDatabase();

                DatabaseSchemaUpdater dbUpdater = db.CreateDatabaseSchemaUpdater();
                if (dbUpdater.DatabaseSchemaVersion < _dbSchemaVersion)
                {
                    dbUpdater.DatabaseSchemaVersion = _dbSchemaVersion;
                    dbUpdater.Execute();
                }
            }
        }

        // +++ Tables +++

        public Table<User> Users;
        public Table<Group> Groups;
        public Table<UserGroup> UserGroups;
        public Table<Recommendation> Recommendations;
        public Table<Setting> Settings;
        public Table<Server> Servers;

        // --- Tables ---
    }
}
