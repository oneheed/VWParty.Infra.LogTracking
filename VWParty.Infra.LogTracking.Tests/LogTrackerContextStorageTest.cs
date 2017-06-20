using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

namespace VWParty.Infra.LogTracking.Tests
{
    [TestClass]
    public class LogTrackerContextStorageTest
    {
        public TestContext TestContext
        {
            get;
            set;
        }



        [TestMethod]
        public void Test_BasicThreadDataSlotStorage()
        {
            var context = LogTrackerContext.Create("UNITTEST", LogTrackerContextStorageTypeEnum.THREAD_DATASLOT);
            Assert.AreEqual(
                context.RequestId,
                LogTrackerContext.Current.RequestId);
            Assert.AreEqual(
                context.RequestStartTimeUTC,
                LogTrackerContext.Current.RequestStartTimeUTC);
        }






        [TestMethod]
        public void Test_ThreadDataSlotStorage_MultiThreads()
        {
            List<Task> tasks = new List<Task>();
            for(int i = 0; i < 10; i++)
            {
                tasks.Add(Task.Run(() => 
                {
                    //LogTrackerContext context = LogTrackerContext.Create("UNITTEST", LogTrackerContextStorageTypeEnum.THREAD_DATASLOT);
                    for (int y = 0; y < 10; y++)
                    {
                        string current_request_id = Guid.NewGuid().ToString();
                        DateTime current_request_time = DateTime.UtcNow;

                        LogTrackerContext context = LogTrackerContext.Init(
                            LogTrackerContextStorageTypeEnum.THREAD_DATASLOT,
                            current_request_id,
                            current_request_time);

                        this.TestContext.WriteLine("TID: {0}, Request-ID: {1}, Request-Time: {2}", Thread.CurrentThread.ManagedThreadId, context.RequestId, context.RequestStartTimeUTC);

                        for (int x = 0; x < 10; x++)
                        {
                            Task.Delay(100).Wait();
                            Assert.AreEqual(
                                current_request_id,
                                LogTrackerContext.Current.RequestId);
                            Assert.AreEqual(
                                current_request_time,
                                LogTrackerContext.Current.RequestStartTimeUTC);
                        }
                    }
                }));
            }

            Task.WaitAll(tasks.ToArray());
        }
    }
}
