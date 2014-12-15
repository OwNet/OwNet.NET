using System;
using System.IO;
using System.Text;

namespace Helpers
{
    public static class Common
    {
        public static int BytesInMB { get { return 1048576; } }
        public static string AppName { get { return "OwNet"; } }

        public static long GetDirectorySize(string p)
        {
            string[] a = Directory.GetFiles(p, "*.*");

            long b = 0;
            foreach (string name in a)
            {
                FileInfo info = new FileInfo(name);
                b += info.Length;
            }

            return b;
        }

        public static bool AreAllValidNumericChars(string str)
        {
            for (int i = 0; i < str.Length; i++)
                if (!Char.IsDigit(str[i]))
                    return false;

            return true;
        }

        public static string RandomString(int size, bool lowerCase)
        {
            StringBuilder builder = new StringBuilder();
            Random random = new Random();
            char ch;
            for (int i = 0; i < size; i++)
            {
                ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
                builder.Append(ch);
            }
            if (lowerCase)
                return builder.ToString().ToLower();
            return builder.ToString();
        }

        public static string FullDateTimeToString(DateTime dt)
        {
            return dt.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffffffK");
        }

        public static DateTime FullDateTimeFromString(string dtStr)
        {
            return DateTime.ParseExact(dtStr, "yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffffffK", null);
        }
    }
}
