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

using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rest;

namespace RestTests
{
    [TestClass]
    public class ContentHandlerTests
    {
        [TestMethod, TestCategory("Unit")]
        public void ContentHandlerAddToTest()
        {
            var h = new ContentHandler();
            var baseObj = h.CreateObjectFrom("<a><b>test</b><c/></a>");
            var insertObj = h.CreateObjectFrom("<d>test2</d>");
            Assert.IsTrue(ContentHandler.AddToAt(insertObj, baseObj, "/a/c"));
            Assert.AreEqual("<a><b>test</b><c><d>test2</d></c></a>", baseObj.Serialize());
        }

        [TestMethod, TestCategory("Unit")]
        public void ContentHandlerCreateJsonObjectTest()
        {
            var h = new ContentHandler();
            var obj = h.CreateObjectFrom("{ \"a\" : \"test\" }");
            Assert.AreEqual("JSON Object", obj.ToString());
            Assert.AreEqual("test", obj.GetProperty("a"));
            ContentHandler.SetPropertyValueOfTo("a", obj, "replaced");
            Assert.AreEqual("replaced", ContentHandler.PropertyValueOf("a", obj));
        }

        [TestMethod, TestCategory("Unit")]
        public void ContentHandlerCreateObjectFromTypeInAssembly()
        {
            var h = new ContentHandler();
            var obj = h.CreateObjectFromTypeInAssembly("xml", "RestTests.FewTypes", "RestTests.dll");
            const string expected = "<?xml version=\"1.0\" encoding=\"utf-16\"?>" +
                                    "<FewTypes xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">" +
                                    "<BoolValue>false</BoolValue>" +
                                    "<ByteValue>0</ByteValue>" +
                                    "<CharValue>0</CharValue>" +
                                    "<DoubleValue>0</DoubleValue>" +
                                    "<IntValue>0</IntValue>" +
                                    "<LongValue>0</LongValue>" +
                                    "</FewTypes>";
            Assert.AreEqual(expected.Length, obj.Serialize().Length);
            Assert.AreEqual(expected, obj.Serialize(), "FewTypes incorrectly serialized");
        }

        [TestMethod, TestCategory("Unit")]
        public void ContentHandlerCreateObjectFromTypeInAssemblyWithParam()
        {
            var h = new ContentHandler();
            var obj = h.CreateObjectFromTypeInAssemblyWithParams("xml", "RestTests.FewTypes", "RestTests.dll",
                new[] {"true"});
            Assert.AreEqual(
                "<?xml version=\"1.0\" encoding=\"utf-16\"?>" +
                "<FewTypes xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">" +
                "<BoolValue>true</BoolValue>" +
                "<ByteValue>255</ByteValue>" +
                "<CharValue>99</CharValue>" +
                "<DoubleValue>3.1415926535</DoubleValue>" +
                "<IntValue>2147483647</IntValue>" +
                "<LongValue>-223372036854775808</LongValue>" +
                "<StringValue>string value</StringValue>" +
                "</FewTypes>",
                obj.Serialize(), "FewTypes incorrectly serialized");
        }

        [TestMethod, TestCategory("Unit")]
        public void ContentHandlerCreateXmlObjectTest()
        {
            var h = new ContentHandler();
            var obj = h.CreateObjectFrom("<a>test</a>");
            Assert.AreEqual("XML Object", obj.ToString());
            Assert.AreEqual("<a>test</a>", obj.Serialize());
            Assert.AreEqual("test", obj.GetProperty("/a"));
            ContentHandler.SetPropertyValueOfTo("/a", obj, "replaced");
            Assert.AreEqual("<a>replaced</a>", obj.Serialize());
        }

        [TestMethod, TestCategory("Unit")]
        public void ContentHandlerDeleteFromObject()
        {
            var h = new ContentHandler();
            var baseObj = h.CreateObjectFrom("<a><b>test</b><c/></a>");
            ContentHandler.DeleteFrom("/a/b", baseObj);
            Assert.AreEqual("<a><c /></a>", ContentHandler.Serialize(baseObj));
        }

