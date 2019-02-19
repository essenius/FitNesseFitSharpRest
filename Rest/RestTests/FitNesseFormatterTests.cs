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

using System.Collections.Generic;
using System.Collections.Specialized;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rest.Utilities;

namespace RestTests
{
    [TestClass]
    public class FitNesseFormatterTests
    {
        [TestMethod, TestCategory("Unit")]
        public void FitNesseFormatterHeaderListSelectionTest()
        {
            var headerList = new NameValueCollection
            {
                {"header1", "value1"},
                {"header2", "value2"},
                {"header3", "value3"}
            };
            var result = FitNesseFormatter.HeaderList(headerList, new List<string> {"header1", "header2"});
            Assert.AreEqual("header1: value1\nheader2: value2\n", result);
        }

        [TestMethod, TestCategory("Unit")]
        public void FitNesseFormatterHeaderListTest()
        {
            var headerList = new NameValueCollection
            {
                {"header1", "value1"},
                {"header2", "value2"},
                {"header3", "value3"}
            };
            var result = FitNesseFormatter.HeaderList(headerList);
            Assert.AreEqual("header1: value1\nheader2: value2\nheader3: value3\n", result);
        }

        [TestMethod, TestCategory("Unit")]
        public void FitNesseFormatterHeaderListWithoutTest()
        {
            var headerList = new NameValueCollection
            {
                {"header1", "value1"},
                {"header2", "value2"},
                {"header3", "value3"}
            };
            var result = FitNesseFormatter.HeaderListWithout(headerList, new List<string> {"header2"});
            Assert.AreEqual("header1: value1\nheader3: value3\n", result);
        }

        [TestMethod, TestCategory("Unit")]
        public void FitNesseFormatterParseNameValueCollectionTest()
        {
            const string input = "header1:value1\nheader2 : value2\n    header3: value3:a    ";
            var actual = FitNesseFormatter.ParseNameValueCollection(input);
            Assert.AreEqual(3, actual.Count);
            Assert.AreEqual("value1", actual.Get("header1"));
            Assert.AreEqual("value2", actual.Get("header2"));
            Assert.AreEqual("value3:a", actual.Get("header3"));
        }
    }
}