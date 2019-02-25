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

using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Rest.Model;

namespace Rest
{
    [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Used by FitSharp")]
    public class RestConfig
    {
        private readonly SessionContext _context;

        [Documentation("Configuration for the Rest Tester")]
        public RestConfig() => _context = Injector.InjectSessionContext();

        [SuppressMessage("ReSharper", "ParameterTypeCanBeEnumerable.Global", Justification = "FitSharp doesn't handle that")]
        [Documentation("Process configuration entries: DefaultAccept, DefaultContentType, Encoding, Proxy, DefaultUserAgent, DefaultXmlNameSpaceKey, " +
                       "XmlValueTypeAttribute, Headers, ContentTypeMapping, Timeout")]
        public List<object> DoTable(List<List<string>> table) => table.Select(ProcessLine).Cast<object>().ToList();

        private List<string> ProcessLine(List<string> line)
        {
            Debug.Assert(line.Count >= 2);
            var outLine = new List<string>();
            var ok = _context.SetConfig(line[0], line[1]);
            outLine.Add(string.Empty);
            outLine.Add(ok ? "pass" : string.Empty);
            return outLine;
        }
    }
}