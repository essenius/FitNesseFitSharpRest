using System.Collections.Generic;
using System.Net.Http.Headers;

namespace Rest.Utilities
{
    public class HeaderDictionary : Dictionary<string, IEnumerable<string>>
    {
        public void AddHeaders(HttpHeaders headers)
        {
            foreach (var header in headers)
            {
                Add(header.Key, header.Value);
            }
        }
    }
}
