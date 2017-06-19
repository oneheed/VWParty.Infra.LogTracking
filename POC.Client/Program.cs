using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using VWParty.Infra.LogTracking;

namespace POC.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            LogTrackerContext.Clean();
            // Logger WITHOUT logConttext - To GrayLog
            Console.WriteLine("Logger WITHOUT logConttext - To GrayLog");
            LogTrackerLogger loggerWOCtxGL = new LogTrackerLogger(LogManager.GetLogger("ToGrayLog"));
            loggerWOCtxGL.Info(new LogMessage() { Message = "This log has no context information from POC.Client" });
            Console.ReadLine();

            //// Logger WITH explicit logConttext - To Gray Log
            //string request_id = Guid.NewGuid().ToString();
            //DateTime utcNow = DateTime.UtcNow;
            //LogTrackerContext lgc = LogTrackerContext.Init(LogTrackerContextStorageTypeEnum.NONE, request_id, utcNow);
            //Console.WriteLine("Logger WITH explicit logConttext - To Gray Log");
            //LogTrackerLogger loggerWTHCtxGL = new LogTrackerLogger(LogManager.GetLogger("ToGrayLog"), lgc);
            //loggerWTHCtxGL.Info(new LogMessage() { Message = "This log has context information from POC.Client" });
            //Console.ReadLine();

            LogTrackerContext.Clean();
            HttpClient client = new HttpClient(new LogTrackerHandler());
            //HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("http://localhost:31554/");

            Console.WriteLine(client.GetAsync("/api/values/123").Result);

            Console.ReadLine();

        }
    }
}
