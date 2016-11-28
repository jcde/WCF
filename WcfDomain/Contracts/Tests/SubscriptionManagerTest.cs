#if UNIT_TESTS
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
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

            // no GetMethods in ServiceModelEx 3.0.0.0
            int mains = SubscriptionManager<IBroadCastContract>.GetMethods().Length;
            int chatsWithMains = SubscriptionManager<IChatsBroadCastContract>.GetMethods().Length;

            Assert.AreEqual(chatsWithMains - mains, meths.Count);
        }
    }
}

#endif