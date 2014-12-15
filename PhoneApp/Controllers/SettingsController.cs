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
using System.Linq;

namespace PhoneApp.Controllers
{
    public class SettingsController
    {
        public static DateTime GetDateTimeSetting(string settingName)
        {
            string value = GetValue(settingName);
            if (value == "")
                return new DateTime(1970, 1, 1);

            return DateTime.Parse(value);
        }

        public static string GetValue(string settingName)
        {
            try
            {
                using (var context = new Database.DatabaseContext(Database.DatabaseContext.DBConnectionString))
                {
                    var settings = context.Settings.Where(s => s.Key == settingName);
                    if (settings.Any())
                        return settings.First().Value;
                }
            }
            catch (Exception ex)
            {
                LogsController.WriteException(settingName, ex.Message);
            }

            return "";
        }

        public static void SetDateTimeSetting(string settingName, DateTime value)
        {
            SetValue(settingName, value.ToString());
        }

        public static void SetValue(string key, string value)
        {
            try
            {
                using (var context = new Database.DatabaseContext(Database.DatabaseContext.DBConnectionString))
                {
                    var settings = context.Settings.Where(s => s.Key == key);
                    if (settings.Any())
                    {
                        var setting = settings.First();
                        setting.Value = value;
                    }
                    else
                    {
                        context.Settings.InsertOnSubmit(new Database.Setting()
                        {
                            Key = key,
                            Value = value
                        });
                    }
                    context.SubmitChanges();
                }
            }
            catch (Exception ex)
            {
                LogsController.WriteException(key, ex.Message);
            }
        }
    }
}
