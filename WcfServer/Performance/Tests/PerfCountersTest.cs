#if UNIT_TESTS
using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SetUp = Microsoft.VisualStudio.TestTools.UnitTesting.TestInitializeAttribute;
using TearDown = Microsoft.VisualStudio.TestTools.UnitTesting.TestCleanupAttribute;
using Test = Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute;
using TestFixture = Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute;

namespace WcfServer.Performance.Tests
{
    [TestFixture]
    public class PerfCountersTest
    {
        private PerfCounters counters;

        [TearDown]
        public void FixtureDown()
        {
            // for FixtureTearDown             counters.Dispose();
        }

        [SetUp]
        public void Setup()
        {
            if (counters == null)
                counters = new PerfCounters("test");
            else
                counters.Clear();
        }

        [Test]
        public void Count()
        {
            counters.Count(PerfCounters.Connections, 3);
            Assert.AreEqual(3, counters.Sample(PerfCounters.Connections));
        }

        [Test]
        public void Messages2ServerPerSecond()
        {
            counters.Count(PerfCounters.Messages2ServerPerSecond);
            counters.Count(PerfCounters.Messages2ServerPerSecond);
            counters.Count(PerfCounters.Messages2ServerPerSecond);
            counters.Count(PerfCounters.Messages2ServerPerSecond);
            Thread.Sleep(1000);
            double round = Math.Round(counters.Sample(PerfCounters.Messages2ServerPerSecond));
            Assert.IsTrue(round >= 3 && round <= 4);
        }

        [Test]
        public void PerfDataMaxMin()
        {
            PerfData c = counters._counters[PerfCounters.Connections];
            Assert.IsNull(c.Max);
            Assert.IsNull(c.Min);
            counters.Count(PerfCounters.Connections, 3);
            counters.Count(PerfCounters.Connections, 2);
            counters.Count(PerfCounters.Connections, 6);
            Assert.AreEqual(6, c.Max);
            Assert.AreEqual(2, c.Min);
        }
    }
}

#endif