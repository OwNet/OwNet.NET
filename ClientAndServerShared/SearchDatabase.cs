using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using HtmlAgilityPack;
using ServiceEntities;

namespace ClientAndServerShared
{
    public class SearchDatabase
    {
        private static Version _databaseVersion = new Version(0, 1, 2);
        protected static IVirtualSearchDatabase _virtualSearchDatabase = null;

        public static IVirtualSearchDatabase VirtualSearchDatabaseObject
        {
            set { _virtualSearchDatabase = value; }
        }

        public static string DatabaseVersionString { get { return _databaseVersion.ToString(3); } }

        public interface IVirtualSearchDatabase
        {
            HtmlDocument GetHtmlDocument(string url);
        }

        protected static string DatabasePath
        {
            get { return GetDatabasePath(DatabaseVersionString); }
        }

        protected static string ConnString
        {
            get { return String.Format("Data Source={0};New=False;Version=3", DatabasePath); }
        }

        private static string GetDatabasePath(string version)
        {
            return String.Format("{0}\\SearchDatabase_v{1}.db3", ClientAndServerController.AppSettings.AppDataFolder(), version);
        }

        public static void Init(string previousDatabaseVersion)
        {
            string dbPath = DatabasePath;
            if (!System.IO.File.Exists(dbPath))
            {
                try
                {
                    System.IO.Directory.CreateDirectory(ClientAndServerController.AppSettings.AppDataFolder());
                    System.IO.File.Copy("SearchDatabase.db3", dbPath);

                    if (previousDatabaseVersion != DatabaseVersionString
                        && previousDatabaseVersion != "")
                    {
                        System.IO.File.Delete(GetDatabasePath(previousDatabaseVersion));
                    }
                }
                catch (Exception ex)
                {
                    LogsController.WriteException("Init", ex.Message);
                }
            }
        }

        [SQLiteFunction(Name = "rank", Arguments = 1, FuncType = FunctionType.Scalar)]
        protected class FtsRank : SQLiteFunction
        {
            public override object Invoke(object[] args)
            {
                return Convert.ToString(args[0]).Count(f => f == ' ');
            }
        }

        protected static void InsertCacheText(SearchCache searchCache, SQLiteConnection sqlconn, bool update)
        {
            using (SQLiteCommand command = sqlconn.CreateCommand())
            {
                command.CommandType = CommandType.Text;
                if (update)
                    command.CommandText = "UPDATE SearchCaches SET Content = @Content, Title = @Title WHERE AbsoluteUri = @AbsoluteUri";
                else
                    command.CommandText = "INSERT INTO SearchCaches (AbsoluteUri, Content, Title) VALUES (@AbsoluteUri, @Content, @Title)";
                command.Parameters.Add("@AbsoluteUri", DbType.String).Value = searchCache.AbsoluteUri;
                command.Parameters.Add("@Content", DbType.String).Value = searchCache.Content;
                command.Parameters.Add("@Title", DbType.String).Value = searchCache.Title;

                command.ExecuteNonQuery();
            }
        }

        public static SearchResults Search(string key, int page)
        {
            int count;
            List<PageObjectWithContent> ret = SearchFrom(key, Helpers.Search.SkipBeforePage(page), out count);
            SearchResults results = null;
            if (ret != null)
            {
                results = new SearchResults()
                {
                    Results = ret
                };
                int totalPages, currentPage;
                Helpers.Search.ProcessPages(count, page, out totalPages, out currentPage);

                results.TotalPages = totalPages;
                results.CurrentPage = currentPage;
            }

            return results;
        }

