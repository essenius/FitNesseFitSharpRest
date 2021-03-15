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

using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rest.ContentObjects;

namespace RestTests
{
    [TestClass]
    public class JsonObjectTests
    {
        private const string JsonTest =
            "{ \"Id\": 0, \"Name\": null, \"IsShared\": false, \"LastModifiedDate\": \"0001-01-01T00:00:00\", \"LastModifiedName\": \"John Doe\", \"Flags\": [] }";

        private const string JsonTest2 =
            "{ \"Test\" : [ [ \"hi\", \"there\" ], [ \"hello\", \"  too\" ] ], \"DateValue\": \"2015-01-01T00:00:00\", \"Tree\": { \"one\": 1, \"two\": 2 } }";

        [TestMethod, TestCategory("Unit")]
        public void JsonObjectAddArrayTest()
        {
            var jsonObj = new JsonObject(JsonTest2);
            Assert.AreEqual("  too", jsonObj.GetProperty("Test[1][1]"), "Too doesn't get trimmed if flag is off");
            var objToAdd = new JsonObject("[ \"a\", 1 ]");
            Assert.IsTrue(jsonObj.AddAt(objToAdd, ""));
            Assert.AreEqual("[\"a\",1]", jsonObj.GetProperty("_"));
            Assert.AreEqual("Integer", jsonObj.GetPropertyType("_[1]"));
        }

        [TestMethod, TestCategory("Unit")]
        public void JsonObjectAddTest()
        {
            const string jsonStringToAdd = "{\"NewProperty\":1}";
            var jsonObj = new JsonObject(JsonTest);
            var initialLength = jsonObj.Serialize().Length;
            var objToAdd = new JsonObject(jsonStringToAdd);
            Assert.IsFalse(jsonObj.AddAt(objToAdd, @"nonexisting"), "Can't add at unknown location");
            Assert.IsTrue(jsonObj.AddAt(objToAdd, "$"), "Adding succeeded");
            var serializedObj = jsonObj.Serialize();
            Assert.IsTrue(serializedObj.Length > initialLength, "Length has changed");
            Assert.IsTrue(serializedObj.Contains("NewProperty"), "Object contains new property");
            Assert.AreEqual(initialLength + jsonStringToAdd.Length - 1, serializedObj.Length, "Size OK");
            Debug.Print(jsonObj.Serialize());
        }

        [TestMethod, TestCategory("Unit")]
        public void JsonObjectCreateFromXmlTest()
        {
            var a = new JsonObject("<test>1</test>");
            Assert.AreEqual("{\"test\":\"1\"}", a.Serialize());
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
            Assert.IsTrue(jsonObj.Delete("LastModifiedDate"));
            Assert.IsNull(jsonObj.GetPropertyType("LastModifiedDate"));
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
            var props = jsonObj.GetProperties("").ToList();
            Assert.AreEqual(7, props.Count);
            Assert.IsTrue(props.Contains("Id"));
            Assert.IsTrue(props.Contains("Flags"));
        }

        [TestMethod, TestCategory("Unit")]
        public void JsonObjectGetPropertyTest()
        {
            var jsonObj = new JsonObject(JsonTest2, true);
            Assert.AreEqual("2015-01-01T00:00:00", jsonObj.GetProperty("DateValue"));
            Assert.AreEqual("{\"one\":1,\"two\":2}", jsonObj.GetProperty("Tree"), "Get sub-object returns serialized sub-object");
            // too doesn't get str
            Assert.AreEqual("[[\"hi\",\"there\"],[\"hello\",\"  too\"]]", jsonObj.GetProperty("Test"), "Get Array succeeds, too doesn't get trimmed");
            Assert.AreEqual("too", jsonObj.GetProperty("Test[1][1]"), "too gets trimmed when the flag is on");
        }

        [TestMethod, TestCategory("Unit")]
        public void JsonObjectSetPropertyTest()
        {
            var jsonObj = new JsonObject(JsonTest);
            Assert.IsFalse(jsonObj.SetProperty(string.Empty, null), "set empty property returns false");
            Assert.IsTrue(jsonObj.SetProperty("Name", "John Smith"), "Set property succeeds where current value is null");
            Assert.AreEqual("John Smith", jsonObj.GetProperty("Name"));
            Assert.AreEqual("String", jsonObj.GetPropertyType("Name"), "property type is correct");
        }

        [TestMethod, TestCategory("Unit")]
        public void JsonObjectTrimTest()
        {
            const string source = "{\"text\":\"   aa   \"}";
            const string locator = "text";
            var noTrim = new JsonObject(source);
            Assert.AreEqual("   aa   ", noTrim.GetProperty(locator));
            Assert.AreEqual("   aa   ", noTrim.Evaluate(locator));
            var trim = new JsonObject(source, true);
            Assert.AreEqual("aa", trim.GetProperty(locator));
            Assert.AreEqual("aa", trim.Evaluate(locator));
        }

        [TestMethod, TestCategory("Unit"), ExpectedException(typeof(ArgumentException))]
        public void JsonObjectWrongContentTest()
        {
            var _ = new JsonObject("qwe");
        }
    }
}
