#if UNIT_TESTS
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SetUp = Microsoft.VisualStudio.TestTools.UnitTesting.TestInitializeAttribute;
using TearDown = Microsoft.VisualStudio.TestTools.UnitTesting.TestCleanupAttribute;
using Test = Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute;
using TestFixture = Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute;


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