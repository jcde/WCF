#if UNIT_TESTS
using NUnit.Framework;

namespace WcfDomain.Contracts.Tests
{
    [TestFixture]
    public class UniquePropertyNotifierTest
    {
        [Test]
        public void GetApplicationID()
        {
            var u = new UniquePropertyNotifier("app");
            Assert.AreEqual("app", UniquePropertyNotifier.GetApplicationID(u.UniqueKey));
        }

        [Test]
        public void GetComputerName()
        {
            var u = new UniquePropertyNotifier("app");
            Assert.AreEqual(u.User.ComputerName, UniquePropertyNotifier.GetComputerName(u.UniqueKey));
        }
    }
}
#endif