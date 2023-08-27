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

using System;
using System.Collections.Generic;
using System.Net.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rest.Utilities;

namespace RestTests
{
    [TestClass]
    public class FitNesseFormatterTests
    {
        [TestMethod]
        [TestCategory("Unit")]
        public void FitNesseFormatterGetHeaderTest()
        {
            var headers = new HeaderDictionary
            {
                { "header1", new List<string> { "value1" } },
                { "header2", new List<string> { "value1", "value2" } }
            };
            Assert.AreEqual(string.Empty, FitNesseFormatter.GetHeader(headers, null));
            Assert.AreEqual(string.Empty, FitNesseFormatter.GetHeader(headers, "bogus"));
            Assert.AreEqual("value1", FitNesseFormatter.GetHeader(headers, "header1"));
            Assert.AreEqual("value1,value2", FitNesseFormatter.GetHeader(headers, "header2"));

            var request = new HttpRequestMessage();
            var requestHeaders = request.Headers;
            foreach (var entry in headers)
            {
                requestHeaders.Add(entry.Key, entry.Value);
            }

            Assert.AreEqual(string.Empty, FitNesseFormatter.GetHeader(requestHeaders, null));
            Assert.AreEqual(string.Empty, FitNesseFormatter.GetHeader(requestHeaders, "bogus"));
            Assert.AreEqual("value1", FitNesseFormatter.GetHeader(requestHeaders, "header1"));
            Assert.AreEqual("value1,value2", FitNesseFormatter.GetHeader(requestHeaders, "header2"));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void FitNesseFormatterHeaderListSelectionTest()
        {
            var headers = new HeaderDictionary
            {
                { "header1", new List<string> { "value1" } },
                { "header2", new List<string> { "value2" } },
                { "header3", new List<string> { "value3" } }
            };

            var result = FitNesseFormatter.HeaderList(headers, new List<string> { "header1", "header2" });
            Assert.AreEqual("header1: value1\nheader2: value2\n", result);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void FitNesseFormatterHeaderListTest()
        {
            var headers = new HeaderDictionary
            {
                { "header1", new List<string> { "value1" } },
                { "header2", new List<string> { "value2" } },
                { "header3", new List<string> { "value3" } }
            };
            var result = FitNesseFormatter.HeaderList(headers);
            Assert.AreEqual("header1: value1\nheader2: value2\nheader3: value3\n", result);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void FitNesseFormatterHeaderListWithoutTest()
        {
            var headers = new HeaderDictionary
            {
                { "header1", new List<string> { "value1" } },
                { "header2", new List<string> { "value2" } },
                { "header3", new List<string> { "value3" } }
            };
            var result = FitNesseFormatter.HeaderListWithout(headers, new List<string> { "header2" });
            Assert.AreEqual("header1: value1\nheader3: value3\n", result);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [ExpectedException(typeof(ArgumentException))]
        public void FitNesseFormatterParseCookiesErrorTest()
        {
            const string input = "=";
            _ = FitNesseFormatter.ParseCookies(input, null, DateTime.UtcNow);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void FitNesseFormatterParseCookiesTest()
        {
            const string input =
                "id=abc;expires=Sun, 14 Jun 2020 20:45:30 GMT;path=/;   domain=.example.com; HttpOnly\n" +
                "cookie2=test\n" +
                "_qa=name=john&age=47;domain=voorbeeld.nl;secure;path=/a/b\n" +
                "maxAgeTest1=ok;max-Age=86400;path=/\n" +
                "maxAgeTest2=good; max-Age=86401; Expires=bogus value with 12:34:56\n" +
                "maxAgeTest3=bad; max-Age=86402; MyExpires=bogus value with 12:34:56\n" +
                "maxAgeTest4=ugly;Expires=; max-Age=86403";
            var actual = FitNesseFormatter.ParseCookies(input, "default.org", new DateTime(2019, 6, 16, 11, 12, 13));
            Assert.AreEqual(7, actual.Count);
            var cookieList = FitNesseFormatter.CookieList(actual);
            Assert.AreEqual(
                "id=abc; Expires=Sun, 14 Jun 2020 20:45:30 GMT; Path=/; Domain=.example.com; HttpOnly\n" +
                "cookie2=test; Path=/; Domain=default.org\n" +
                "_qa=name=john&age=47; Path=/a/b; Domain=voorbeeld.nl; Secure\n" +
                "maxAgeTest1=ok; Expires=Mon, 17 Jun 2019 11:12:13 GMT; Path=/; Domain=default.org\n" +
                "maxAgeTest2=good; Expires=Mon, 17 Jun 2019 11:12:14 GMT; Path=/; Domain=default.org\n" +
                "maxAgeTest3=bad; Expires=Mon, 17 Jun 2019 11:12:15 GMT; Path=/; Domain=default.org\n" +
                "maxAgeTest4=ugly; Expires=Mon, 17 Jun 2019 11:12:16 GMT; Path=/; Domain=default.org", cookieList);
        }

        [TestMethod]
        [TestCategory("Unit")]
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