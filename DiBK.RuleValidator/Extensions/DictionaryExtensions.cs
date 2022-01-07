using System.Collections.Generic;
using System.Linq;

namespace DiBK.RuleValidator
{
    public static class DictionaryExtensions
    {
        public static void Append<T, U>(this Dictionary<T, U> dict1, Dictionary<T, U> dict2)
        {
            foreach (var (key, value) in dict2)
            {
                if (!dict1.ContainsKey(key))
                    dict1.Add(key, value);
                else
                    dict1[key] = value;
            }
        }

        public static Dictionary<T, U> Merge<T, U>(this Dictionary<T, U> dict1, Dictionary<T, U> dict2)
        {
            return dict1
                .Concat(dict2)
                .ToLookup(kvp => kvp.Key, kvp => kvp.Value)
                .ToDictionary(kvp => kvp.Key, grp => grp.First());
        }
    }
}
