using System;
using System.IO;
using ClientAndServerShared;

namespace WPFProxy.Proxy
{
    public class OutputStreamWriter
    {
        private Stream _browserStream = null;
        private ProxyEntry _cacheEntry = null;
        private Stream _outStream = null;
        private MemoryStream _memoryStream = null;
        private bool _isHTML = false;

        public OutputStreamWriter(ProxyEntry cacheEntry, Stream outStream)
        {
            _browserStream = outStream;
            _cacheEntry = cacheEntry;

            if (_cacheEntry.IsHTMLDocument())
            {
                _isHTML = true;
                _memoryStream = new MemoryStream();
                _outStream = _memoryStream;
            }
            else
            {
                _outStream = _browserStream;
                StreamWriter myResponseWriter = new StreamWriter(_browserStream);
                ProxyServer.WriteResponseStatus(_cacheEntry.StatusCode, _cacheEntry.StatusDescription, myResponseWriter);
                ProxyServer.WriteResponseHeaders(myResponseWriter, _cacheEntry.ResponseHeaders);
            }
        }

        public bool WriteToOutput(SharedProxy.Streams.Input.StreamItem streamItem, int readerId)
        {
            Stream stream = null;
            bool success = true;

            try
            {
                while ((stream = streamItem.GetStream(readerId)) != null)
                {
                    if (!WriteToOutput(stream))
                    {
                        success = false;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                LogsController.WriteException("WriteToOutput", ex.Message);
            }
            finally
            {
                Close();
            }
            return success;
        }

        private bool WriteToOutput(Stream readStream)
        {
            byte[] buffer = new byte[Helpers.Proxy.ProxyServer.BUFFER_SIZE];
            bool empty = true;
            int read;
            try
            {
                while ((read = readStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    _outStream.Write(buffer, 0, read);
                    empty = false;
                }
                _outStream.Flush();
            }
            catch (Exception ex)
            {
                LogsController.WriteException("Write to output", ex.Message);
            }
            return !empty;
        }

        private void Close()
        {
            if (_isHTML)
            {
                StreamWriter myResponseWriter = null;
                try
                {
                    byte[] bytes = _memoryStream.ToArray();

                    HtmlResponder cacheResponder = new HtmlResponder(bytes);// tu sa vklada iframe - nema to byt neskor?
                    cacheResponder.CacheEntry = _cacheEntry;

                    bytes = cacheResponder.GetResponseBytes();

                    myResponseWriter = new StreamWriter(_browserStream);
                    ProxyServer.WriteResponseStatus(_cacheEntry.StatusCode, _cacheEntry.StatusDescription, myResponseWriter);
                    ProxyServer.WriteResponseHeaders(myResponseWriter, cacheResponder.GetHeaders());

                    _browserStream.Write(bytes, 0, bytes.Length);
                }
                catch (Exception ex)
                {
                    LogsController.WriteException("Write response", ex.Message);
                }
                finally
                {
                    if (myResponseWriter != null)
                        myResponseWriter.Close();
                    _cacheEntry.WasOutput = true;
                }
            }

            _browserStream.Flush();
        }
    }
}
