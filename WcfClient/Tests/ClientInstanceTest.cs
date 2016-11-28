#if UNIT_TESTS
using NUnit.Framework;
using WcfClient.Properties;

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