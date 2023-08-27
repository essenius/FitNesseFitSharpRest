// Copyright 2015-2023 Rik Essenius
//
//   Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file 
//   except in compliance with the License. You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software distributed under the License 
//   is distributed on an "AS IS" BASIS WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and limitations under the License.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rest.ContentObjects;

namespace RestTests
{
    [TestClass]
    public class ContentObjectTest
    {
        [TestMethod]
        [TestCategory("Unit")]
        public void ContentObjectJsonParse()
        {
            var c = ContentObject.Parse("{}");
            Assert.AreEqual("JSON Object", c.ToString());
            Assert.AreEqual("{}", c.Serialize());
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void ContentObjectTextParse()
        {
            var c = ContentObject.Parse("abc");
            Assert.AreEqual("Text Object", c.ToString());
            Assert.AreEqual("abc", c.Serialize());
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void ContentObjectUnknownParse()
        {
            var c = ContentObject.Parse("abc" + (char)2);
            Assert.AreEqual("Binary Object", c.ToString());
            Assert.AreEqual("abc\u0002", c.Serialize());
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void ContentObjectXmlParse()
        {
            var c = ContentObject.Parse("<a>test</a>");
            Assert.AreEqual("XML Object", c.ToString());
            Assert.AreEqual("<a>test</a>", c.Serialize());
        }
    }
}