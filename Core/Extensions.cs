using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExtensionMethods
{
    public static class Extensions
    {
        public static object RandomValue<TKey, TValue>(this Dictionary<TKey, TValue> dict)
        {
            Random rand = new Random((int)DateTime.Now.Ticks);
            List<TValue> values = Enumerable.ToList(dict.Values);
            return values[rand.Next(values.Count)];
        }
    }
}
