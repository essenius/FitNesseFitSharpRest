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
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rest;
using Rest.ContentObjects;
using ContentHandler = Rest.ContentHandler;

namespace RestTests
{
    [TestClass]
    public class PropertiesForObjectTest
    {
        private static void AssertListEqualsArray(IReadOnlyCollection<object> myList, IReadOnlyList<string[]> expected)
        {
            var row = 0;
            Assert.AreEqual(expected.Count, myList.Count, "Number of rows wrong");
            foreach (var subListObj in myList)
            {
                var subList = subListObj as IReadOnlyList<object>;
                Assert.IsNotNull(subList);
                Assert.AreEqual(expected[row].Length, subList.Count, "Row " + row + " doesn't have the right size");
                var column = 0;
                foreach (var entry in subList)
                {
                    Assert.AreEqual(expected[row][column++], entry, "Value incorrect");
                }
                row++;
            }
        }

        [TestMethod, TestCategory("Unit")]
        public void PropertiesForObjectDecisionTest()
        {
            var h = new ContentHandler();
            var obj = h.ObjectFromTypeInAssemblyWithParams("xml", "RestTests.FewTypes", "RestCoreTests.dll", new[] {"true"});
            var a = new PropertiesForObject(obj);
            a.Reset();
            a.Execute();
            Assert.IsFalse(a.ValueWasSet, "Value was not set after reset+execute");
            Assert.IsNull(a.Value, "Value was null after reset+execute");
            a.Property = "/FewTypes/IntValue";
            a.Execute();
            Assert.IsFalse(a.ValueWasSet, "Value was not set after execute with property set");
            Assert.AreEqual("2147483647", a.Value, "Value was set correctly after setting property");
            Assert.AreEqual(typeof(string).ToString(), a.Type, "Type was set correctly after setting property");
            a.Value = "1";
            a.Execute();
            Assert.IsTrue(a.ValueWasSet, "Value was set after specifying Value");
            Assert.AreEqual("1", a.Value, "Value was correctly set");
        }

        [TestMethod, TestCategory("Unit")]
        public void PropertiesForObjectQueryTest()
        {
            var h = new ContentHandler();
            var obj = h.ObjectFromTypeInAssemblyWithParams("xml", "RestTests.FewTypes", "RestCoreTests.dll", new[] {"true"});
            var a = new PropertiesForObject(obj, "/FewTypes/StringValue");
            var expected = new[]
            {
                new[] {"Property", "/FewTypes[1]/StringValue[1]"},
                new[] {"Type", "System.String"},
                new[] {"Value", "string value"}
            };
            var myList = a.Query().First() as List<object>;
            //var row = 0;
            Assert.IsNotNull(myList);
            AssertListEqualsArray(myList, expected);
        }

        [TestMethod, TestCategory("Unit")]
        public void PropertiesForObjectTextTableTest()
        {
            var h = new ContentHandler();
            var obj = h.ObjectFrom("contains 21, 3000000000, 51.6 and no other true numbers");
            const string regex = "((\\b\\d+(\\.\\d+)?)|true|numbers)";
            var a = new PropertiesForObject(obj, regex);
            var multiPattern = string.Format(TextObject.MatchGroupPattern, regex, "{{0}}");
            var expected = new[]
            {
                new[] {"report:Property", "report:Type", "report:Value"},
                new[] {"report:" + string.Format(multiPattern, 1), "report:System.Int32", "report:21"},
                new[] {"report:" + string.Format(multiPattern, 2), "report:System.Int64", "report:3000000000"},
                new[] {"report:" + string.Format(multiPattern, 3), "report:System.Double", "report:51.6"},
                new[] {"report:" + string.Format(multiPattern, 4), "report:System.Boolean", "report:true"},
                new[] {"report:" + string.Format(multiPattern, 5), "report:System.String", "report:numbers"}
            };
            var myList = a.DoTable(new List<List<string>>());
            AssertListEqualsArray(myList, expected);
        }

        [TestMethod, TestCategory("Unit")]
        public void PropertiesForObjectXmlTableTest()
        {
            var h = new ContentHandler();
            var obj = h.ObjectFromTypeInAssemblyWithParams("xml", "RestTests.ManyTypes", "RestCoreTests.dll", new[] {"true"});
            var a = new PropertiesForObject(obj, "/ManyTypes/StringValue");
            var expected = new[]
            {
                new[] {"report:Property", "report:Type", "report:Value"},
                new[] {"report:/ManyTypes[1]/StringValue[1]", "report:System.String", "report:string value"}
            };
            var myList = a.DoTable(new List<List<string>>());
            AssertListEqualsArray(myList, expected);
        }
    }
}
