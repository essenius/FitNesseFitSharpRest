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
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rest.Model;

namespace RestTests
{
    [TestClass]
    public class InjectorTests
    {
        [TestMethod, TestCategory("Unit")]
        public void InjectorInjectRestSessionTest()
        {
            var session1 = Injector.InjectRestSession(null);
            Assert.IsNull(session1.EndPoint);
            Assert.AreEqual(Encoding.GetEncoding("iso-8859-1"), session1.Context.RequestEncoding);
            var session2 = Injector.InjectRestSession(null);
            Assert.AreNotSame(session1, session2);
            var session3 = Injector.InjectRestSession("http://localhost");
            Assert.AreEqual(new Uri("http://localhost"), session3.EndPoint);
            Assert.AreNotEqual(new Uri("http://localhost"), session1.EndPoint);
        }
    }
}