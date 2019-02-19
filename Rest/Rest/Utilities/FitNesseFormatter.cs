// Copyright 2015-2019 Rik Essenius
//
//   Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file 
//   except in compliance with the License. You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software distributed under the License 
//   is distributed on an "AS IS" BASIS WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and limitations under the License.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text.RegularExpressions;

namespace Rest.Utilities
{
    internal static class FitNesseFormatter
    {
        public static string HeaderList(NameValueCollection headers) => HeaderListWithout(headers, new List<string>());

        public static string HeaderList(NameValueCollection headers, IEnumerable<string> filterHeaders) =>
            filterHeaders.Aggregate(string.Empty, (current, filter) => current + MultiLineHeader(filter, headers[filter]));

        public static string HeaderListWithout(NameValueCollection headers, List<string> headersToOmit)
        {
            var returnValue = string.Empty;
            for (var i = 0; i < headers.Count; i++)
            {
                if (!headersToOmit.Contains(headers.Keys[i], StringComparer.CurrentCultureIgnoreCase))
                {
                    returnValue += MultiLineHeader(headers.Keys[i], headers[i]);
                }
            }
            return returnValue;
        }

        private static string MultiLineHeader(string header, string value) => $"{header}: {value}\n";

        public static NameValueCollection ParseNameValueCollection(string input)
        {
            var collection = new NameValueCollection();
            var lines = input.SplitLines();
            foreach (
                var keyValuePair in
                lines.Select(line => line.ParseKeyValuePair()).Where(keyValuePair => keyValuePair.Key != null))
            {
                collection.Set(keyValuePair.Key, keyValuePair.Value);
            }
            return collection;
        }

        public static string ReplaceNewLines(string foreignInput, string replacement) => Regex.Replace(foreignInput, @"\r\n?|\n", replacement);
    }
}