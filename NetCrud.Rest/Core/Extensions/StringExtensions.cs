using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetCrud.Rest.Core.Extensions
{
    public static class StringExtensions
    {
        public static string UppercaseFirst(this string s)
        {
            // Check for empty string.
            if (string.IsNullOrEmpty(s))
            {
                return string.Empty;
            }
            string[] parts = s.Split(".").Select(x => char.ToUpper(x[0]) + x.Substring(1)).ToArray();
            // Return char and concat substring.
   
            return string.Join('.', parts);
        }

        public static string[] GetFields(this string s)
        {
            var lst = new List<string>();
            int openBrackets = 0;
            int lastIndex = -1;
            for (int i = 0; i < s.Length; i++)
            {
                if(s[i] == ',' && openBrackets == 0 && i != lastIndex)
                {
                    string item = s.Substring(lastIndex + 1, i - lastIndex - 1);
                    if(!string.IsNullOrWhiteSpace(item)) lst.Add(item.Replace(" ",""));
                    lastIndex = i;
                }
                else if(s[i] == '[')
                {
                    openBrackets++;
                }
                else if (s[i] == ']')
                {
                    openBrackets--;
                }
            }
            if (openBrackets != 0) throw new Exception("Extra Open Brackets Error in Field Property!");

            string last = s.Substring(lastIndex + 1);
            if (!string.IsNullOrWhiteSpace(last)) lst.Add(last.Replace(" ", ""));

            return lst.ToArray();
        } 
    }
}
