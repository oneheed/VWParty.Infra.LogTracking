using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace VWParty.Infra.LogTracking
{
    public class LogTrackerHandler : HttpClientHandler
    {
        public LogTrackerHandler()
        {
            this._context = LogTrackerContext.Current ?? LogTrackerContext.Create();
        }

        public LogTrackerHandler(LogTrackerContext context)
        {
            this._context = context;
        }

        private LogTrackerContext _context = null;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            //LogTrackerContext ltc = LogTrackerContext.Current ?? LogTrackerContext.Init();

            request.Headers.Add(
                LogTrackerContext._KEY_REQUEST_ID,
                this._context.RequestId);

            request.Headers.Add(
                LogTrackerContext._KEY_REQUEST_START_UTCTIME,
                this._context.RequestStartTimeUTC.ToString("u"));
            

            return base.SendAsync(request, cancellationToken);
        }
    }
}
