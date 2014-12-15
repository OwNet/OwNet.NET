using System;
using System.Collections.Generic;
using System.Linq;

namespace CentralServerShared
{
    public class CacheUpdater
    {
        public static int MinCacheAccessesBeforeUpdate = 2;
        public static int DefaultUpdateTimeSpan = 24;
        public static int MinUpdateTimeSpan = 12;
        public static int MaxUpdateTimeSpan = 168;
        
        public static void UpdateCacheHash(Cache cache)
        {
            long size = 0;
            string hash = HttpGetRetriever.DownloadNewHash(cache.AbsoluteUri, out size);
            if (hash != null)
            {
                bool changed = cache.Hash != hash;
                cache.Hash = hash;
                cache.DateUpdated = DateTime.Now;
                cache.UpdateAt = null;
                cache.Size = size;

                CreatePreviousUpdate(cache, changed);
            }
        }

        protected static void CreatePreviousUpdate(Cache cache, bool success)
        {
            DateTime lastUpdateTime;
            PreviousUpdate lastUpdate = cache.PreviousUpdates.OrderByDescending(u => u.Date).LastOrDefault();
            if (lastUpdate != null)
                lastUpdateTime = lastUpdate.Date;
            else
                lastUpdateTime = cache.DateCreated;

            cache.PreviousUpdates.Add(new PreviousUpdate()
            {
                Date = DateTime.Now,
                Success = success,
                HoursSincePreviousUpdate = (DateTime.Now - lastUpdateTime).TotalHours
            });
        }

        public static DateTime CalculateNextUpdateDate(Cache cache)
        {
            var updates = cache.PreviousUpdates.OrderByDescending(u => u.Date).Take(3);
            int successCount = 0, failCount = 0;
            List<int> timeSpans = new List<int>();
            DateTime dt = DateTime.Now;
            int thisTimeSpan = 0;

            foreach (PreviousUpdate update in updates)
            {
                if (update.Success)
                    successCount++;
                else
                    failCount++;

                timeSpans.Add((int)update.HoursSincePreviousUpdate);
                dt = update.Date;
            }

            if (timeSpans.Count < 2)
                thisTimeSpan = DefaultUpdateTimeSpan;
            else if (timeSpans.Count == 2)
                thisTimeSpan = timeSpans.Last();
            else if (successCount == 3)
                thisTimeSpan = (int)(0.9 * timeSpans.Last());
            else if (failCount == 3)
                thisTimeSpan = timeSpans.Sum();
            else if (successCount > failCount)
                thisTimeSpan = timeSpans.Max();
            else if (failCount > successCount)
                thisTimeSpan = timeSpans.Sum();

            thisTimeSpan = Math.Max(Math.Min(thisTimeSpan, MaxUpdateTimeSpan), MinUpdateTimeSpan);

            if (dt.AddHours(thisTimeSpan) < DateTime.Now)
                return DateTime.Now;

            return dt.AddHours(thisTimeSpan);
        }
    }
}