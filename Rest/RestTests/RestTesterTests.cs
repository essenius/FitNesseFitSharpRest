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
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rest;
using Rest.Model;

namespace RestTests
{
    [TestClass]
    public class RestTesterTests
    {
        [TestMethod]
        [TestCategory("Integration")]
        [ExpectedException(typeof(AggregateException))]
        public void RestTesterInvalidUrlTest()
        {
            // Make sure we don't get any proxy intercepts
            var restConfig = new RestConfig();
            var input = new List<List<string>>
            {
                new List<string> {"Proxy", "None"}
            };

            _ = restConfig.DoTable(input);

            var rt = new RestTester
            {
                EndPoint = "http://localhost:8765",
                RequestBody = "{ \"userId\":96 }"
            };

            rt.SendTo("Post", "posts");

            Assert.AreEqual(403, rt.ResponseCode);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void RestTesterStripNewLinesFromTest()
        {
            Assert.AreEqual("Hi There! How are you?",
                RestTester.StripNewLinesFrom("\n\rHi \r\nThere!\n How are you?\r"));
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void RestTesterTypicodeTest1()
        {
            // we need at least one test using https
            var rt = new RestTester
            {
                EndPoint = "https://jsonplaceholder.typicode.com/",
                RequestBody = "{\"title\": \"Test Data\", \"body\": \"Test Body\", \"userId\":96 }"
            };
            Assert.IsNull(rt.RequestUri, "RequestUri null (OK)");
            Assert.IsNull(rt.Cookies, "RequestCookies null (OK)");
            rt.SetRequestHeaderTo("Content-Type", "application/json; charset=UTF-8");
            rt.SetRequestHeaderTo("header1", "dummy");
            rt.SetRequestHeaderTo("header1", "value for header 1");

            Assert.IsTrue(string.IsNullOrEmpty(rt.Cookies), "Request Cookies empty");
            rt.SendTo("POST", "posts");
            Assert.AreEqual("https://jsonplaceholder.typicode.com/posts", rt.RequestUri, "RequestUri OK");

            var requestHeaders = rt.RequestHeaders();
            Assert.IsTrue(requestHeaders.Contains("User-Agent:"), "request header contains User Agent");
            var requestHeadersWithout = rt.RequestHeadersWithout(new List<string> {"User-Agent"});
            Assert.IsFalse(requestHeadersWithout.Contains("User-Agent:"), "request header without User Agent OK");
            Assert.IsTrue(requestHeadersWithout.Contains("Content-Type: application/json"),
                "request header Content-Type OK");
            var customRequestHeader = rt.RequestHeaders(new List<string> {"header1"});
            Assert.IsTrue(customRequestHeader.Contains("header1: value for header 1"), "custom header exists");
            Assert.IsTrue(requestHeadersWithout.Contains("Content-Length: 57"), "request header Content-Length OK");
            Assert.AreEqual(201, rt.ResponseCode, "Response code OK");
            Assert.AreEqual("Created", rt.ResponseCodeDescription);
            Assert.AreEqual("101", rt.ValueFromResponseMatching("id"), "Response ID OK");
            var responseHeaders = rt.ResponseHeaders(new List<string> {"Content-Length", "Content-Type"});
            Assert.AreEqual("Content-Length: 78\nContent-Type: application/json; charset=utf-8\n", responseHeaders);
            var ro = rt.ResponseObject;
            Assert.AreEqual("JSON Object", ro.ToString());
            Assert.IsTrue(string.IsNullOrEmpty(rt.Cookies), "cookies empty");
            Assert.IsNull(rt.PropertyOfCookie("test", "test"), "no property test for cookie test");
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void RestTesterTypicodeTest2()
        {
            var rt = new RestTester("http://jsonplaceholder.typicode.com/");
            Assert.AreEqual("http://jsonplaceholder.typicode.com/", rt.EndPoint);
            rt.SetRequestHeaderTo("Content-Type", "application/json; charset=UTF-8");
            rt.SendToWithBody("POST", "posts", "{\"title\": \"Test Data\", \"body\": \"Test Body\", \"userId\":96 }");
            Assert.AreEqual("http://jsonplaceholder.typicode.com/posts", rt.RequestUri, "Request Uri OK");
            Assert.AreEqual(
                rt.RequestBody,
                "{\"title\": \"Test Data\", \"body\": \"Test Body\", \"userId\":96 }",
                "Request Body OK");
            Assert.AreEqual("57", rt.ValueFromRequestHeaderMatching("Content-Length", "(\\d+)"));
            Assert.AreEqual("78", rt.ValueFromResponseHeaderMatching("Content-Length", "(\\d+)"));

            var responseHeaders = rt.ResponseHeaders();
            Assert.IsTrue(responseHeaders.Contains("Date:"), "response header Date OK");

            var responseHeadersWithout = rt.ResponseHeadersWithout(new List<string> {"Date"});
            Assert.IsFalse(responseHeadersWithout.Contains("Date"), "response header without Date OK");
            Assert.IsTrue(responseHeadersWithout.Contains("Cache-Control:"), "Response headers contain Cache Control");

            var response = rt.Response();
            Assert.IsTrue(response.Contains("\"title\": \"Test Data\""), "Response contains title");
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void RestTesterTypicodeTest3()
        {
            var context = Injector.InjectSessionContext();
            context.SetConfig("Cookies", "cookie1=value1");
            var rt = new RestTester
            {
                EndPoint = "http://jsonplaceholder.typicode.com/",
                RequestBody = "{\"title\": \"Test Data\", \"body\": \"Test Body\", \"userId\":96 }"
            };
            rt.SetRequestHeaderTo("Content-Type", "application/json; foo=2");
            rt.SetRequestHeaderTo("User-Agent", "FitNesseClient");
            rt.SetRequestHeaderTo("Accept", "application/json; test=3");

            rt.SendTo("POST", "posts");
            Assert.AreEqual(201, rt.ResponseCode);
            var requestHeaders = rt.RequestHeaders();
            Assert.AreEqual("cookie1=value1; Path=/; Domain=jsonplaceholder.typicode.com", rt.Cookies, "Cookies OK");
            Assert.AreEqual("/",rt.PropertyOfCookie("Path", "cookie1"));
            Assert.AreEqual("jsonplaceholder.typicode.com", rt.PropertyOfCookie("Domain", "cookie1"));
            Assert.AreEqual("/", rt.PropertyOfCookie("Path", 0));

            Assert.IsTrue(requestHeaders.Contains("Content-Type: application/json; foo=2"));
            Assert.IsTrue(requestHeaders.Contains("User-Agent: FitNesseClient"));
            Assert.IsTrue(requestHeaders.Contains("Accept: application/json; test=3"));
            rt.SendTo("DELETE", "posts/96");
            Assert.AreEqual(200, rt.ResponseCode);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void RestTesterEndPointNullTest()
        {
            var rt = new RestTester();
            Assert.IsNull(rt.EndPoint);
            Assert.IsNull(rt.RequestUri);
            Assert.IsNull(rt.Cookies);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void RestTesterVersionInfoTest()
        {
            Assert.AreEqual(ApplicationInfo.Version, RestTester.VersionInfo("SHORT"));
        }

        [TestInitialize]
        public void TestInitialize() => Injector.CleanSessionContext();
    }
}
