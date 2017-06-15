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
    [LogTracker(Prefix = "ANDREW")]
    public class ValuesController : ApiController
    {
        //private static Logger _log = LogManager.GetCurrentClassLogger();
        private static LogTrackerLogger _logger = new LogTrackerLogger(LogManager.GetCurrentClassLogger());
        // GET api/values
        [SwaggerOperation("GetAll")]
        public IEnumerable<string> Get()
        {
            _logger.Info(new LogMessage() { Message = "This log comes from POC.WebAPI1.Get()" });

            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        [SwaggerOperation("GetById")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public string Get(int id)
        {
            _logger.Info(new LogMessage() { Message = "This log comes from POC.WebAPI1.Get(id)" });

            HttpClient client = new HttpClient(new LogTrackerHandler(LogTrackerContext.Current));
            client.BaseAddress = new Uri("http://localhost:31604/");
            client.GetAsync("/api/values/867").Wait();

            //_log.Info("[req: {0}] hello! ", LogTrackerContext.Current.RequestId);

            return "value";
        }

        // POST api/values
        [SwaggerOperation("Create")]
        [SwaggerResponse(HttpStatusCode.Created)]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        [SwaggerOperation("Update")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        [SwaggerOperation("Delete")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public void Delete(int id)
        {
        }
    }

    
}
