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

using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rest.Utilities;

namespace RestTests
{
    [TestClass]
    public class ExtensionFunctionsTests
    {
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global", Justification = "false positive"),
         SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global", Justification = "false positive")]
        public TestContext TestContext { get; set; }

        [TestMethod, TestCategory("Unit")]
        public void ExtensionFunctionsCastToInferredTypeTest()
        {
            Assert.AreEqual(5, "5".CastToInferredType(), "Int Cast");
            const long longValue = int.MaxValue + 1L;
            Assert.AreEqual(longValue, longValue.ToString().CastToInferredType(), "Long Cast");
            Assert.AreEqual(-194.770, "-194.770".CastToInferredType(), "Double Cast");
            Assert.AreEqual(true, "true".CastToInferredType(), "Bool Cast");
            Assert.AreEqual("string", "string".CastToInferredType(), "String Cast");
        }

        [TestMethod, TestCategory("Unit")]
        public void ExtensionFunctionsIsLikeTest()
        {
            Assert.IsFalse("abc".IsLike("bc"));
            Assert.IsTrue("abc".IsLike("a?c"));
            Assert.IsTrue("abc".IsLike("a*"));
        }

        [TestMethod, TestCategory("Unit"), DataSource(@"Microsoft.VisualStudio.TestTools.DataSource.XML", "|DataDirectory|\\TestData.xml",
             "MatchParser", DataAccessMethod.Sequential), DeploymentItem("RestTests\\TestData.xml")]
        public void ExtensionFunctionsParseKeyValuePairTest()
        {
            var input = TestContext.DataRow["input"].ToString();
            var expectedMethod = TestContext.DataRow["expectedMethod"].ToString();
            var expectedLocator = TestContext.DataRow["expectedLocator"].ToString();
            var kvp = input.ParseKeyValuePair();
            Assert.AreEqual(expectedMethod, kvp.Key, "Method OK");
            Assert.AreEqual(expectedLocator, kvp.Value, "Locator OK");
        }

        [TestMethod, TestCategory("Unit")]
        public void ExtensionFunctionsStripAfterTest()
        {
            Assert.AreEqual("12", "12:34".StripAfter(":"));
            Assert.AreEqual("12", "12".StripAfter(":"));
        }
    }
}