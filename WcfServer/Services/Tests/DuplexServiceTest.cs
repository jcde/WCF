#if UNIT_TESTS
using System;
using System.Collections.Generic;
using NUnit.Framework;

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