        public static SearchResultsCollection SearchFrom(string key, int from, out int count)
        {
            SearchResultsCollection ret = new SearchResultsCollection();
            count = 0;

            try
            {
                //Connecting database
                SQLiteConnection sqlconn = new SQLiteConnection(ConnString);

                //Open the connection
                sqlconn.Open();

                //Query
                SQLiteCommand commandCount = sqlconn.CreateCommand();
                commandCount.CommandType = CommandType.Text;
                commandCount.CommandText = "SELECT COUNT(*) AS Total FROM SearchCaches WHERE SearchCaches MATCH @Key";
                commandCount.Parameters.Add("@Key", DbType.String).Value = key;

                object x = commandCount.ExecuteScalar();
                count = Convert.ToInt32(x);

                SQLiteCommand command = sqlconn.CreateCommand();
                command.CommandType = CommandType.Text;
                command.CommandText = "SELECT AbsoluteUri, Title, snippet(SearchCaches) AS Content, rank(offsets(SearchCaches)) AS Rank FROM SearchCaches WHERE SearchCaches MATCH @Key ORDER BY Rank DESC LIMIT @From, @To";
                command.Parameters.Add("@Key", DbType.String).Value = key;
                command.Parameters.Add("@From", DbType.Int32).Value = from;
                command.Parameters.Add("@To", DbType.Int32).Value = Helpers.Search.ItemsPerPage;

                //SQLiteDataAdapter to fill the DataSet
                SQLiteDataAdapter dataAdapter = new SQLiteDataAdapter(command);
                DataSet ds = new DataSet();
                dataAdapter.Fill(ds);

                //Get the collection of rows from the DataSet
                DataRowCollection dataRowCol = ds.Tables[0].Rows;

                PageObjectWithContent item;
                //Add the tables available in the DB to the combo box
                foreach (DataRow dr in dataRowCol)
                {
                    item = new PageObjectWithContent();
                    if (dr["Title"] != System.DBNull.Value)
                        item.Title = (string)dr["Title"];
                    if (dr["Content"] != System.DBNull.Value)
                        item.Content = (string)dr["Content"];
                    if (dr["AbsoluteUri"] != System.DBNull.Value)
                        item.AbsoluteURI = (string)dr["AbsoluteUri"];
                    if (dr["Rank"] != System.DBNull.Value)
                        item.Rank = Convert.ToInt32(dr["Rank"]);
                    ret.Add(item);
                }

                //Close the Connection
                sqlconn.Close();

            }
            catch (SQLiteException sqlex)
            {
                Console.WriteLine(sqlex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return ret;
        }

        public static void SaveFromCache(string url)
        {
            if (_virtualSearchDatabase == null)
                return;

            using (SQLiteConnection sqlconn = new SQLiteConnection(ConnString))
            {
                bool update = false;
                sqlconn.Open();

                using (SQLiteCommand command = new SQLiteCommand("SELECT COUNT(*) AS count FROM SearchCaches WHERE AbsoluteUri = @AbsoluteUri", sqlconn))
                {
                    command.Parameters.Add("@AbsoluteUri", DbType.String).Value = url;

                    object result = command.ExecuteScalar();
                    long count = (long)result;
                    if (count > 0)
                        update = true;
                }

                HtmlDocument htmlDoc = _virtualSearchDatabase.GetHtmlDocument(url);

                if (htmlDoc == null || htmlDoc.DocumentNode == null)
                    return;

                SearchCache searchCache = new SearchDatabase.SearchCache();
                HtmlNode titleNode = htmlDoc.DocumentNode.SelectSingleNode("/html/head/title");
                if (titleNode != null)
                    searchCache.Title = titleNode.InnerText;

                searchCache.Content = HtmlCleaner.ExtractText(htmlDoc);
                if (searchCache.Content == null)
                    return;

                searchCache.AbsoluteUri = url;
                InsertCacheText(searchCache, sqlconn, update);
            }
        }

        public class SearchCache
        {
            public string AbsoluteUri { get; set; }
            public string Content { get; set; }
            public string Title { get; set; }
        }

        protected class HtmlCleaner
        {
            static string[] whitelist = new string[] { "td", "tr", "br", "table", "thead", "tfoot", "tbody", "div", "span", "i", "u", "s", "b", "ul", "ol", "li" };
            static string[] blacklist = new string[] { "script", "embed", "object", "img", "#comment" };

            public static string ExtractText(string html)
            {
                HtmlDocument htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(html);

                return ExtractText(htmlDoc);
            }

            public static string ExtractText(HtmlDocument htmlDoc)
            {
                HtmlNode body = htmlDoc.DocumentNode.SelectSingleNode("//body");

                if (body != null)
                {
                    Cleanup(body, CleanupMode.Blacklist);
                    return System.Text.RegularExpressions.Regex.Replace(body.InnerText, @"\s+", " ");
                }

                return null;
            }

            private static void Cleanup(HtmlNode body, CleanupMode mode)
            {
                List<HtmlNode> nodesToBeRemoved = new List<HtmlNode>();
                foreach (var node in body.ChildNodes)
                {
                    if ((mode == CleanupMode.Whitelist && !whitelist.Contains(node.Name))
                        || (mode == CleanupMode.Blacklist && blacklist.Contains(node.Name)))
                    {
                        nodesToBeRemoved.Add(node);
                    }
                    else if (node.HasChildNodes)
                    {
                        Cleanup(node, mode);
                    }
                }
                for (int n = nodesToBeRemoved.Count - 1; n >= 0; --n)
                {
                    nodesToBeRemoved.ElementAt(n).ParentNode.RemoveChild(nodesToBeRemoved.ElementAt(n));
                }
            }

            public enum CleanupMode
            {
                Whitelist,
                Blacklist
            }
        }
    }
}
