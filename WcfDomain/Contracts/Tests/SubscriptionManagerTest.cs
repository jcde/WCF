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

            int mains = SubscriptionManager<IBroadCastContract>.GetMethods().Count;
            int chatsWithMains = SubscriptionManager<IChatsBroadCastContract>.GetMethods().Count;

            Assert.AreEqual(chatsWithMains - mains, meths.Count);
        }
    }
}

#endif