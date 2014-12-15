using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using ClientAndServerShared;

namespace WPFProxy.Proxy
{
    public class ProxyServer : Helpers.Proxy.ProxyServer
    {
        public static readonly char[] spaceSplit = new char[] { ' ' };

        private TcpListener _listener;
        private Thread _listenerThread;

        protected static readonly ProxyServer _server = new ProxyServer();

        public static ProxyServer Server
        {
            get { return _server; }
        }

        public IPAddress ListeningIPInterface
        {
            get
            {
                IPAddress addr = IPAddress.Loopback;
                if (ConfigurationManager.AppSettings["ListeningIPInterface"] != null)
                    IPAddress.TryParse(ConfigurationManager.AppSettings["ListeningIPInterface"], out addr);

                return addr;
            }
        }

        public Int32 ListeningPort
        {
            get
            {
                Int32 port = 8081;
                if (ConfigurationManager.AppSettings["ListeningPort"] != null)
                    Int32.TryParse(ConfigurationManager.AppSettings["ListeningPort"], out port);

                return port;
            }
        }

        private ProxyServer()
        {
            _listener = new TcpListener(ListeningIPInterface, ListeningPort);
        }

        public bool Start()
        {
            try
            {
                _listener.Start();

            }
            catch (Exception ex)
            {
                LogsController.WriteException("Start()", ex.Message);
                return false;
            }

            _listenerThread = new Thread(new ParameterizedThreadStart(Listen));

            _listenerThread.Start(_listener);

            return true;
        }

        public void Stop()
        {
            _listener.Stop();

            //wait for server to finish processing current connections...

            _listenerThread.Abort();
            _listenerThread.Join();
            _listenerThread.Join();
        }

        private static void Listen(Object obj)
        {
            TcpListener listener = (TcpListener)obj;
            try
            {
                while (true)
                {
                    TcpClient client = listener.AcceptTcpClient();
                    while (!ThreadPool.QueueUserWorkItem(new WaitCallback(ProcessClient), client)) ;
                }
            }
            catch (ThreadAbortException) { }
            catch (SocketException) { }
        }

        private static void ProcessClient(Object obj)
        {
            TcpClient client = (TcpClient)obj;
            try
            {
                DoHttpProcessing(client);
            }
            catch (Exception ex)
            {
                LogsController.WriteException("ProcessClient()", ex.Message);
            }
            finally
            {
                client.Close();
            }
        }

        private static void DoHttpProcessing(TcpClient client)
        {
            Stream clientStream = client.GetStream();
            Stream outStream = clientStream; //use this stream for writing out - may change if we use ssl
            StreamReader clientStreamReader = new StreamReader(clientStream);

            try
            {
                //read the first line HTTP command
                String httpCmd = clientStreamReader.ReadLine();
                if (String.IsNullOrEmpty(httpCmd))
                {
                    clientStreamReader.Close();
                    clientStream.Close();
                    return;
                }
                //break up the line into three components
                String[] splitBuffer = httpCmd.Split(spaceSplit, 3);

                String method = splitBuffer[0];
                String remoteUri = splitBuffer[1];
                Version version = new Version(1, 0);

                HttpResponder responder = HttpLocalResponder.CreateIfMatches(remoteUri, method);
                if (responder == null)
                    responder = new HttpResponder(remoteUri, method);

                if (responder != null)
                    responder.Respond(clientStream, outStream, clientStreamReader);
            }
            catch (Exception ex)
            {
                LogsController.WriteException("DoHttpProcessing()", ex.Message);
            }
            finally
            {
                clientStreamReader.Close();
                clientStream.Close();
                outStream.Close();
            }

        }

        public static int GetContentLengthFromHeaders(Dictionary<string, string> requestHeaders)
        {
            int contentLen = 0;
            if (requestHeaders.ContainsKey("content-length"))
                int.TryParse(requestHeaders["content-length"], out contentLen);
            return contentLen;
        }

        public static Dictionary<string, string> ReadRequestHeaders(StreamReader sr)
        {
            String httpCmd;
            Dictionary<string, string> requestHeaders = new Dictionary<string, string>();

            if (sr == null) return requestHeaders;

            do
            {
                httpCmd = sr.ReadLine();
                if (String.IsNullOrEmpty(httpCmd))
                    return requestHeaders;

                String[] header = httpCmd.Split(colonSpaceSplit, 2, StringSplitOptions.None);
                requestHeaders.Add(header[0].ToLower(), header[1]);

            } while (!String.IsNullOrWhiteSpace(httpCmd));
            return requestHeaders;
        }

        public static void WriteResponseStatus(HttpStatusCode code, String description, StreamWriter myResponseWriter)
        {
            String s = String.Format("HTTP/1.0 {0} {1}", (Int32)code, description);
            myResponseWriter.WriteLine(s);
        }

        public static void WriteResponseHeaders(StreamWriter myResponseWriter, List<Tuple<String, String>> headers)
        {
            if (headers != null)
            {
                foreach (Tuple<String, String> header in headers)
                {
                    myResponseWriter.WriteLine(String.Format("{0}: {1}", header.Item1, header.Item2));
                }
            }
            myResponseWriter.WriteLine();
            myResponseWriter.Flush();
        }
    }
}
