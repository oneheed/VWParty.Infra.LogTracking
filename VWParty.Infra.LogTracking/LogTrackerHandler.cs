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
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (HttpContext.Current != null && string.IsNullOrEmpty(HttpContext.Current.Request.Headers.Get("X-REQUEST-ID")) == false)
            {
                request.Headers.Add("X-REQUEST-ID", HttpContext.Current.Request.Headers.Get("X-REQUEST-ID"));
                request.Headers.Add("X-REQUEST-UTCTIME", HttpContext.Current.Request.Headers.Get("X-REQUEST-UTCTIME"));
            }
            else if (false)
            {
                // check owin context
            }
            else
            {
                request.Headers.Add("X-REQUEST-ID", Guid.NewGuid().ToString());
                request.Headers.Add("X-REQUEST-UTCTIME", DateTime.UtcNow.ToString("s"));
            }

            return base.SendAsync(request, cancellationToken);
        }
    }
}
