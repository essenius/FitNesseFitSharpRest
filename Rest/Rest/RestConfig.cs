// Copyright 2015-2020 Rik Essenius
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
using System.Linq;
using Rest.Model;

namespace Rest
{
    /// <summary>Fixture for the Rest Config table table, specifying the configuration data for the Rest Tester</summary>
    public class RestConfig
    {
        private readonly SessionContext _context;

        /// <summary>Configuration for the Rest Tester</summary>
        /// <guarantees>Session context has been set</guarantees>
        public RestConfig()
        {
            Injector.CleanSessionContext();
            _context = Injector.InjectSessionContext();
        }

        /// <summary>
        ///     Process configuration entries: DefaultAccept, DefaultContentType, Encoding, Proxy, DefaultUserAgent,
        ///     DefaultXmlNameSpaceKey,
        ///     XmlValueTypeAttribute, Headers, ContentTypeMapping, Timeout
        /// </summary>
        /// <param name="table">rows of at least 2 cells, as FitNesse provides when executing a Table Table</param>
        /// <returns>
        ///     list of the same size with empty values, and 'pass' in the second column where the related configuration was
        ///     set
        /// </returns>
        public List<object> DoTable(List<List<string>> table) => table.Select(ProcessLine).Cast<object>().ToList();

        /// <summary>Processes a line in the configuration and sets context accordingly</summary>
        /// <param name="line">list of at least two values (parameter name and parameter value</param>
        /// <returns>list of two values: one empty string and 'pass' if the config could be set, and empty string otherwise</returns>
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
