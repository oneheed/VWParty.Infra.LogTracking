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
            //LogTrackerContext.Clean();

            //// Logger WITHOUT logConttext - To GrayLog
            //Console.WriteLine("Logger WITHOUT logConttext - To GrayLog");
            //LogTrackerLogger loggerWOCtxGL = new LogTrackerLogger(LogManager.GetLogger("ToGrayLog"));
            //loggerWOCtxGL.Info(new LogMessage() { Message = "This log has no context information from POC.Client" });
            //Console.ReadLine();

            //// Logger WITH explicit logConttext - To Gray Log
            //string request_id = Guid.NewGuid().ToString();
            //DateTime utcNow = DateTime.UtcNow;
            //LogTrackerContext lgc = LogTrackerContext.Init(LogTrackerContextStorageTypeEnum.NONE, request_id, utcNow);
            //Console.WriteLine("Logger WITH explicit logConttext - To Gray Log");
            //LogTrackerLogger loggerWTHCtxGL = new LogTrackerLogger(LogManager.GetLogger("ToGrayLog"), lgc);
            //loggerWTHCtxGL.Info(new LogMessage() { Message = "This log has context information from POC.Client" });
            //Console.ReadLine();
            //LogTrackerContext.Clean();


            //HttpClient client = new HttpClient();
            HttpClient client = new HttpClient(new LogTrackerHandler());
            client.BaseAddress = new Uri("http://localhost:31554/");
            Console.WriteLine(client.GetAsync("/api/values").Result);
            Console.WriteLine(client.GetAsync("/api/values/123").Result);

            // 測試Init錯誤參數
            LogTrackerContext wrong_request_id = LogTrackerContext.Init(LogTrackerContextStorageTypeEnum.NONE, "", DateTime.UtcNow);
            LogTrackerContext wrong_start_time = LogTrackerContext.Init(LogTrackerContextStorageTypeEnum.NONE, Guid.NewGuid().ToString(), DateTime.MinValue);
            // 以下測試需要將LogTrackerHandler中的request_id或start_time如上改成錯誤參數進行測試
            //HttpClient wrong_http_context = new HttpClient(new LogTrackerHandler());
            //client.BaseAddress = new Uri("http://localhost:31554/");
            //Console.WriteLine(wrong_http_context.GetAsync("/api/values").Result);
            //Console.WriteLine(wrong_http_context.GetAsync("/api/values/123").Result);
        }
    }
}
