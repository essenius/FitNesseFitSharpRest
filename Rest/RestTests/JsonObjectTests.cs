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

using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rest.ContentObjects;

namespace RestTests
{
    [TestClass]
    public class JsonObjectTests
    {
        private const string JsonTest =
            "{ \"Id\": 0, \"SiteId\": 0, \"Name\": null, \"UserId\": 0, \"ReportTypeId\": 0, \"DurationValue\": 0, \"DurationPeriodId\": 25, \"CompletedByFilter\": null, \"CommentFilter\": null, \"AlternateDurationValue\": null, \"AlternateDurationPeriodId\": null, \"ExceptionsOnly\": false, \"ColorCoding\": null, \"ScheduleJobDefinitionId\": null, \"IsShared\": false, \"ScheduleRecipients\": null, \"ShowItemNames\": false, \"IsTrendDataSmoothed\": true, \"TrendSmoothingFactor\": 65536, \"TrendDataDensity\": 0, \"LastModifiedDate\": \"0001-01-01T00:00:00\", \"LastModifiedName\": \"John Doe\", \"Site\": null, \"Owner\": null, \"ReportType\": null, \"DurationPeriod\": null, \"AlternateDurationPeriod\": null, \"ActivityCompletionFlags\": [], \"ReportConfigurationGroupOptions\": [], \"ScheduleJobDefinition\": null }";

        private const string JsonTest2 =
            "{ \"Test\" : [ [ \"hi\", \"there\" ], [ \"hello\", \"too\" ] ], \"DateValue\": \"2015-01-01T00:00:00\" }";

        [TestMethod, TestCategory("Unit")]
        public void JsonObjectAddArrayTest()
        {
            var jsonObj = new JsonObject(JsonTest2);
            var objToAdd = new JsonObject("[ \"a\", 1 ]");
            Assert.IsTrue(jsonObj.AddAt(objToAdd, ""));
            Assert.AreEqual("[a, 1]", jsonObj.GetProperty("_"));
            Assert.AreEqual("Integer", jsonObj.GetPropertyType("_[1]"));
        }

        [TestMethod, TestCategory("Unit")]
        public void JsonObjectAddTest()
        {
            const string jsonStringToAdd = "{ \"NewProperty\" : 1 }";
            var jsonObj = new JsonObject(JsonTest);
            var initialLength = jsonObj.Serialize().Length;
            var objToAdd = new JsonObject(jsonStringToAdd);
            Assert.IsFalse(jsonObj.AddAt(objToAdd, @"nonexisting"), "Can't add at unknown location");
            Assert.IsTrue(jsonObj.AddAt(objToAdd, "$"), "Adding succeeded");
            var serializedObj = jsonObj.Serialize();
            Assert.IsTrue(serializedObj.Length > initialLength, "Length has changed");
            Assert.IsTrue(serializedObj.Contains("NewProperty"), "Object contains new property");
            Assert.AreEqual(initialLength + jsonStringToAdd.Length, serializedObj.Length, "Size OK");
            Debug.Print(jsonObj.Serialize());
        }

        [TestMethod, TestCategory("Unit")]
        public void JsonObjectDeleteArrayTest()
        {
            var jsonObj = new JsonObject(JsonTest2);
            Debug.Print(jsonObj.Serialize());
            Assert.IsNotNull(jsonObj.GetPropertyType("Test[1]"));
            Assert.IsTrue(jsonObj.Delete("Test[1]"));
            Assert.IsNull(jsonObj.GetPropertyType("Test[1]"));
            Debug.Print(jsonObj.Serialize());
            Assert.IsTrue(jsonObj.Delete("Test[0][1]"));
            Debug.Print(jsonObj.Serialize());
        }

        [TestMethod, TestCategory("Unit")]
        public void JsonObjectDeleteTest()
        {
            var jsonObj = new JsonObject(JsonTest);
            Assert.IsFalse(jsonObj.Delete(string.Empty), "Can't delete empty locator");
            Assert.IsFalse(jsonObj.Delete(@"nonexisting"), @"Can't delete nonexisting locator");
            Assert.IsTrue(jsonObj.Delete("CompletedByFilter"));
            Assert.IsNull(jsonObj.GetPropertyType("CompletedByFilter"));
        }

        [TestMethod, TestCategory("Unit")]
        public void JsonObjectEvaluatorTest()
        {
            const string source = "{\"ResponseCode\":\"0\", \"ResponseText\":\"3349\"}";
            var jsonObject = new JsonObject(source);
            Assert.AreEqual("0", jsonObject.Evaluate("ResponseCode"));
            Assert.AreEqual("3349", jsonObject.Evaluate("ResponseText"));
            Assert.AreEqual(null, jsonObject.Evaluate("NonExistingElement"));
        }

        [TestMethod, TestCategory("Unit")]
        public void JsonObjectGetPropertiesTest()
        {
            var jsonObj = new JsonObject(JsonTest);
            var props = jsonObj.GetProperties("");
            foreach (var entry in props)
            {
                Debug.Print(entry);
            }
        }

        [TestMethod, TestCategory("Unit")]
        public void JsonObjectSetPropertyTest()
        {
            var jsonObj = new JsonObject(JsonTest);
            Assert.IsFalse(jsonObj.SetProperty(string.Empty, null), "set empty property returns false");
            Assert.IsTrue(jsonObj.SetProperty("CompletedByFilter", "text value"), "Set valid property succeeds");
            Assert.AreEqual("String", jsonObj.GetPropertyType("CompletedByFilter"), "property type is correct");
        }
    }
}