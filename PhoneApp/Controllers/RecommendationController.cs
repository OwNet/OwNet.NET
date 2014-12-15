using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;

namespace PhoneApp.Controllers
{
    public class RecommendationController : Helpers.NotifierObject
    {
        Database.Recommendation _recommendation = null;
        IsolatedStorageFile _isoFile = null;
        string _filePath = "";

        public RecommendationController(Database.Recommendation recommendation)
        {
            _recommendation = recommendation;

            _isoFile = IsolatedStorageFile.GetUserStoreForApplication();
            if (!_isoFile.DirectoryExists("Recommendations"))
                _isoFile.CreateDirectory("Recommendations");
            _filePath = GetRecommendationFilePath(_recommendation.AbsoluteUri);
        }

        internal static string GetRecommendationFilePath(string absoluteUri)
        {
            return string.Format("Recommendations\\{0}.recommendation.html", absoluteUri.GetHashCode());
        }

        public void GetWebContent()
        {
            if (_isoFile.FileExists(_filePath))
            {
                NotifySuccess();
                return;
                //_isoFile.DeleteFile(_filePath);
            }
            try
            {
                var client = new PhoneAppCentralService.PhoneAppServiceClient();
                client.GetWebsiteContentAsync(_recommendation.AbsoluteUri);
                client.GetWebsiteContentCompleted += new EventHandler<PhoneAppCentralService.GetWebsiteContentCompletedEventArgs>(GetWebsiteContent);
            }
            catch (Exception ex)
            {
                LogsController.WriteException("LoadGroups", ex.Message);
            }
        }

        void GetWebsiteContent(object sender, PhoneAppCentralService.GetWebsiteContentCompletedEventArgs e)
        {
            try
            {
                var content = e.Result;
                if (!content.Success)
                    return;

                StreamWriter sw = new StreamWriter(new IsolatedStorageFileStream(_filePath, FileMode.Append, _isoFile));
                sw.WriteLine(content.Content);
                sw.Close();
            }
            catch (Exception ex)
            {
                LogsController.WriteException("SaveRecommendationContent", ex.Message);
            }

            NotifySuccess();
        }
    }
}
