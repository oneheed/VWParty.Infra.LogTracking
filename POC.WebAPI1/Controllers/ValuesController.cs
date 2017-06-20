using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Swashbuckle.Swagger.Annotations;
using NLog;
using VWParty.Infra.LogTracking;

namespace POC.WebAPI1.Controllers
{
    [LogTracker(Prefix = "POC1")]
    public class ValuesController : ApiController
    {
        private static LogTrackerLogger _logger = new LogTrackerLogger(LogManager.GetCurrentClassLogger());

        // GET api/values
        public IEnumerable<string> Get()
        {
            _logger.Info(new LogMessage() { Message = "This log comes from POC.WebAPI1.Get()" });

            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        public string Get(int id)
        {
            _logger.Info(new LogMessage() { Message = "This log comes from POC.WebAPI1.Get(id)" });

            HttpClient client = new HttpClient(new LogTrackerHandler());
            client.BaseAddress = new Uri("http://localhost:31604/");
            client.GetAsync("/api/values/867").Wait();

            //_log.Info("[req: {0}] hello! ", LogTrackerContext.Current.RequestId);

            return "value";
        }
        
    }

    
}
