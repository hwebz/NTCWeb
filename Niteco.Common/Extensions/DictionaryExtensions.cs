using System;
using System.Collections;
using System.Collections.Generic;

namespace Niteco.Common.Extensions
{
    public static class DictionaryExtensions
    {
        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
        {
            return GetValueOrDefault<TKey, TValue, TValue>(dictionary, key, default(TValue));
        }

        public static TDefault GetValueOrDefault<TKey, TValue, TDefault>(this IDictionary<TKey, TValue> dictionary, TKey key, TDefault @default) where TValue : TDefault
        {
            TValue obj;
            if (!dictionary.TryGetValue(key, out obj))
                return @default;
            else
                return obj;
        }

        // a utility function for merging a 'target' dictionary into the 'source'
        public static void Merge<TKey, TValue>(this IDictionary<TKey, TValue> source, IDictionary<TKey, TValue> target)
        {
            if (target == null || source == null) return;

            foreach (var key in target.Keys)
            {
                if (source.ContainsKey(key))
                {
                    source[key] = target[key];
                }
                else
                {
                    source.Add(key, target[key]);
                }
            }
        }

        public static void Merge(this IDictionary source, IDictionary target)
        {
            if (target == null || source == null) return;

            foreach (var key in target.Keys)
            {
                if (source.Contains(key))
                {
                    source[key] = target[key];
                }
                else
                {
                    source.Add(key, target[key]);
                }
            }
        }

        public static TValue TryGetValue<TKey, TValue>(this IDictionary<TKey, TValue> source, TKey key)
        {
            var value = default(TValue);

            if (source.TryGetValue(key, out value))
            {
                return value;
            }

            return default(TValue);
        }

        public static bool ContainsValue<TKey, TValue>(this IDictionary<TKey, TValue> source, TKey key, string value)
        {
            return source.ContainsKey(key) &&
                source[key].ToString().Equals(value, StringComparison.CurrentCultureIgnoreCase);
        }

        public static bool ContainsValue(this IDictionary source, string key, string value)
        {
            return source.Contains(key) &&
                source[key].ToString().Equals(value, StringComparison.CurrentCultureIgnoreCase);
        }
    }
}
