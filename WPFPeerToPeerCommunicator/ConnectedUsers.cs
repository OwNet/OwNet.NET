using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Windows.Threading;

namespace WPFPeerToPeerCommunicator
{
    public class ConnectedUsers : ObservableCollection<ConnectedUser>
    {
        public ConnectedUsers()
            : base()
        {
            DispatcherTimer timer = new DispatcherTimer();
            timer.Tick += new EventHandler(RemoveOldUsers);
            timer.Interval = new TimeSpan(0, 0, 10);
            timer.Start();
        }

        public bool ContainsID(int userID)
        {
            foreach (ConnectedUser user in Items)
            {
                if (user.UserID == userID)
                    return true;
            }
            return false;
        }

        public ConnectedUser UserWithID(int userID)
        {
            foreach (ConnectedUser user in Items)
            {
                if (user.UserID == userID)
                    return user;
            }
            return null;
        }

        private void RemoveOldUsers(object sender, EventArgs e)
        {
            List<ConnectedUser> itemsToRemove = new List<ConnectedUser>();
            foreach (ConnectedUser user in Items)
            {
                TimeSpan span = DateTime.Now.Subtract(user.UpdatedAt);
                if (span.TotalSeconds > 30.0)
                    itemsToRemove.Add(user);
            }

            for (int i = itemsToRemove.Count - 1; i >= 0; i--)
                Remove(itemsToRemove.ElementAt(i));
        }
    }

    public class ConnectedUser
    {
        private int _userID;
        private string _hostname;
        private string _username;
        private DateTime _updatedAt = DateTime.Now;
        private DateTime _createdAt = DateTime.Now;
        private IPAddress _ipAddress = null;

        public ConnectedUser()
        {
        }

        public int UserID
        {
            get { return _userID; }
            set { _userID = value; }
        }

        public string Hostname
        {
            get { return _hostname; }
            set { _hostname = value; }
        }

        public string Username
        {
            get { return _username; }
            set { _username = value; }
        }

        public DateTime UpdatedAt
        {
            get { return _updatedAt; }
        }

        public IPAddress IPAddress
        {
            get { return _ipAddress; }
            set { _ipAddress = value; }
        }

        public void Update()
        {
            _updatedAt = DateTime.Now;
        }
    }
}
