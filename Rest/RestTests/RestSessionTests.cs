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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rest.Model;

namespace RestTests
{
    [TestClass]
    public class RestSessionTests
    {
        [TestMethod]
        [TestCategory("Unit")]
        public void RestSessionMakeRequestTest()
        {
            const string textBody = "random text";
            var h = new RestSession(null, new SessionContext(), new RestRequestMockFactory())
            {
                Body = textBody,
                EndPoint = new Uri("http://localhost")
            };
            Assert.IsTrue(h.MakeRequest("Get", "api"), "MakeRequest succeeds");
            Assert.AreEqual("http://localhost/api", h.Request.RequestUri.AbsoluteUri, "RequestUri OK");
            var m = (RestRequestMock)h.Request;
            Assert.IsTrue(m.ExecuteWasCalled, "Execute was called");
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void RestSessionNullValueTest()
        {
            var session = new RestSession(null, null, null) { Body = null };
            Assert.IsNull(session.ResponseText);
            Assert.IsNull(session.Body);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void RestSessionRequestHeaderValueNullTest()
        {
            var h = new RestSession(null, new SessionContext(), null);
            Assert.IsTrue(string.IsNullOrEmpty(h.RequestHeaderValue(null)));
            Assert.IsTrue(string.IsNullOrEmpty(h.ResponseHeaderValue(null)));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void RestSessionSetBodyTest()
        {
            const string textBody = "Text Line 1\r\nTextLine2\r\n";
            var h = new RestSession(null, null, null) { Body = textBody };
            Assert.AreEqual(h.Body, textBody);

            const string jsonBodyIn = "{\"title\": \"Test Data\",\r\n \"body\": \"Test Body\",\r\n \"userId\":96\r\n }";
            const string jsonBodyOut = "{\"title\": \"Test Data\", \"body\": \"Test Body\", \"userId\":96 }";
            h.Body = jsonBodyIn;
            Assert.AreEqual(h.Body, jsonBodyOut);
        }
    }
}