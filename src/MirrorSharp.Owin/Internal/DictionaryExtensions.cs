using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MirrorSharp.Owin.Internal {
    internal static class DictionaryExtensions {
        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key) {
            TValue value;
            return dictionary.TryGetValue(key, out value) ? value : default(TValue);
        }
    }
}
