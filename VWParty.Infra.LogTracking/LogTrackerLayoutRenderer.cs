using NLog.LayoutRenderers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace VWParty.Infra.LogTracking
{
    [LayoutRenderer("vwparty-request-id")]
    public class RequestIdLayoutRenderer : LayoutRenderer
    {
        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            if (LogTrackerContext.Current != null)
            {
                builder.Append(LogTrackerContext.Current.RequestId);
            }
        }
    }

    [LayoutRenderer("vwparty-request-time")]
    public class RequestStartTimeLayoutRender : LayoutRenderer
    {
        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            if (LogTrackerContext.Current != null)
            {
                builder.Append(LogTrackerContext.Current.RequestStartTimeUTC);
            }
        }
    }

    [LayoutRenderer("vwparty-request-execute")]
    public class RequestExecuteTimeLayoutRender : LayoutRenderer
    {
        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            if (LogTrackerContext.Current != null)
            {
                builder.Append(LogTrackerContext.Current.RequestExecutingTime);
            }
        }
    }
}
