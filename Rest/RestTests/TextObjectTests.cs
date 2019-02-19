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

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rest.ContentObjects;

namespace RestTests
{
    [TestClass]
    public class TextObjectTests
    {
        [TestMethod, TestCategory("Unit"), ExpectedException(typeof(NotImplementedException)),
         SuppressMessage("ReSharper", "UnusedVariable", Justification = "Forcing exception")]
        public void TextObjectCreateWithNonStringThrowsException()
        {
            var obj = new TextObject(25);
        }

        [TestMethod, TestCategory("Unit")]
        public void TextObjectDeleteTest()
        {
            const string source = "abc=123; def=789";
            var textObject = new TextObject(source);
            Assert.IsTrue(textObject.Delete("(abc=123; )"));
            Assert.AreEqual("def=789", textObject.Serialize());
        }

        [TestMethod, TestCategory("Unit")]
        public void TextObjectEvaluatorTest()
        {
            const string source = "{\"ResponseCode\":\"0\", \"ResponseText\":\"3349\"}";
            var textObject = new TextObject(source);
            Assert.AreEqual("3349", textObject.Evaluate("\"ResponseText\":\"(\\d+)\""));
            Assert.AreEqual(null, textObject.Evaluate("NonExistingElement"));
            Assert.AreEqual("Text", textObject.Evaluate("(?:(?:\"Response([a-zA-Z]*)\":).*?){2}"));
        }

        [TestMethod, TestCategory("Unit")]
        public void TextObjectGetPropertiesTest()
        {
            const string source = "MyTest\r\nYourTest\r\nHisTest\r\n";
            var textObject = new TextObject(source);
            var props = textObject.GetProperties("([a-z|A-Z]*?)Test").ToList();
            Assert.AreEqual(3, props.Count);
            Assert.AreEqual("My", textObject.GetProperty(props[0]));
            Assert.AreEqual("Your", textObject.GetProperty(props[1]));
            Assert.AreEqual("His", textObject.GetProperty(props[2]));
        }

        [TestMethod, TestCategory("Unit")]
        public void TextObjectIsValidTest()
        {
            Assert.IsTrue(TextObject.IsValid("abc"), "abc is valid text");
            Assert.IsFalse(TextObject.IsValid("abc" + (char) 2), "abc with control char is not valid text");
        }

        [TestMethod, TestCategory("Unit")]
        public void TextObjectSetPropertyTest()
        {
            const string source = "abc=123; def=789";
            var textObject = new TextObject(source);
            // replace the first occurrence of = followed by a number of digits
            Assert.IsTrue(textObject.SetProperty("(?:=(\\d+))", "456"));
            Assert.AreEqual("abc=456; def=789", textObject.Serialize());

            // regex should not match, so it should not change anything 
            Assert.IsFalse(textObject.SetProperty("(?:ghi=(\\d+))", "456"));
            Assert.AreEqual("abc=456; def=789", textObject.Serialize());
        }

        [TestMethod, TestCategory("Unit"), ExpectedException(typeof(NotImplementedException))]
        public void TextObjectAddToTest()
        {
            new TextObject(string.Empty).AddAt(null, string.Empty);
        }
    }
}