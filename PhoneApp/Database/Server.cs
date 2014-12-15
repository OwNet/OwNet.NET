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
using System.Data.Linq.Mapping;

namespace PhoneApp.Database
{
    [Table]
    public class Server
    {
        [Column(IsPrimaryKey = true, DbType = "INT NOT NULL", CanBeNull = false, AutoSync = AutoSync.OnInsert)]
        public int Id { get; set; }

        [Column(CanBeNull = false)]
        public string ServerUsername { get; set; }

        [Column(CanBeNull = false)]
        public string Address { get; set; }

        [Column(CanBeNull = false)]
        public DateTime DateCreated { get; set; }

        [Column(CanBeNull = false)]
        public DateTime DateModified { get; set; }
    }
}
