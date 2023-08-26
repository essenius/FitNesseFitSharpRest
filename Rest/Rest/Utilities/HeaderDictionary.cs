// Copyright 2023 Rik Essenius
//
//   Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file 
//   except in compliance with the License. You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software distributed under the License 
//   is distributed on an "AS IS" BASIS WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and limitations under the License.

using System.Collections.Generic;
using System.Net.Http.Headers;

namespace Rest.Utilities
{
    public class HeaderDictionary : Dictionary<string, IEnumerable<string>>
    {
        public void AddHeaders(HttpHeaders headers)
        {
            if (headers == null) return;
            foreach (var header in headers)
            {
                Add(header.Key, header.Value);
            }
        }
    }
}
