// Copyright 2015-2021 Rik Essenius
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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rest;
using Rest.Model;

namespace RestTests
{
    [TestClass]
    public class RestConfigTest
    {
        [TestMethod]
        [TestCategory("Unit")]
        public void RestConfigTableTest()
        {
            var c = new RestConfig();
            var input = new List<List<string>>
            {
                new List<string> {"DefaultAccept", "application/json"},
                new List<string> {"DefaultContentType", "application/json"},
                new List<string> {"Headers", "header1:test1\r\nheader2:test2"},
                new List<string>
                {
                    "ContentTypeMapping",
                    "application/xml:XML\r\napplication/json:JSON\r\ntext/plain:TEXT\r\ndefault:JSON"
                },
                new List<string> {"Proxy", "System"},
                new List<string> {"Encoding", "iso-8859-1"},
                new List<string> {"DefaultUserAgent", "FitNesseRest"},
                new List<string> {"DefaultXmlNameSpaceKey", "atom"},
                new List<string> {"XmlValueTypeAttribute", string.Empty},
                new List<string> {"Timeout", "7.5"},
                new List<string> {"TrimWhitespace", "false"},
                new List<string> {"CookieDomain", "localhost"},
                new List<string> {"Cookies", "cookie1=value1\r\ncookie2=value2"},
                new List<string> {"SecurityProtocol", "Tls12"},
                // this must be the last one
                new List<string> {"NonExisting", "Bogus Value"}
            };

            var output = c.DoTable(input);
            Assert.AreEqual(input.Count, output.Count);
            for (var i = 0; i < output.Count - 1; i++)
            {
                var line = output[i] as List<string>;
                Assert.IsNotNull(line);
                Assert.AreEqual(string.Empty, line[0]);
                Assert.AreEqual("pass", line[1], "line " + i);
            }

            var lastLine = output[output.Count - 1] as List<string>;
            Assert.IsNotNull(lastLine);
            Assert.AreEqual(string.Empty, lastLine[1]);
        }

        [TestCleanup]
        [TestInitialize]
        public void TestInitializeAndCleanup() => Injector.CleanSessionContext();
    }
}
