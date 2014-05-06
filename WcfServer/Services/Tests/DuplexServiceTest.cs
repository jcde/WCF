#if UNIT_TESTS
using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SetUp = Microsoft.VisualStudio.TestTools.UnitTesting.TestInitializeAttribute;
using TearDown = Microsoft.VisualStudio.TestTools.UnitTesting.TestCleanupAttribute;
using Test = Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute;
using TestFixture = Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute;

using WcfDomain.Contracts;
using WcfDomain.Contracts.Clients;

namespace WcfServer.Services.Tests
{
    [TestFixture]
    public class DuplexServiceTest
    {
        [Test]
        public void GetToken()
        {
            Assert.AreEqual(Guid.Empty,
                            DuplexService<IBroadCastContract>.GetToken(new Dictionary<Guid, Connection>(), null));
        }
    }
}

#endif