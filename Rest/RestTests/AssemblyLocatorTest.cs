// Copyright 2023-2024 Rik Essenius
//
//   Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file 
//   except in compliance with the License. You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software distributed under the License 
//   is distributed on an "AS IS" BASIS WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and limitations under the License.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rest.Utilities;

// TODO: eliminate duplication with FixtureExplorer

namespace RestTests
{
    [TestClass]
    public class AssemblyLocatorTest
    {
        [TestMethod, DeploymentItem("config.xml")]
        public void TestAssemblyFoundInCurrentFolder()
        {
            var locator = new AssemblyLocator("Rest.dll", ".");
            Assert.AreEqual("Rest.dll", locator.FindAssemblyPath());
        }

        [TestMethod, DeploymentItem("bogus.xml")]
         // only needed for .NET Framework
        public void TestAssemblyNotFound()
        {
            var locator = new AssemblyLocator("bogus.dll", ".");
            Assert.IsNull(locator.FindAssemblyPath());
        }

        [TestMethod, DeploymentItem("TestConfig.xml")]
         // only needed for .NET Framework
        public void TestAssemblySearchXml()
        {
            var locator = new AssemblyLocator("MyTest2.dll", ".");
            Assert.AreEqual("MyTest2/MyTest2.dll", locator.FindAssemblyPath());
        }
    }
}
