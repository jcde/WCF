#if UNIT_TESTS
using System.Collections;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SetUp = Microsoft.VisualStudio.TestTools.UnitTesting.TestInitializeAttribute;
using TearDown = Microsoft.VisualStudio.TestTools.UnitTesting.TestCleanupAttribute;
using Test = Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute;
using TestFixture = Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute;


namespace WcfDomain.Contracts.Clients.Tests
{
    [TestFixture]
    public class ConnectionTest
    {
        [Test]
        public void Equals()
        {
            var clients = new ArrayList {new Connection {ClientUniqueKey = "1"}};
            Assert.IsTrue(clients.Contains("1"));

            var items = new List<object> {new Connection {ClientUniqueKey = "2"}};
            Assert.IsTrue(items.Contains("2"));

            var list = new List<Connection> {new Connection {ClientUniqueKey = "3"}};
            Assert.IsFalse(((IList) list).Contains("3"));
        }
    }
}

#endif