using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using ClientAndServerShared;
using WPFProxy.Proxy;
using WPFProxy.Database;

namespace WPFProxy.Cache
{
    public class CachingExceptions
    {
        // Tuple: regex, dont_cache_on_server
        private static List<Tuple<Regex, bool>> _blacklist = new List<Tuple<Regex, bool>>();

        internal static void Init()
        {
            try
            {
                DatabaseEntities database = Controller.GetDatabase();
                var blacklistItems = from p in database.Blacklist select p;

                if (!blacklistItems.Any())
                {
                    database.Blacklist.AddObject(new Blacklist()
                    {
                        RegularExpression = @"^http://.*facebook\.com.*",
                        CacheOnServer = 0,
                        Title = "Facebook"
                    });
                    database.Blacklist.AddObject(new Blacklist()
                    {
                        RegularExpression = @"^http://.*ak\.fbcdn\.net.*",
                        CacheOnServer = 0,
                        Title = "Facebook"
                    });
                    database.Blacklist.AddObject(new Blacklist()
                    {
                        RegularExpression = @"^http://.*youtube\.com/videoplayback.*",
                        CacheOnServer = 1,
                        Title = "YouTube"
                    });
                    database.Blacklist.AddObject(new Blacklist()
                    {
                        RegularExpression = @"^http://.*\.(?:ogg|ogv|wmv|avi|mp4|flv)(?:$|[/?&%])",
                        CacheOnServer = 1,
                        Title = "Videos"
                    });
                    Controller.SubmitDatabaseChanges(database);
                    blacklistItems = from p in database.Blacklist select p;
                }
                if (blacklistItems.Any())
                {
                    foreach (Blacklist item in blacklistItems)
                    {
                        _blacklist.Add(new Tuple<Regex, bool>(new Regex(item.RegularExpression, RegexOptions.Compiled),
                            item.CacheOnServer == 0));
                    }
                }
                database.Dispose();
            }
            catch (Exception ex)
            {
                LogsController.WriteException("Load blacklist", ex.Message);
            }
        }

        // 0 - can cache, 1 - cant cache on client, 2 - cant cache on client and server
        public static int DoNotCache(ProxyEntry entry)
        {
            if (entry.IsPost || Controller.DoNotCache)
                return 2;
            return MatchesBlacklist(entry.AbsoluteUri);
        }

        public static void SetCanCacheOnEntry(ProxyEntry entry)
        {
            entry.CanCache = (int)(ProxyEntry.CanCacheOptions.CanCacheOnClient | ProxyEntry.CanCacheOptions.CanCacheOnServer);

            if (entry.IsPost || Controller.DoNotCache)
                entry.CanCache = (int)ProxyEntry.CanCacheOptions.CantCache;
            else
            {
                foreach (Tuple<Regex, bool> regex in _blacklist)
                {
                    if (regex.Item1.IsMatch(entry.AbsoluteUri))
                    {
                        entry.CanCache = (int)ProxyEntry.CanCacheOptions.CantCache;
                        if (!regex.Item2)
                            entry.CanCache = (int)ProxyEntry.CanCacheOptions.CanCacheOnServer;
                        break;
                    }
                }
            }
        }

        private static int MatchesBlacklist(string url)
        {
            foreach (Tuple<Regex, bool> regex in _blacklist)
            {
                if (regex.Item1.IsMatch(url))
                    return 1 + Convert.ToInt32(regex.Item2);
            }
            return 0;
        }

        public static List<Blacklist> MatchingExpressions(string url)
        {
            List<Blacklist> matches = new List<Blacklist>();

            try
            {
                DatabaseEntities database = Controller.GetDatabase();
                var blacklistItems = from p in database.Blacklist select p;

                if (blacklistItems.Any())
                {
                    foreach (Blacklist item in blacklistItems)
                    {
                        if (new Regex(item.RegularExpression).IsMatch(url))
                            matches.Add(item);
                    }
                }
                database.Dispose();
            }
            catch (Exception ex)
            {
                LogsController.WriteException("Load blacklist", ex.Message);
            }

            return matches;
        }

        internal static Blacklist AddException(string exception, bool dontCacheOnServer, string title)
        {
            Blacklist item = null;
            _blacklist.Add(new Tuple<Regex, bool>(new Regex(exception, RegexOptions.Compiled), dontCacheOnServer));

            try
            {
                DatabaseEntities database = Controller.GetDatabase();

                item = new Blacklist()
                {
                    RegularExpression = exception,
                    CacheOnServer = dontCacheOnServer ? 0 : 1,
                    Title = title
                };
                database.Blacklist.AddObject(item);

                Controller.SubmitDatabaseChanges(database);
                database.Dispose();
            }
            catch (Exception ex)
            {
                LogsController.WriteException("Add exception", ex.Message);
            }
            return item;
        }

        internal static void DeleteException(int id)
        {
            DatabaseEntities database = Controller.GetDatabase();
            try
            {
                var exception = database.Blacklist.First(b =>
                            b.Id == id);

                foreach (var tuple in _blacklist.ToArray())
                {
                    if (tuple.Item1.ToString() == exception.RegularExpression)
                        _blacklist.Remove(tuple);
                }

                database.Blacklist.DeleteObject(exception);
                Controller.SubmitDatabaseChanges(database);
            }
            catch (Exception ex)
            {
                LogsController.WriteException("RemoveException()", ex.Message);
            }
            finally
            {
                database.Dispose();
            }
        }
    }
}
