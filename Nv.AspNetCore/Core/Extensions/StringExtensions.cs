using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nv.AspNetCore.Core.Extensions
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
    }
}