        [TestMethod, TestCategory("Unit")]
        public void ContentHandlerEvaluateTest()
        {
            var h = new ContentHandler();
            var baseObj = h.CreateObjectFrom("<a><b/><b class='q'>test</b></a>");
            var value = ContentHandler.EvaluateOn("/a/b[@class='q']", baseObj);
            Assert.AreEqual("test", value);
        }

        [TestMethod, TestCategory("Unit")]
        public void ContentHandlerGetClassesTest()
        {
            var classCollection = ContentHandler.ClassesIn("RestTests.dll");
            Assert.IsTrue(classCollection.Contains("RestTests.FewTypes"));
            Assert.IsTrue(classCollection.Contains("RestTests.ManyTypes"));
        }

        [TestMethod, TestCategory("Unit")]
        public void ContentHandlerLoadObjectFromTest()
        {
            const string jsonTest = "{ \"Id\": 0, \"Name\": \"Joe\", \"IsShared\": true }";
            var file = Path.GetTempFileName();
            File.WriteAllText(file, jsonTest);
            var h = new ContentHandler();
            var baseObj = h.LoadObjectFrom(file);
            Assert.AreEqual("0", ContentHandler.PropertyValueOf("Id", baseObj), "Id matches");
            Assert.AreEqual("Joe", ContentHandler.PropertyValueOf("Name", baseObj), "Name matches");
            Assert.AreEqual("True", ContentHandler.PropertyValueOf("IsShared", baseObj), "IsShared matches");
        }

        [TestMethod, TestCategory("Unit"),
        ExpectedException(typeof(ArgumentException))]
        public void ContentHandlerLoadBinaryObjectFromTest()
        {
            var file = Path.GetTempFileName();

            using (var writer = new BinaryWriter(File.Open(file, FileMode.Create)))
            {
                writer.Write(0);
            }
            var h = new ContentHandler();
            _ = h.LoadObjectFrom(file);
        }

        [TestMethod, TestCategory("Unit")]
        public void ContentHandlerPropertiesOfTest()
        {
            var h = new ContentHandler();
            var baseObj = h.CreateObjectFrom("<a><b>test</b><c/></a>");
            var props = ContentHandler.PropertiesOf("/a/*", baseObj).ToArray();
            Assert.AreEqual(props[0], "/a[1]/b[1]");
            Assert.AreEqual(props[1], "/a[1]/c[1]");
        }

        [TestMethod, TestCategory("Unit")]
        public void ContentHandlerPropertyTypeOfTest()
        {
            var h = new ContentHandler();
            var baseObj = h.CreateObjectFrom("<a><b>test</b><b/></a>");
            var value = ContentHandler.PropertyTypeOf("/a/b", baseObj);
            Assert.AreEqual("System.String", value);
        }

        [TestMethod, TestCategory("Unit")]
        public void ContentHandlerPropertySetOfContainsTest()
        {
            var h = new ContentHandler();
            var baseObj = h.CreateObjectFrom("<a><b>test</b><b/></a>");
            Assert.IsTrue(ContentHandler.PropertySetOfContainsValueLike("", baseObj, "test"));
            Assert.IsFalse(ContentHandler.PropertySetOfContainsValueLike("", baseObj, "q"));
            Assert.IsTrue(ContentHandler.PropertySetOfContainsValueLike("/a", baseObj, "test"));
            Assert.IsTrue(ContentHandler.PropertySetOfContainsValueLike("/a/b", baseObj, "test"));
            Assert.IsFalse(ContentHandler.PropertySetOfContainsValueLike("/a/c", baseObj, "test"));
            Assert.IsTrue(ContentHandler.PropertySetOfContainsValueLike("", baseObj, ""));
            Assert.IsFalse(ContentHandler.PropertySetOfContainsValueLike("/b", baseObj, ""));
        }

        [TestMethod, TestCategory("Unit")]
        public void ContentHandlerPropertyValueOfTest()
        {
            var h = new ContentHandler();
            var baseObj = h.CreateObjectFrom("<a><b>test</b><c/></a>");
            var value = ContentHandler.PropertyValueOf("/a/b", baseObj);
            Assert.AreEqual("test", value);
        }
    }
}