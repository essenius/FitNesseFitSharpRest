// Copyright 2023 Rik Essenius
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
using Rest.ContentObjects;

namespace RestTests
{
    [TestClass]
    public class BinaryObjectTests
    {
        private static void AssertNotImplementedException(Action action)
        {
            try
            {
                action();
                Assert.Fail("No NotImplementedException thrown");
            }
            catch (NotImplementedException)
            {
                // expected
            }
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void BinaryObjectTest()
        {
            var objectUnderTest = new BinaryObject("test");
            const string bogus = "bogus";
            Assert.AreEqual("test", objectUnderTest.Serialize());
            Assert.AreEqual("Binary Object", objectUnderTest.ToString());
            Assert.AreEqual(ContentType.Unknown, objectUnderTest.ContentType);
            AssertNotImplementedException(() => objectUnderTest.AddAt(null, bogus));
            AssertNotImplementedException(() => objectUnderTest.Delete(bogus));
            AssertNotImplementedException(() => objectUnderTest.GetProperty(bogus));
            AssertNotImplementedException(() => objectUnderTest.Evaluate(bogus));
            AssertNotImplementedException(() => objectUnderTest.GetProperties(bogus));
            AssertNotImplementedException(() => objectUnderTest.GetPropertyType(bogus));
            AssertNotImplementedException(() => objectUnderTest.SerializeProperty(bogus));
            AssertNotImplementedException(() => objectUnderTest.SetProperty(bogus, bogus));
        }
    }
}