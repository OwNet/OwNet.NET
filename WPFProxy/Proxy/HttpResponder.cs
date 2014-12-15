using System;
using System.IO;
using ClientAndServerShared;
using WPFProxy.Cache;

namespace WPFProxy.Proxy
{
    class HttpResponder
    {
        public String RemoteUri { get; set; }
        public String Method { get; set; }

        public HttpResponder(String remoteUri, String method)
        {
            RemoteUri = remoteUri;
            Method = method.ToUpper();
        }

        public static char[] ReadPostData(int contentLen, StreamReader clientStreamReader)
        {
            char[] postBuffer = new char[contentLen];
            int bytesRead = 0;
            int totalBytesRead = 0;

            try
            {
                if (contentLen > 0 && (bytesRead = clientStreamReader.ReadBlock(postBuffer, 0, contentLen)) > 0)
                    totalBytesRead += bytesRead;
            }
            catch (Exception ex)
            {
                LogsController.WriteException("ReadPostData", ex.Message);
            }

            return postBuffer;
        }

        public virtual void Respond(Stream clientStream, Stream outStream, StreamReader clientStreamReader)
        {
            if (Method.ToUpper() == "CONNECT") // SSL is not supported
            {
                clientStream.Close();
                outStream.Close();
                return;
            }

            try
            {
                if (RemoteUri.Contains("videoplayback"))
                    RemoteUri = RemoteUri;

                ProxyEntry entry = new ProxyEntry(RemoteUri, clientStreamReader, Method);
                entry.CanCacheHtml = !Controller.DoNotCacheHtml;

                int doNotCache = CachingExceptions.DoNotCache(entry);
                CachingExceptions.SetCanCacheOnEntry(entry);
                entry.Refresh = ProxyCache.RefreshAll;
                entry.UseServer = Controller.UseDataService;
                int downloadMethod = 0, readerId = -1;
                SharedProxy.Streams.Input.StreamItem streamItem = null;
                var AccessedAt = DateTime.Now;

                try
                {
                    streamItem = SharedProxy.Streams.ProxyStreamManager.CreateStreamItem(entry, out downloadMethod, out readerId);
                    if (streamItem != null)
                    {
                        OutputStreamWriter writer = new OutputStreamWriter(entry, outStream);
                        writer.WriteToOutput(streamItem, readerId);
                    }
                }
                catch (Exception ex)
                {
                    LogsController.WriteException("Respond", ex.Message);
                }
                finally
                {
                    if (streamItem != null)
                        streamItem.DisposeReader(readerId);

                    TimeSpan ts = DateTime.Now - AccessedAt;
                    double duration = ts.TotalSeconds;

                    lock (CacheReporter.NotReportedItems)
                    {
                        CacheReporter.NotReportedItems.Add(new ServiceEntities.Cache.CacheLog()
                        {
                            AbsoluteUri = entry.AbsoluteUri,
                            FetchDuration = duration,
                            AccessedAt = AccessedAt,
                            DownloadedFrom = downloadMethod,
                            Type = (int)entry.Type
                        });
                    }
                }

                if (!entry.WasOutput && entry.IsOffline && entry.AcceptsHtml())
                    HttpLocalResponder.OutputOfflineError(clientStream, outStream, clientStreamReader,
                        entry.AbsoluteUri, entry.WebRequest.Referer);
            }
            catch (Exception ex)
            {
                LogsController.WriteException("respond() [outer2]", ex.Message);
            }
        }
    }
}
