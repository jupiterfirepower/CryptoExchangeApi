using System.Linq;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Common.Contracts
{
    public static class NameValueCollectionExtentions
    {
        public static IEnumerable<KeyValuePair<string, string>> AsKeyValuePair(this NameValueCollection source)
        {
            return source.AllKeys.SelectMany(
                source.GetValues,
                (k, v) => new KeyValuePair<string, string>(k, v));
        }
    }
}
