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
            HttpClient client = new HttpClient(new LogTrackerHandler());
            client.BaseAddress = new Uri("http://localhost:31554/");

            Console.WriteLine(client.GetAsync("/api/values/123").Result);
        }
    }
}
