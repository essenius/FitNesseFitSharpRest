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
using System.Net.Http;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rest.Model;
using Rest.Utilities;

namespace RestTests
{
    [TestClass]
    public class RestRequestTests
    {
        [TestMethod]
        [TestCategory("Unit")]
        public void RestRequestBinaryBodyTest()
        {
            var context = new SessionContext();
            var uri = new Uri("http://localhost/api");
            var factory = new RestRequestFactory();
            var unknownContentRequest = factory.Create(uri, context);
            unknownContentRequest.SetBody("\u0001", HttpMethod.Post);
            Assert.AreEqual("application/octet-stream", unknownContentRequest.ContentHeaders?.ContentType?.MediaType);
        }

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
            target.ExecuteAndFail(HttpMethod.Head);
            Assert.AreEqual("FitNesseRest", FitNesseFormatter.GetHeader(target.Headers, "User-Agent"));
            Assert.IsTrue(FitNesseFormatter.GetHeader(target.Headers, "Accept").Contains("application/json"));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void RestRequestSetBodyForGetTest()
        {
            var context = new SessionContext();
            var uri = new Uri("http://localhost/api");
            var factory = new RestRequestFactory();
            var restRequest = factory.Create(uri, context);
            restRequest.SetBody("hello", HttpMethod.Get);

            var fieldInfo = restRequest.GetType().GetField("_request", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(fieldInfo, "FieldInfo not null");
            var req = fieldInfo.GetValue(restRequest) as HttpRequestMessage;
            Assert.IsNotNull(req, "Request is not null");
            Assert.IsNull(req.Content, "Content is null");
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void RestRequestSupportsBodyTest()
        {
            Assert.IsTrue(RestRequest.SupportsBody(HttpMethod.Put));
            Assert.IsTrue(RestRequest.SupportsBody(HttpMethod.Post));
            Assert.IsFalse(RestRequest.SupportsBody(HttpMethod.Delete));
            Assert.IsFalse(RestRequest.SupportsBody(HttpMethod.Get));
            Assert.IsFalse(RestRequest.SupportsBody(HttpMethod.Head));
        }

    }
}