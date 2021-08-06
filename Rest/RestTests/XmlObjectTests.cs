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
using System.Globalization;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rest.ContentObjects;

namespace RestTests
{
    [TestClass]
    public class XmlObjectTests
    {
        private const string AtomXml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
                                       "<feed xml:base=\"https://localhost/Model.svc/\" " +
                                       "xmlns=\"http://www.w3.org/2005/Atom\" xmlns:d=\"http://schemas.microsoft.com/ado/2007/08/dataservices\" " +
                                       "xmlns:m=\"http://schemas.microsoft.com/ado/2007/08/dataservices/metadata\">" +
                                       "<id>https://localhost/Model.svc/Categories</id>" +
                                       "<title type=\"text\">Categories</title>" +
                                       "<updated>2015-02-19T07:36:45Z</updated>" +
                                       "<link rel=\"self\" title=\"Categories\" href=\"Categories\" />" +
                                       "<entry>" +
                                       "<id>https://localhost/Model.svc/Categories(41)</id>" +
                                       "<category term=\"Context.Category\" scheme=\"http://schemas.microsoft.com/ado/2007/08/dataservices/scheme\" />" +
                                       "<link rel=\"edit\" title=\"Category\" href=\"Categories(41)\" />" +
                                       "<title />" +
                                       "<updated>2015-02-19T07:36:45Z</updated>" +
                                       "<author><name /></author>" +
                                       "<content type=\"application/xml\">" +
                                       "<m:properties>" +
                                       "<d:Id m:type=\"Edm.Int32\">41</d:Id>" +
                                       "<d:Name>TestCategory</d:Name>" +
                                       "<d:CreatedBy m:null=\"true\" />" +
                                       "<d:ModifiedBy m:null=\"true\" />" +
                                       "<d:Created m:type=\"Edm.DateTime\">2013-04-04T08:58:23.413</d:Created>" +
                                       "<d:Modified m:type=\"Edm.DateTime\" m:null=\"true\" />" +
                                       "</m:properties>" +
                                       "</content>" +
                                       "</entry>" +
                                       "<entry>" +
                                       "<id>https://localhost/Model.svc/Categories(47)</id>" +
                                       "<category term=\"Context.Category\" scheme=\"http://schemas.microsoft.com/ado/2007/08/dataservices/scheme\" />" +
                                       "<link rel=\"edit\" title=\"Category\" href=\"Categories(47)\" />" +
                                       "<title />" +
                                       "<updated>2015-02-19T07:36:45Z</updated>" +
                                       "<author><name /></author>" +
                                       "<content type=\"application/xml\">" +
                                       "<m:properties>" +
                                       "<d:Id m:type=\"Edm.Int32\">47</d:Id>" +
                                       "<d:Name>TestTest</d:Name>" +
                                       "<d:CreatedBy m:null=\"true\" />" +
                                       "<d:ModifiedBy m:null=\"true\" />" +
                                       "<d:Created m:type=\"Edm.DateTime\">2013-04-17T08:52:57.08</d:Created>" +
                                       "<d:Modified m:type=\"Edm.DateTime\" m:null=\"true\" />" +
                                       "</m:properties>" +
                                       "</content>" +
                                       "</entry>" +
                                       "</feed>";

