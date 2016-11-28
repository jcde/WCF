#if UNIT_TESTS
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;

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