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
using System.Diagnostics.CodeAnalysis;
using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rest;
using Rest.Model;

namespace RestTests
{
    [TestClass]
    public class RestTesterTests
    {
        [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Test framework signature")]
        [SuppressMessage("Usage", "CA1801:Review unused parameters", Justification = "Test framework signature")]
        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            // SessionContext is a singleton, so could have been set by other tests. Make sure it is pristine.
            Injector.CleanSessionContext();
            var _ = new RestConfig();
        }

        [TestMethod, TestCategory("Integration")]
        public void RestTesterTypicodeTest1()
        {
            var rt = new RestTester
            {
                EndPoint = "http://jsonplaceholder.typicode.com/",
                RequestBody = "{\"title\": \"Test Data\", \"body\": \"Test Body\", \"userId\":96 }"
            };
            rt.SetRequestHeaderTo("header1", "dummy");
            rt.SetRequestHeaderTo("header1", "value for header 1");
            Assert.IsTrue(string.IsNullOrEmpty(rt.RequestCookies), "Request Cookies empty");
            rt.SendTo("POST", "posts");
            var requestHeaders = rt.RequestHeaders();
            Assert.IsTrue(requestHeaders.Contains("User-Agent:"), "request header contains User Agent");
            var requestHeadersWithout = rt.RequestHeadersWithout(new List<string> {"User-Agent"});
            Assert.IsFalse(requestHeadersWithout.Contains("User-Agent:"), "request header without User Agent OK");
            Assert.IsTrue(requestHeadersWithout.Contains("Content-Type: application/json"), "request header Content-Type OK");
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

            Assert.IsTrue(rt.ResponseCookies().StartsWith("__cfduid="));
            Assert.IsTrue(rt.RequestCookies.Contains("Path=/; Domain=.typicode.com; HttpOnly"), "Request Cookie exists");
            Assert.IsTrue(rt.PropertyOfResponseCookie("value", "__cfduid").ToString().Length > 0);
            Assert.IsTrue(rt.PropertyOfResponseCookie("value", 0).ToString().Length > 0);
        }

        [TestMethod, TestCategory("Integration")]
        public void RestTesterTypicodeTest2()
        {
            var rt = new RestTester("http://jsonplaceholder.typicode.com/");
            Assert.AreEqual("http://jsonplaceholder.typicode.com/", rt.EndPoint);
            rt.SendToWithBody("POST", "posts", "{\"title\": \"Test Data\", \"body\": \"Test Body\", \"userId\":96 }");
            Assert.AreEqual("http://jsonplaceholder.typicode.com/posts", rt.RequestUri, "Request Uri OK");
            Assert.AreEqual(rt.RequestBody, "{\"title\": \"Test Data\", \"body\": \"Test Body\", \"userId\":96 }", "Request Body OK");
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

        [TestMethod, TestCategory("Integration"), ExpectedException(typeof(WebException))]
        public void RestTesterInvalidUrlTest()
        {
            var rt = new RestTester
            {
                EndPoint = "http://localhost:23456",
                RequestBody = "{ \"userId\":96 }"
            };
            rt.SendTo("POST", "posts");
        }

        [TestMethod, TestCategory("Integration")]
        public void RestTesterTypicodeTest3()
        {
            var rt = new RestTester
            {
                EndPoint = "http://jsonplaceholder.typicode.com/",
                RequestBody = "{\"title\": \"Test Data\", \"body\": \"Test Body\", \"userId\":96 }"
            };
            rt.SetRequestHeaderTo("Content-Type", "application/json; foo=2");
            rt.SetRequestHeaderTo("User-Agent", "FitNesseClient");
            rt.SetRequestHeaderTo("Accept", "application/json; test=3");
            rt.SendTo("POST", "posts");
            var requestHeaders = rt.RequestHeaders();
            Assert.IsTrue(requestHeaders.Contains("Content-Type: application/json; foo=2"));
            Assert.IsTrue(requestHeaders.Contains("User-Agent: FitNesseClient"));
            Assert.IsTrue(requestHeaders.Contains("Accept: application/json; test=3"));
        }

        [TestMethod, TestCategory("Unit")]
        public void RestTesterVersionInfoTest()
        {
            Assert.AreEqual(ApplicationInfo.Version , RestTester.VersionInfo("SHORT"));
        }
    }
}