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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rest.Utilities;

namespace RestTests
{
    [TestClass]
    public class ExtensionFunctionsTests
    {
        [TestMethod]
        [TestCategory("Unit")]
        public void ExtensionFunctionsCastToInferredTypeTest()
        {
            Assert.AreEqual(5, "5".CastToInferredType(), "Int Cast");
            const long longValue = int.MaxValue + 1L;
            Assert.AreEqual(longValue, longValue.ToString().CastToInferredType(), "Long Cast");
            Assert.AreEqual(-194.770, "-194.770".CastToInferredType(), "Double Cast");
            Assert.AreEqual(true, "true".CastToInferredType(), "Bool Cast");
            Assert.AreEqual("string", "string".CastToInferredType(), "String Cast");
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void ExtensionFunctionsIsLikeTest()
        {
            Assert.IsFalse("abc".IsLike("bc"));
            Assert.IsTrue("abc".IsLike("a?c"));
            Assert.IsTrue("abc".IsLike("a*"));
        }

        [DataTestMethod]
        [TestCategory("Unit")]
        [DataRow("  abc  ", "", "abc")]
        [DataRow("abc:def", "abc", "def")]
        [DataRow("abc : def", "abc", "def")]
        [DataRow("abc:def:ghi", "abc", "def:ghi")]
        [DataRow("abc : def:ghi", "abc", "def:ghi")]
        [DataRow("abc : def : ghi", "abc", "def : ghi")]
        [DataRow(":abc", "", "abc")]
        [DataRow("abc:", "abc", "")]
        [DataRow(":", "", "")]
        [DataRow(":abc", "", "abc")]
        [DataRow("", "", "")]
        public void ExtensionFunctionsParseKeyValuePairTest(string input, string expectedMethod, string expectedLocator)
        {
            var kvp = input.ParseKeyValuePair();
            Assert.AreEqual(expectedMethod, kvp.Key, "Method OK");
            Assert.AreEqual(expectedLocator, kvp.Value, "Locator OK");
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void ExtensionFunctionsStripAfterTest()
        {
            Assert.AreEqual("12", "12:34".StripAfter(":"));
            Assert.AreEqual("12", "12".StripAfter(":"));
        }
    }
}
