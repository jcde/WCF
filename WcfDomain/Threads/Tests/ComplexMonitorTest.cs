#if UNIT_TESTS
using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SetUp = Microsoft.VisualStudio.TestTools.UnitTesting.TestInitializeAttribute;
using TearDown = Microsoft.VisualStudio.TestTools.UnitTesting.TestCleanupAttribute;
using Test = Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute;
using TestFixture = Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute;

using WcfDomain.Contracts.Chats;
using WcfDomain.Contracts.Clients;

namespace WcfDomain.Threads.Tests
{
    [TestFixture]
    public class ComplexMonitorTest
    {
        private static void AddActiveUserInParallel(ChatRoom room)
        {
            Console.Write(Guid.Empty);
            ThreadPool.QueueUserWorkItem(state =>
                                             {
                                                 lock (room.ActiveClients)
                                                     room.ActiveClients.Add(new Connection {ClientUniqueKey = "1"});
                                             });
        }

        [Test]
        public void CopyFrom_ChatRoomDictionary()
        {
            var r = new ChatRoom
                        {
                            ActiveClients = new List<Connection>
                                                {
                                                    new Connection {ClientUniqueKey = "1"}
                                                }
                        };
            var oldl = new Dictionary<int, ChatRoom> {{0, r}};
            var newl = (List<ChatRoom>) ComplexMonitor.CopyFrom(oldl.Values);

            Assert.AreEqual(newl.Count, oldl.Count);
            Assert.AreNotEqual(newl, oldl);
            Assert.AreNotSame(newl[0].ActiveClients, oldl[0].ActiveClients);
            Assert.AreEqual(newl[0].ActiveClients.Count, oldl[0].ActiveClients.Count);
            Assert.AreEqual(newl[0].ActiveClients[0], oldl[0].ActiveClients[0]);
            Assert.AreEqual(newl[0].Name, oldl[0].Name);
        }

        [Test]
        public void CopyFrom_List()
        {
            int? intref = 7;
            var oldl = new List<int?> {1, 2, intref};
            var newl = (List<int?>) ComplexMonitor.CopyFrom(oldl);

            Assert.AreEqual(newl.Count, oldl.Count);
            Assert.AreNotSame(newl, oldl);
            Assert.AreEqual(newl[2], intref);
        }

        [Test]
        public void GetProps()
        {
            Assert.AreEqual(4, ComplexMonitor.GetProps(typeof (ChatRoom)).Count);
        }

        /// <summary>
        /// object lock by ComplexMonitor WILL lock its properties 
        /// </summary>
        [Test]
        public void PropertiesComplexMonitorLock()
        {
            var room = new ChatRoom();
            var m = new ComplexMonitor(room);
            using (m)
            {
                AddActiveUserInParallel(room);
                Thread.Sleep(500);
                room.ActiveClients.Add(
                    new Connection {ClientUniqueKey = "2"});
                Assert.AreEqual(1, room.ActiveClients.Count);
            }
            Assert.IsNull(m._LockedObj);
        }

        /// <summary>
        /// object lock will NOT lock its properties 
        /// </summary>
        [Test]
        public void PropertiesLock()
        {
            var room = new ChatRoom();
            lock (room)
            {
                AddActiveUserInParallel(room);
                Thread.Sleep(500);
                room.ActiveClients.Add(
                    new Connection {ClientUniqueKey = "2"});
                Assert.AreEqual(2, room.ActiveClients.Count);
            }
        }
    }
}

#endif