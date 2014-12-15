using System;
using System.Collections.Generic;
using System.Text;

namespace Helpers
{
    public class ConvertDictionary
    {
        static Dictionary<string, int> _dictionary = new Dictionary<string, int>()
        {
	    {"salmon", 5},
	    {"tuna", 6},
	    {"clam", 2},
	    {"asparagus", 3}
        };

        public static string GetString(Dictionary<string, int> d)
        {
            // Build up each line one-by-one and them trim the end
            StringBuilder builder = new StringBuilder();
            foreach (KeyValuePair<string, int> pair in d)
            {
                builder.Append(pair.Key).Append(":").Append(pair.Value).Append(',');
            }
            string result = builder.ToString();
            // Remove the final delimiter
            result = result.TrimEnd(',');
            return result;
        }

        public static Dictionary<string, int> GetDict(string s)
        {
            Dictionary<string, int> d = new Dictionary<string, int>();
            // Divide all pairs (remove empty strings)
            string[] tokens = s.Split(new char[] { ':', ',' },
                StringSplitOptions.RemoveEmptyEntries);
            // Walk through each item
            for (int i = 0; i < tokens.Length; i += 2)
            {
                string name = tokens[i];
                string freq = tokens[i + 1];

                // Parse the int (this can throw)
                int count = int.Parse(freq);
                // Fill the value in the sorted dictionary
                if (d.ContainsKey(name))
                {
                    d[name] += count;
                }
                else
                {
                    d.Add(name, count);
                }
            }
            return d;
        }
    }
}
