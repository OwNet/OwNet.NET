using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WPFProxy.Cache;
using ClientAndServerShared;
using System.Threading;
using WPFProxy.Proxy;
using System.Data.SqlServerCe;
using System.Data;
using WPFProxy.Database;

namespace WPFProxy.Cache
{
    class CacheEntrySaver : SharedProxy.Cache.CacheEntrySaver
    {
        protected override void ExecuteSave(Dictionary<int, SaveItem> copyToSave)
        {
            if (copyToSave.Count == 0)
                return;

            var keys = copyToSave.Keys.ToList();

            DatabaseEntities context = null;
            try
            {
                context = Controller.GetDatabase();
                var caches = from c in context.Caches
                             where keys.Contains(c.ID)
                             select c;

                using (var connection = new SqlCeConnection(Controller.GetDatabaseConnectionString()))
                {
                    connection.Open();

                    var insertHeaderCommand = connection.CreateCommand();
                    insertHeaderCommand.CommandText = "INSERT INTO CacheHeaders ([Key], [Value], CacheId) " +
                        "VALUES (@Key, @Value, @CacheId)";
                    insertHeaderCommand.Parameters.Add("@Key", SqlDbType.NVarChar, 4000);
                    insertHeaderCommand.Parameters.Add("@Value", SqlDbType.NVarChar, 4000);
                    insertHeaderCommand.Parameters.Add("@CacheId", SqlDbType.Int, 4);

                    if (caches.Any())
                    {
                        // UPDATE
                        var deleteHeadersCommand = connection.CreateCommand();
                        deleteHeadersCommand.CommandText = "DELETE FROM CacheHeaders WHERE CacheId = @CacheId";
                        deleteHeadersCommand.Parameters.Add("@CacheId", SqlDbType.Int, 4);

                        var updateCommand = connection.CreateCommand();
                        updateCommand.CommandText = "UPDATE Caches SET " +
                            "AbsoluteUri = @AbsoluteUri, " +
                            "UserAgent = @UserAgent, " +
                            "Expires = @Expires, " +
                            "StatusCode = @StatusCode, " +
                            "StatusDescription = @StatusDescription, " +
                            "CharacterSet = @CharacterSet, " +
                            "ContentType = @ContentType, " +
                            "AccessCount = @AccessCount, " +
                            "AccessValue = @AccessValue, " +
                            "DateModified = @DateModified, " +
                            "Size = @Size, " +
                            "Parts = @Parts " +
                            "WHERE ID = @ID";
                        updateCommand.Parameters.Add("@ID", SqlDbType.Int, 4);
                        updateCommand.Parameters.Add("@AbsoluteUri", SqlDbType.NVarChar, 4000);
                        updateCommand.Parameters.Add("@UserAgent", SqlDbType.NVarChar, 400);
                        updateCommand.Parameters.Add("@Expires", SqlDbType.DateTime, 8);
                        updateCommand.Parameters.Add("@StatusCode", SqlDbType.Int, 4);
                        updateCommand.Parameters.Add("@StatusDescription", SqlDbType.NVarChar, 100);
                        updateCommand.Parameters.Add("@CharacterSet", SqlDbType.NVarChar, 100);
                        updateCommand.Parameters.Add("@ContentType", SqlDbType.NVarChar, 100);
                        updateCommand.Parameters.Add("@AccessCount", SqlDbType.Int, 4);
                        updateCommand.Parameters.Add("@AccessValue", SqlDbType.Float, 8);
                        updateCommand.Parameters.Add("@DateModified", SqlDbType.DateTime, 8);
                        updateCommand.Parameters.Add("@Size", SqlDbType.BigInt, 8);
                        updateCommand.Parameters.Add("@Parts", SqlDbType.Int, 4);

                        var increaseAccessCountCommand = connection.CreateCommand();
                        increaseAccessCountCommand.CommandText = "UPDATE Caches SET " +
                            "AccessCount = @AccessCount, " +
                            "AccessValue = @AccessValue " +
                            "WHERE ID = @ID";
                        increaseAccessCountCommand.Parameters.Add("@AccessCount", SqlDbType.Int, 4);
                        increaseAccessCountCommand.Parameters.Add("@AccessValue", SqlDbType.Float, 8);
                        increaseAccessCountCommand.Parameters.Add("@ID", SqlDbType.Int, 4);

                        foreach (var cache in caches)
                        {
                            SaveItem item = copyToSave[cache.ID];
                            if (item.Entry != null)
                            {
                                deleteHeadersCommand.Parameters["@CacheId"].Value = cache.ID;
                                deleteHeadersCommand.ExecuteNonQuery();

                                updateCommand.Parameters["@ID"].Value = cache.ID;
                                updateCommand.Parameters["@AbsoluteUri"].Value = item.Entry.AbsoluteUri;
                                updateCommand.Parameters["@Expires"].Value = GetDBValue(item.Entry.Expires);
                                updateCommand.Parameters["@UserAgent"].Value = item.Entry.UserAgent;
                                updateCommand.Parameters["@StatusCode"].Value = item.Entry.StatusCode;
                                updateCommand.Parameters["@StatusDescription"].Value = item.Entry.StatusDescription;
                                updateCommand.Parameters["@CharacterSet"].Value = GetDBValue(item.Entry.CharacterSet);
                                updateCommand.Parameters["@ContentType"].Value = item.Entry.ContentType;
                                updateCommand.Parameters["@DateModified"].Value = item.Entry.DateModified;
                                updateCommand.Parameters["@Size"].Value = item.Entry.Size;
                                updateCommand.Parameters["@Parts"].Value = item.Entry.NumFileParts;
                                if (item.IncreaseAccessCount > 0)
                                {
                                    updateCommand.Parameters["@AccessCount"].Value =
                                        (cache.AccessCount ?? 0) + item.IncreaseAccessCount;
                                    updateCommand.Parameters["@AccessValue"].Value =
                                        ProxyEntry.GetGDSFPriority(cache.AccessCount ?? 0, item.Size);
                                }
                                updateCommand.ExecuteNonQuery();

                                if (item.Entry.ResponseHeaders != null)
                                {
                                    foreach (Tuple<String, String> header in item.Entry.ResponseHeaders)
                                    {
                                        insertHeaderCommand.Parameters["@Key"].Value = header.Item1;
                                        insertHeaderCommand.Parameters["@Value"].Value = header.Item2;
                                        insertHeaderCommand.Parameters["@CacheId"].Value = cache.ID;
                                        insertHeaderCommand.ExecuteNonQuery();
                                    }
                                }
                            }
                            else
                            {
                                increaseAccessCountCommand.Parameters["@AccessCount"].Value =
                                    (cache.AccessCount ?? 0) + item.IncreaseAccessCount;
                                increaseAccessCountCommand.Parameters["@AccessValue"].Value =
                                    ProxyEntry.GetGDSFPriority(cache.AccessCount ?? 0, item.Size);
                                increaseAccessCountCommand.Parameters["@ID"].Value = cache.ID;
                                increaseAccessCountCommand.ExecuteNonQuery();
                            }
                            keys.Remove(cache.ID);
                        }

                        updateCommand.Dispose();
                        increaseAccessCountCommand.Dispose();
                    }

                    // INSERT
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "INSERT INTO Caches (ID, AbsoluteUri, UserAgent, Expires, StatusCode, " +
                            "DateStored, StatusDescription, CharacterSet, ContentType, AccessCount, AccessValue, " +
                            "DateModified, Size, Parts) VALUES (@ID, @AbsoluteUri, @UserAgent, @Expires, @StatusCode, " +
                            "@DateStored, @StatusDescription, @CharacterSet, @ContentType, @AccessCount, @AccessValue, " +
                            "@DateModified, @Size, @Parts)";
                        command.Parameters.Add("@ID", SqlDbType.Int, 4);
                        command.Parameters.Add("@AbsoluteUri", SqlDbType.NVarChar, 4000);
                        command.Parameters.Add("@UserAgent", SqlDbType.NVarChar, 400);
                        command.Parameters.Add("@Expires", SqlDbType.DateTime, 8);
                        command.Parameters.Add("@StatusCode", SqlDbType.Int, 4);
                        command.Parameters.Add("@DateStored", SqlDbType.DateTime, 8);
                        command.Parameters.Add("@StatusDescription", SqlDbType.NVarChar, 100);
                        command.Parameters.Add("@CharacterSet", SqlDbType.NVarChar, 100);
                        command.Parameters.Add("@ContentType", SqlDbType.NVarChar, 100);
                        command.Parameters.Add("@AccessCount", SqlDbType.Int, 4);
                        command.Parameters.Add("@AccessValue", SqlDbType.Float, 8);
                        command.Parameters.Add("@DateModified", SqlDbType.DateTime, 8);
                        command.Parameters.Add("@Size", SqlDbType.BigInt, 8);
                        command.Parameters.Add("@Parts", SqlDbType.Int, 4);

                        foreach (int id in keys)
                        {
                            SaveItem item = copyToSave[id];
                            if (item.Entry != null)
                            {
                                command.Parameters["@ID"].Value = item.Entry.ID;
                                command.Parameters["@AbsoluteUri"].Value = item.Entry.AbsoluteUri;
                                command.Parameters["@Expires"].Value = GetDBValue(item.Entry.Expires);
                                command.Parameters["@UserAgent"].Value = item.Entry.UserAgent;
                                command.Parameters["@StatusCode"].Value = item.Entry.StatusCode;
                                command.Parameters["@StatusDescription"].Value = item.Entry.StatusDescription;
                                command.Parameters["@CharacterSet"].Value = GetDBValue(item.Entry.CharacterSet);
                                command.Parameters["@ContentType"].Value = item.Entry.ContentType;
                                command.Parameters["@DateModified"].Value = item.Entry.DateModified;
                                command.Parameters["@DateStored"].Value = item.Entry.DateStored;
                                command.Parameters["@Size"].Value = item.Entry.Size;
                                command.Parameters["@Parts"].Value = item.Entry.NumFileParts;
                                if (item.IncreaseAccessCount > 0)
                                {
                                    command.Parameters["@AccessCount"].Value = item.IncreaseAccessCount;
                                    command.Parameters["@AccessValue"].Value = ProxyEntry.GetGDSFPriority(0, item.Size);
                                }
                                command.ExecuteNonQuery();

                                if (item.Entry.ResponseHeaders != null)
                                {
                                    foreach (Tuple<String, String> header in item.Entry.ResponseHeaders)
                                    {
                                        insertHeaderCommand.Parameters["@Key"].Value = header.Item1;
                                        insertHeaderCommand.Parameters["@Value"].Value = header.Item2;
                                        insertHeaderCommand.Parameters["@CacheId"].Value = item.Entry.ID;
                                        insertHeaderCommand.ExecuteNonQuery();
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogsController.WriteException("SaveNow", ex.Message);
            }
            finally
            {
                if (context != null)
                    context.Dispose();
            }
        }

        object GetDBValue(object value)
        {
            if (value == null)
                return DBNull.Value;
            return value;
        }
    }
}
