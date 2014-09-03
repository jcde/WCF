#if UNIT_TESTS
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WcfClient.Properties;
using Test = Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute;
using TestFixture = Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute;

namespace WcfClient.Tests
{
    [TestFixture]
    public class ClientInstanceTest
    {
        [Test]
        public void ClientSpecificMode()
        {
            Settings.ClientSpecificMode = true;
            Assert.AreEqual("localhost", Settings.Default.Server);
        }
    }
}

#endif