        private const string PlainXml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
                                        "<feed>" +
                                        "<id>https://localhost/Model.svc/Categories</id>" +
                                        "<title type=\"text\">Categories</title>" +
                                        "<updated>2015-02-19T07:36:45Z</updated>" +
                                        "<link rel=\"self\" title=\"Categories\" href=\"Categories\" />" +
                                        "<entry>" +
                                        "<id>https://localhost/Model.svc/Categories(41)</id>" +
                                        "<category term=\"Context.Category\" scheme=\"http://schemas.microsoft.com/ado/2007/08/dataservices/scheme\" />" +
                                        "<link rel=\"edit\" title=\"Category\" href=\"Categories(41)\" />" +
                                        "<title />" +
                                        "<updated>2015-02-19T07:36:45Z</updated>" +
                                        "<author><name /></author>" +
                                        "<content type=\"application/xml\">" +
                                        "<properties>" +
                                        "<Id type=\"Edm.Int32\">41</Id>" +
                                        "<Name>TestCategory</Name>" +
                                        "<CreatedBy null=\"true\" />" +
                                        "<ModifiedBy null=\"true\" />" +
                                        "<Created type=\"Edm.DateTime\">2013-04-04T08:58:23.413</Created>" +
                                        "<Modified type=\"Edm.DateTime\" null=\"true\" />" +
                                        "</properties>" +
                                        "</content>" +
                                        "</entry>" +
                                        "<entry>" +
                                        "<id>https://localhost/Model.svc/Categories(47)</id>" +
                                        "<category term=\"Context.Category\" scheme=\"http://schemas.microsoft.com/ado/2007/08/dataservices/scheme\" />" +
                                        "<link rel=\"edit\" title=\"Category\" href=\"Categories(47)\" />" +
                                        "<title />" +
                                        "<updated>2015-02-19T07:36:45Z</updated>" +
                                        "<author><name /></author>" +
                                        "<content type=\"application/xml\">" +
                                        "<properties>" +
                                        "<Id type=\"Edm.Int32\">47</Id>" +
                                        "<Name>TestTest</Name>" +
                                        "<CreatedBy null=\"true\" />" +
                                        "<ModifiedBy null=\"true\" />" +
                                        "<Created type=\"Edm.DateTime\">2013-04-17T08:52:57.08</Created>" +
                                        "<Modified type=\"Edm.DateTime\" null=\"true\" />" +
                                        "</properties>" +
                                        "</content>" +
                                        "</entry>" +
                                        "</feed>";

        [TestMethod, TestCategory("Unit")]
        public void XmlObjectAddXmlTest()
        {
            var xmlObj = new XmlObject(AtomXml, "atom", "m:type");
            var objToAdd = new XmlObject("<?xml version=\"1.0\"?><contributor><name>Lara</name></contributor>", null, null);
            xmlObj.AddAt(objToAdd, "/atom:feed/atom:entry[2]/atom:content/m:properties");
            Assert.AreEqual("Lara", xmlObj.Evaluate("/atom:feed/atom:entry[2]/atom:content/m:properties/contributor/name"));
        }

        [TestMethod, TestCategory("Unit")]
        public void XmlObjectAtomTest()
        {
            var xmlObj = new XmlObject(AtomXml, "atom", "m:type");
            Assert.AreEqual("https://localhost/Model.svc/Categories(41)", xmlObj.Evaluate("/atom:feed/atom:entry[1]/atom:id"));
            Assert.AreEqual("41", xmlObj.Evaluate("/atom:feed/atom:entry[1]/atom:content/m:properties/d:Id"));
        }

        [TestMethod, TestCategory("Unit")]
        public void XmlObjectCreateFromJsonTest()
        {
            var a = new XmlObject("{ \"test\": 1 }", null, null);
            Assert.AreEqual("<test>1</test>", a.Serialize());
            var b = new XmlObject("{ \"test\": 1, \"a\": \"b\" }", null, null);
            Assert.AreEqual("<root><test>1</test><a>b</a></root>", b.Serialize());
        }

        [TestMethod, TestCategory("Unit")]
        public void XmlObjectCreateMultiRootTest()
        {
            var a = new XmlObject("<a>test</a><b>test</b>", "a", null);
            Assert.AreEqual("<root><a>test</a><b>test</b></root>", a.Serialize());
        }

        [TestMethod, TestCategory("Unit")]
        public void XmlObjectCreatePlainTest()
        {
            var a = new XmlObject("<a>test</a>", "a", null);
            Assert.AreEqual("<a>test</a>", a.Serialize());
        }

        [TestMethod, TestCategory("Unit")]
        public void XmlObjectEvaluateTest()
        {
            const string items = "<item id=\"myId\">3349</item>\r\n<item id=\"otherId\">2268</item>\r\n<item id=\"bool\">false</item>";
            const string source = "<data>" + items + "</data>";
            var xmlObject = new XmlObject(source, null, null);
            var evaluation = xmlObject.Evaluate("/data");

            // Apply CompareOptions.IgnoreSymbols enum here to support asserting on multiple platforms. e.g. New line symbol "\n\r" for Windows, and "\n" for Linux/Mac 
            Assert.AreEqual(0, string.Compare(items, evaluation, CultureInfo.CurrentCulture, CompareOptions.IgnoreSymbols));
            Assert.AreEqual(0, string.Compare("3349", xmlObject.Evaluate("/data/item[@id='myId']"), CultureInfo.CurrentCulture, CompareOptions.IgnoreSymbols));
            Assert.AreEqual(0, string.Compare("2268", xmlObject.Evaluate("/data/item[@id='otherId']"), CultureInfo.CurrentCulture, CompareOptions.IgnoreSymbols));
            Assert.AreEqual("True", xmlObject.Evaluate("5 > 2"));
            Assert.AreEqual("abc", xmlObject.Evaluate("'abc'"));
            Assert.AreEqual(null, xmlObject.Evaluate("/data/nonexistingitem"));
        }

