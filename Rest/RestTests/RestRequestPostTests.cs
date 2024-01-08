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
using System.Net.Http.Headers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rest.Model;

namespace RestTests
{
    [TestClass]
    public class RestRequestPostTests
    {


        [TestMethod]
        [TestCategory("Unit")]
        public void RestRequestUpdateHeadersPostTest() {
            var context = new SessionContext();
            var uri = new Uri("http://localhost/api");
            var factory = new RestRequestFactory();
            var restRequest = factory.Create(uri, context);
            var headers = new NameValueCollection
            {
                { "accept", "plain/text" },
                { "content-type", "application/xml" },
                { "Authorization", "my-hash" }
            };
            restRequest.SetBody("<hello/>", HttpMethod.Post);
            restRequest.UpdateHeaders(headers);

            Assert.AreEqual("plain/text", restRequest.Headers.Accept.ToString(), "Accept OK");
            restRequest.ExecuteAndFail(HttpMethod.Post);
            Assert.IsNotNull(restRequest.Headers.Authorization, "restRequest.Headers.Authorization != null");
            Assert.AreEqual("my-hash", restRequest.Headers.Authorization.ToString(), "Authorization OK");
            Assert.AreEqual(new MediaTypeHeaderValue("application/xml"), restRequest.ContentHeaders?.ContentType, "Content type OK");
        }
    }
}