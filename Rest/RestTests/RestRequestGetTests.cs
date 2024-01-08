// Copyright 2015-2024 Rik Essenius
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
using System.Net.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rest.Model;
using Rest.Utilities;

namespace RestTests
{
    [TestClass]
    public class RestRequestGetTests
    {

        [TestMethod]
        [TestCategory("Unit")]
        public void RestRequestUpdateHeadersGetTest()
        {
            var context = new SessionContext();
            var uri = new Uri("http://localhost/api");
            var factory = new RestRequestFactory();
            var restRequest = factory.Create(uri, context);
            var headers = new NameValueCollection
            {
                { "header1", "value1" },
                { "User-Agent", "UnitTest" },
                { "accept", "plain/text" }
            };
            restRequest.SetBody(string.Empty, HttpMethod.Get);
            Assert.IsFalse(restRequest.Headers.TryGetValues("header1", out _), "header1 doesn't exist upfront");
            restRequest.UpdateHeaders(headers);
            Assert.AreEqual("value1", FitNesseFormatter.GetHeader(restRequest.Headers, "header1"), "header1 exists afterwards");
            Assert.AreEqual("UnitTest", restRequest.Headers.UserAgent.ToString(), "User-Agent OK");
            Assert.AreEqual("plain/text", restRequest.Headers.Accept.ToString(), "Accept OK");
            restRequest.ExecuteAndFail(HttpMethod.Get);
            Assert.IsNull(restRequest.ContentHeaders, "No content headers");
        }

    }
}