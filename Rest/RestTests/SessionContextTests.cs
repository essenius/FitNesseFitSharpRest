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
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rest.Model;
using Rest.Utilities;

namespace RestTests
{
    [TestClass]
    public class SessionContextTests
    {
        [TestMethod]
        [TestCategory("Unit")]
        public void SessionContextAddHeaderTest()
        {
            var context = new SessionContext();
            context.SetConfig("Headers", "header3:value3\r\nheader4:value4");
            var client = context.Client;
            var headers = client.DefaultRequestHeaders;
            Assert.AreEqual(2, headers.Count());
            Assert.AreEqual("value3", FitNesseFormatter.GetHeader(headers, "header3"));
            Assert.AreEqual("value4", FitNesseFormatter.GetHeader(headers, "header4"));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void SessionContextContentTypeMapperTest()
        {
            var context = new SessionContext();
            Assert.IsNull(context.ContentType(null));
            Assert.AreEqual("json", context.ContentType("application/json"), "default json");
            Assert.AreEqual("xml", context.ContentType("application/xml"), "default xml");
            Assert.AreEqual("text", context.ContentType("text/plain"), "default text");
            Assert.IsNull(context.ContentType("unknown/content"), "unknown content type");
            Assert.AreEqual("xml", context.ContentType("application/atom+xml"), "atom is an XML variant");
            Assert.AreEqual("unknown", context.ContentType("default"), "default is unknown");
            Assert.IsNull(context.ContentType("application/rss+xml"), "rss is not handled by default");
            context.SetConfig("ContentTypeMapping", "application/rss+xml : xml");
            Assert.AreEqual("xml", context.ContentType("application/rss+xml"),
                "atom returns xml after config update");
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void SessionContextCookieTest()
        {
            var context = new SessionContext();
            var uri = new Uri("http://localhost");
            context.SetConfig("Cookies", "cookie1=value1\r\ncookie2=value2");
            var message = new HttpRequestMessage { RequestUri = uri };
            context.SetCookies(message);
            var cookieContainer = context.CookieContainer;
            Assert.IsNotNull(cookieContainer);
            Assert.AreEqual(2, cookieContainer.Count);
            var collection = cookieContainer.GetCookies(uri);
            Assert.AreEqual(2, collection.Count);
            Assert.AreEqual("value1", collection["cookie1"]?.Value);
            Assert.AreEqual("value2", collection["cookie2"]?.Value);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void SessionContextProxyTest()
        {
            var context = new SessionContext();
            //Use reflection here to get/set private fields and invoke the private methods.
            var fieldInfo = context.GetType().GetProperty("Proxy", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(fieldInfo, "fieldInfo not null");
            var iProxy = fieldInfo.GetValue(context) as IWebProxy;
            Assert.IsNotNull(iProxy);
            var proxyAddress = iProxy.GetProxy(new Uri("http://www.google.com"));

            // override the proxy and check if it was set
            context.SetConfig("Proxy", "http://localhost:8888");
            var proxy = fieldInfo.GetValue(context) as WebProxy;
            Assert.IsNotNull(proxy);
            Assert.AreEqual(new Uri("http://localhost:8888/"), proxy.Address);

            //remove the proxy and see if it is indeed gone
            context.SetConfig("Proxy", "None");
            proxy = fieldInfo.GetValue(context) as WebProxy;
            Assert.IsNotNull(proxy);
            Assert.IsNull(proxy.Address);

            //Restore the system proxy and check if we can get the proxy address for google again
            context.SetConfig("Proxy", "System");
            iProxy = fieldInfo.GetValue(context) as IWebProxy;
            Assert.IsNotNull(iProxy);
            Assert.AreEqual(proxyAddress, iProxy.GetProxy(new Uri("http://www.google.com")));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void SessionContextSecurityProtocolTest()
        {
            Assert.AreEqual("SystemDefault", SessionContext.SecurityProtocol);
        }
    }
}