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

using System;
using System.Net;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rest.Model;

namespace RestTests
{
    [TestClass]
    public class SessionContextTests
    {
        [TestMethod, TestCategory("Unit")]
        public void SessionContextContentTypeMapperTest()
        {
            var context = new SessionContext();
            Assert.AreEqual("json", context.ContentHandler("application/json"), "default json");
            Assert.AreEqual("xml", context.ContentHandler("application/xml"), "default xml");
            Assert.AreEqual("text", context.ContentHandler("text/plain"), "default text");
            Assert.AreEqual("json", context.ContentHandler("unknown/content"), "unknown content type");
            Assert.AreEqual("json", context.ContentHandler("application/atom+xml"), "atom not set by default");
            context.SetConfig("ContentTypeMapping", "application/atom+xml : xml");
            Assert.AreEqual("xml", context.ContentHandler("application/atom+xml"), "atom returns xml after config update");
        }

        [TestMethod, TestCategory("Unit")]
        public void SessionContextProxyTest()
        {
            var context = new SessionContext();
            //Use reflection here to get/set private fields and invoke the private methods.
            var fieldInfo = context.GetType().GetProperty("Proxy", BindingFlags.NonPublic | BindingFlags.Instance);
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

        [TestMethod, TestCategory("Unit")]
        public void SessionContextCookieTest()
        {
            var context = new SessionContext();
            var uri = new Uri("http://localhost");
            context.SetConfig("CookieDomain", "localhost");
            context.SetConfig("Cookies", "cookie1=value1\r\ncookie2=value2");
            var request = WebRequest.Create(uri) as HttpWebRequest;
            Assert.IsNotNull(request);
            context.SetDefaults(request);
            var cookieContainer = request.CookieContainer;
            Assert.IsNotNull(cookieContainer);
            Assert.AreEqual(2, cookieContainer.Count);
            var collection = cookieContainer.GetCookies(uri);
            Assert.AreEqual(2, collection.Count);
            Assert.AreEqual("value1", collection["cookie1"]?.Value);
            Assert.AreEqual("value2", collection["cookie2"]?.Value);
        }

        [TestMethod, TestCategory("Unit")]
        public void SessionContextSecurityProtocolTest()
        {
            var context = new SessionContext();
            Assert.IsTrue(context.SecurityProtocol.Contains("Tls12"));
            context.SecurityProtocol = "SystemDefault";
            Assert.IsFalse(context.SecurityProtocol.Contains("Tls12"));
        }

        [TestMethod, TestCategory("Unit"), ExpectedException(typeof(ArgumentException))]
        public void SessionContextCookieWithoutDomainTest()
        {
            var context = new SessionContext();
            context.SetConfig("Cookies", "cookie1=value1");
        }

    }
}