        [TestMethod, TestCategory("Unit")]
        public void XmlObjectGetAllPropertiesTest()
        {
            var xmlObj = new XmlObject(AtomXml, "atom", "m:type");
            var actual = xmlObj.GetProperties("/atom:feed/atom:entry[2]/atom:author//*").Aggregate("", (current, entry) => current + entry);
            Assert.AreEqual("/atom:feed[1]/atom:entry[2]/atom:author[1]/atom:name[1]", actual);
        }

        [TestMethod, TestCategory("Unit")]
        public void XmlObjectGetPropertiesTest()
        {
            var a = new XmlObject("<?pi test?><!--comment--><a type='string' alt='check'>test</a><b type='int'>4</b>", "q", "type");
            var props = a.GetProperties("a/@*").ToList();
            Assert.AreEqual(2, props.Count);
            Assert.AreEqual("/root[1]/a[1]/@type", props[0]);
            Assert.AreEqual("/root[1]/a[1]/@alt", props[1]);
            props = a.GetProperties("//comment()").ToList();
            Assert.AreEqual("/root[1]/comment()", props[0]);
            var all = a.GetProperties(string.Empty).ToList();
            Assert.AreEqual("/root[1]", all[0]);
            Assert.AreEqual(3, all.Count);
            a = new XmlObject("<root/>", "q", "type");
            props = a.GetProperties("/root").ToList();
            Assert.AreEqual(1, props.Count);
        }

        [TestMethod, TestCategory("Unit"), ExpectedException(typeof(ArgumentException))]
        public void XmlObjectGetPropertiesUnknownElementTest()
        {
            var a = new XmlObject("<?pi test?>", "q", null);
            var _ = a.GetProperties("processing-instruction('pi')");
        }

        [TestMethod, TestCategory("Unit")]
        public void XmlObjectGetPropertyForUnknownElementTest()
        {
            var a = new XmlObject("<root/>", "q", null);
            Assert.IsTrue(string.IsNullOrEmpty(a.GetProperty("/any")));
        }

        [TestMethod, TestCategory("Unit")]
        public void XmlObjectGetTypeViaAttributeTest()
        {
            var a = new XmlObject("<a type='string'>test</a><b type='int'>4</b><c>def</c>", "q", "type");
            Assert.AreEqual("string", a.GetPropertyType("a"), "a is ok");
            Assert.AreEqual("int", a.GetPropertyType("b"), "b is ok");
            Assert.AreEqual("System.String", a.GetPropertyType("c"), "c is ok");
        }

        [TestMethod, TestCategory("Unit")]
        public void XmlObjectPlainXmlTest()
        {
            var xmlObj = new XmlObject(PlainXml, null, null);
            Assert.AreEqual("https://localhost/Model.svc/Categories(41)", xmlObj.Evaluate("/feed/entry[1]/id"));
            Assert.AreEqual("41", xmlObj.Evaluate("/feed/entry[1]/content/properties/Id"));
        }

        [TestMethod, TestCategory("Unit")]
        public void XmlObjectSetPropertyTest()
        {
            var a = new XmlObject("<root id=''/>", "q", null);
            Assert.IsFalse(a.SetProperty("/any", "should not work"));
            Assert.IsTrue(a.SetProperty("/root/@id", "test"));
            Assert.AreEqual("test", a.GetProperty("/root/@id"));
        }

        [TestMethod, TestCategory("Unit")]
        public void XmlObjectTrimTest()
        {
            const string source = "<text>   aa   </text>";
            const string locator = "/text";
            var noTrim = new XmlObject(source, "q", null);
            Assert.AreEqual("   aa   ", noTrim.GetProperty(locator));
            Assert.AreEqual("   aa   ", noTrim.Evaluate(locator));
            var trim = new XmlObject(source, "q", null, true);
            Assert.AreEqual("aa", trim.GetProperty(locator));
            Assert.AreEqual("aa", trim.Evaluate(locator));
        }

        [TestMethod, TestCategory("Unit"), ExpectedException(typeof(ArgumentException))]
        public void XmlObjectWrongXmlTest()
        {
            var _ = new XmlObject("qwe", null, null);
        }
    }
}
