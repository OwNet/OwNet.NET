using System;
using System.Threading;

namespace Prefetching
{
    public class BrowserWorkerEventArgs : EventArgs
    {
        public string Uri { get; set; }
        public string Message { get; set; }
    }
    public class BrowserWorker
    {
        
        public int WaitDownloadComplete { get; set; }
        public int BrowserTimeout { get; set; }
        public string Proxy { get; set; }
        public BrowserWorker()
            : this(5 * 1000, 2 * 60 * 1000)
        {
        }



        public BrowserWorker(int waitDownloadComplete, int browserTimeout)
        {
            WaitDownloadComplete = waitDownloadComplete;
            BrowserTimeout = browserTimeout;
        }

        
        public delegate void CompletedEventHandler(object sender, BrowserWorkerEventArgs e);
        public delegate void TimedOutEventHandler(object sender, BrowserWorkerEventArgs e);
        public delegate void CancelledEventHandler(object sender, BrowserWorkerEventArgs e);
        
        public event CompletedEventHandler Completed = null;
        public event TimedOutEventHandler TimedOut = null;
        public event CancelledEventHandler Cancelled = null;

        protected void OnCompleted(BrowserWorkerEventArgs e)
        {
            if (Completed != null)
            {
                Completed(this, e);
            }
        }

        protected void OnTimeout(BrowserWorkerEventArgs e)
        {
            if (TimedOut != null)
            {
                TimedOut(this, e);
            }
        }

        protected void OnCancelled(BrowserWorkerEventArgs e)
        {
            if (Cancelled != null)
            {
                Cancelled(this, e);
            }
        }

        public bool Start(string uri)
        {
            return Start(uri, uri);
        }

        public bool Start(string startUri, string endUri)
        {
            try
            {
                Uri startUrl = new Uri(startUri);
                Uri endUrl = new Uri(endUri);
                
                Thread th = new Thread(() =>
                {
                    try
                    {
                        Helpers.AudioMixerHelper.SetVolume(0);
                        System.Windows.Forms.WebBrowser br = new System.Windows.Forms.WebBrowser();
                        br.ScriptErrorsSuppressed = true;
                        System.Timers.Timer _timerCompleteDownload = new System.Timers.Timer(WaitDownloadComplete); // 5 seconds timeout after page loaded
                        System.Timers.Timer _timerBrowserTimeout = new System.Timers.Timer(BrowserTimeout); // 2 minutes timeout
                        _timerBrowserTimeout.AutoReset = false;
                        _timerBrowserTimeout.Enabled = false;
                        _timerCompleteDownload.AutoReset = false;
                        _timerCompleteDownload.Enabled = false;

                        _timerBrowserTimeout.Elapsed +=
                            new System.Timers.ElapsedEventHandler(delegate(object sender, System.Timers.ElapsedEventArgs e)
                        {
                            try
                            {
                                OnTimeout(new BrowserWorkerEventArgs() { Message = "Timeout", Uri = endUri });
                                br.Navigate("about:blank");     // redirect out of the page
                                br = null;
                            }
                            catch (Exception) { }
                            System.Windows.Forms.Application.ExitThread();
                        });


                        // blocks alerts
                        br.Navigated +=
                            new System.Windows.Forms.WebBrowserNavigatedEventHandler(
                                delegate(object sender, System.Windows.Forms.WebBrowserNavigatedEventArgs e)
                        {
                            try
                            {
                                Action<System.Windows.Forms.HtmlDocument> blockAlerts = (System.Windows.Forms.HtmlDocument d) =>
                                {
                                    System.Windows.Forms.HtmlElement h = d.GetElementsByTagName("head")[0];
                                    System.Windows.Forms.HtmlElement s = d.CreateElement("script");
                                    (s.DomElement as mshtml.IHTMLScriptElement).text = "window.alert=function(){};";
                                    h.AppendChild(s);
                                };
                                System.Windows.Forms.WebBrowser b = sender as System.Windows.Forms.WebBrowser;
                                blockAlerts(b.Document);
                                string myrul = b.Document.Url.AbsoluteUri;
                                for (int i = 0; i < b.Document.Window.Frames.Count; i++)
                                    try { blockAlerts(b.Document.Window.Frames[i].Document); }
                                    catch (Exception) { };
                            }
                            catch (Exception)
                            {
                            }
                        });

                        
                        br.DocumentCompleted += 
                            new System.Windows.Forms.WebBrowserDocumentCompletedEventHandler(
                                delegate(object sender, System.Windows.Forms.WebBrowserDocumentCompletedEventArgs e)
                        {
                            ((System.Windows.Forms.WebBrowser)sender).Document.Window.Error +=
                                new System.Windows.Forms.HtmlElementErrorEventHandler(
                                    delegate(object senderxx, System.Windows.Forms.HtmlElementErrorEventArgs f)
                            {
                                // Ignore the error and suppress the error dialog box. 
                                f.Handled = true;
                            });
                            try
                            {
                                if (endUrl.Equals(e.Url))
                                {
                                    _timerBrowserTimeout.Stop();
                                    _timerCompleteDownload.Elapsed += 
                                        new System.Timers.ElapsedEventHandler(
                                            delegate(object sender2, System.Timers.ElapsedEventArgs f)
                                    {
                                        try
                                        {
                                            _timerCompleteDownload.Stop();
                                            OnCompleted(new BrowserWorkerEventArgs() { Message = "Complete", Uri = endUri });
                                            br.Navigate("about:blank");     // redirect out of the page
                                            br = null;
                                            System.Windows.Forms.Application.ExitThread();   // Stops the thread
                                        }
                                        catch (Exception) { }
                                    });
                                    _timerCompleteDownload.Start();
                                }
                            }
                            catch (Exception)
                            {
                            }
                        });

                        br.Navigate(startUrl.AbsoluteUri);
                        _timerBrowserTimeout.Start();
                        System.Windows.Forms.Application.Run();
                    }
                    catch (Exception e)
                    {
                        OnCancelled(new BrowserWorkerEventArgs() { Message = e.Message, Uri = endUri });
                        System.Windows.Forms.Application.ExitThread();
                    }
                });
                th.SetApartmentState(ApartmentState.STA);
                
                th.Start();
                return true;
            }
            catch (Exception)
            {

            }
            return false;
        }


        public bool Start(string startUri, string endUri, string proxyAddress, string proxyPort)
        {
            ProxySettings.WinINet.SetConnectionProxy(false, proxyAddress + ":" + proxyPort);
            return Start(startUri, endUri);
        }
        public bool Start(string uri, string proxyAddress, string proxyPort)
        {
            return Start(uri, uri, proxyAddress, proxyPort);
        }
    }
}
