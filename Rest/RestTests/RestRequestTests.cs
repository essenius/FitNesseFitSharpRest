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
using System.Collections.Specialized;
using System.Net;
using System.Reflection;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rest.Model;

namespace RestTests
{
    [TestClass]
    public class RestRequestTests
    {
        [TestMethod]
        [TestCategory("Unit")]
        public void RestRequestCreateTest()
        {
            var context = new SessionContext();
            var uri = new Uri("http://localhost/api");
            var factory = new RestRequestFactory();
            var target = factory.Create(uri, context);
            Assert.IsNotNull(target);
            Assert.AreEqual(uri, target.RequestUri);
            Assert.AreEqual("FitNesseRest", target.HeaderValue("User-Agent"));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void RestRequestSetBodyForGetTest()
        {
            var context = new SessionContext();
            var uri = new Uri("http://localhost/api");
            var factory = new RestRequestFactory();
            var restRequest = factory.Create(uri, context);
            var method = restRequest.GetType().GetMethod("SetBody", BindingFlags.Instance | BindingFlags.NonPublic);
            method.Invoke(restRequest, new object[] {"hello", Encoding.GetEncoding("iso-8859-1"), "GET"});

            var fieldInfo = restRequest.GetType().GetField("_request", BindingFlags.Instance | BindingFlags.NonPublic);
            var req = fieldInfo.GetValue(restRequest) as HttpWebRequest;
            Assert.AreEqual(0, req?.ContentLength);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void RestRequestSupportsBodyTest()
        {
            Assert.IsTrue(RestRequest.SupportsBody("PUT"));
            Assert.IsTrue(RestRequest.SupportsBody("POST"));
            Assert.IsTrue(RestRequest.SupportsBody("DELETE"));
            Assert.IsFalse(RestRequest.SupportsBody("GET"));
            Assert.IsFalse(RestRequest.SupportsBody("HEAD"));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void RestRequestUpdateHeadersTest()
        {
            var context = new SessionContext();
            var uri = new Uri("http://localhost/api");
            var factory = new RestRequestFactory();
            var target = factory.Create(uri, context);
            var headers = new NameValueCollection
            {
                {"header1", "value1"},
                {"User-Agent", "UnitTest"},
                {"accept", "plain/text"},
                {"content-type", "application/xml"},
                {"Authorization", "my-hash"}
            };
            Assert.IsTrue(string.IsNullOrEmpty(target.HeaderValue("header1")), "header1 doesn't exist upfront");
            Assert.IsTrue(target.HeaderValue("Accept").Contains("application/json"),
                "Accept contains application/json upfront");
            target.UpdateHeaders(headers);
            Assert.AreEqual("value1", target.HeaderValue("header1"), "header1 exists afterwards");
            Assert.AreEqual("UnitTest", target.HeaderValue("User-Agent"), "User-Agent changed");
            Assert.AreEqual("plain/text", target.HeaderValue("Accept"), "Accept changed");
            Assert.AreEqual("application/xml", target.HeaderValue("Content-Type"), "Content-Type changed");
            Assert.AreEqual("my-hash", target.HeaderValue("Authorization"), "Authorization changed");
        }

        [TestMethod]
        [TestCategory("Unit")]
        [ExpectedException(typeof(ArgumentException))]
        public void RestRequestUpdateWrongHeaderTest()
        {
            var context = new SessionContext();
            var uri = new Uri("http://localhost/api");
            var factory = new RestRequestFactory();
            var target = factory.Create(uri, context);

            // We force a Date entry in the Headers property of the request by setting the request's Date property
            // That will cause the case statement in UpdateHeaders to choose default (i.e. throw argumentexception).
            var fieldInfo = target.GetType().GetField("_request", BindingFlags.Instance | BindingFlags.NonPublic);
            var request = fieldInfo.GetValue(target) as HttpWebRequest;
            Assert.IsNotNull(request, "Request field captured");
            request.Date = new DateTime(2019, 02, 23);

            var headers = new NameValueCollection
            {
                {"Date", "1-Jan-2000"}
            };
            target.UpdateHeaders(headers);
        }
    }
}