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
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rest.ContentObjects;
using Rest.Model;

namespace RestTests
{
    [TestClass, SuppressMessage("ReSharper", "UnusedVariable", Justification = "Forcing exception")]
    public class ContentObjectFactoryTests
    {
        [TestMethod, TestCategory("Unit"), ExpectedException(typeof(ArgumentNullException))]
        public void ContentObjectFactoryCreateNullTest()
        {
            var a = new ContentObjectFactory(null).Create(null, 1);
        }

        [TestMethod, TestCategory("Unit"), ExpectedException(typeof(ArgumentException))]
        public void ContentObjectFactoryCreateWrongTypeTest()
        {
            var a = new ContentObjectFactory(new SessionContext()).Create("cs", 1);
        } 
    }
}