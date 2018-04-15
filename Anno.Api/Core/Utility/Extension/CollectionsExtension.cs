using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anno.Api.Core.Utility.Extension
{
    public static class CollectionsExtension
    {
        //Generic List

        public static IEnumerable<T> TakeLast<T>(this IEnumerable<T> source, int count)
        {
            return source.Skip(Math.Max(0, source.Count() - count));
        }

        //Dictionary

        public static string GetValue(this Dictionary<string, string> input, string key)
        {
            if (input.ContainsKey(key))
                return input[key];
            else
                return null;
        }
    }
}
