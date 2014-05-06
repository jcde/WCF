#if UNIT_TESTS
using System.Collections.Generic;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SetUp = Microsoft.VisualStudio.TestTools.UnitTesting.TestInitializeAttribute;
using TearDown = Microsoft.VisualStudio.TestTools.UnitTesting.TestCleanupAttribute;
using Test = Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute;
using TestFixture = Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute;

using ServiceModelEx;
using WcfDomain.Contracts.Chats;

namespace WcfDomain.Contracts.Tests
{
    [TestFixture]
    public class SubscriptionManagerTest
    {
        [Test]
        public void GetOperations()
        {
            var meths = new List<MethodInfo>(
                typeof (IChatsBroadCastContract).GetMethods(BindingFlags.Public | BindingFlags.FlattenHierarchy |
                                                            BindingFlags.Instance));

            int mains = SubscriptionManager<IBroadCastContract>.GetMethods().Count;
            int chatsWithMains = SubscriptionManager<IChatsBroadCastContract>.GetMethods().Count;

            Assert.AreEqual(chatsWithMains - mains, meths.Count);
        }
    }
}

#endif