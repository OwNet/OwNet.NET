using System;
using System.Collections.Generic;
using System.Linq;
using ServiceEntities;

namespace WPFServer
{
    class LiveStream
    {
        private static List<Activity> _activityStream = new List<Activity>();
        private static List<DateTime> _activityDates = new List<DateTime>();

        public static List<Activity> ActivityStream
        {
            get { lock (_activityStream) return _activityStream.OrderByDescending(x => x.DateTime).ToList(); }
        }

        internal static List<Activity> GetNewerThan(DateTime since)
        {
            lock (_activityStream)
            {
                int i = _activityDates.BinarySearch(since);
                if (i < 0)
                    return _activityStream.Where(x => x.DateTime > since).OrderByDescending(x => x.DateTime).ToList();

                int count = _activityStream.Count;
                i++;
                if (i < count)
                {
                    List<Activity> range = _activityStream.GetRange(i, count - i);
                    range.Reverse();
                    return range;
                }

                return new List<Activity>();
            }
        }

        internal static void AddActivity(Activity activity)
        {
            lock (_activityStream)
            {
                if (_activityStream.Count > 0 && _activityDates.Last() > activity.DateTime)
                {
                    int i = _activityStream.Count - 1;
                    for (; i >= 0 && _activityDates[i] > activity.DateTime; i--)
                    {
                    }
                    i++;
                    _activityStream.Insert(i, activity);
                    _activityDates.Insert(i, activity.DateTime);
                }
                else
                {
                    _activityStream.Add(activity);
                    _activityDates.Add(activity.DateTime);
                }

                while (_activityStream.Count > 100)
                {
                    _activityStream.RemoveAt(0);
                    _activityDates.RemoveAt(0);
                }
            }
        }

        internal static void ClearAllActivity()
        {
            lock (_activityStream)
            {
                _activityStream.Clear();
                _activityDates.Clear();
            }
        }

        internal static void AddActivity(DatabaseContext.Activity activityItem)
        {
            AddActivity(new ActivityEntity(activityItem));
        }
    }
}
