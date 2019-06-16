using Rest;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RestTests
{
    [TestClass]
    public class DocumentationAttributeTest
    {
        [TestMethod, TestCategory("Unit")]
        public void DocumentationAttributeTest1()
        {
            Assert.AreEqual("test", new DocumentationAttribute("test").Message);
        }

    }
}
