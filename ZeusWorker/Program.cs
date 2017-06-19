using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VWParty.Infra.LogTracking;

namespace ZeusWorker
{
    class Program
    {
        static void Main(string[] args)
        {
            LogTrackerLogger logger = new LogTrackerLogger(LogManager.GetCurrentClassLogger());
            Guid guid1 = Guid.NewGuid();
            DateTime dt1 = DateTime.UtcNow;
            Console.WriteLine(String.Format("Worker: request_id1: {0} | request_start_time_utc: {1}", guid1.ToString(), dt1));
            LogTrackerContext.Init(LogTrackerContextStorageTypeEnum.THREAD_DATASLOT, guid1.ToString(), dt1);
            MockZeus.Service.Mercury.TP.GBTP gb1 = new MockZeus.Service.Mercury.TP.GBTP();
            gb1.GetBalance();

            System.Threading.Thread.Sleep(1000);

            Guid guid2 = Guid.NewGuid();
            DateTime dt2 = DateTime.UtcNow;
            Console.WriteLine(String.Format("Worker: request_id1: {0} | request_start_time_utc: {1}", guid2.ToString(), dt2));
            LogTrackerContext.Init(LogTrackerContextStorageTypeEnum.THREAD_DATASLOT, guid2.ToString(), dt2);
            MockZeus.Service.Mercury.TP.GBTP gb2 = new MockZeus.Service.Mercury.TP.GBTP();
            gb2.GetBalance();

            Console.ReadLine();
        }
    }
}
