using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VWParty.Infra.LogTracking
{
    public static class LoggerExtension
    {
        public static LogTrackerLogger ToLogTrackerLogger(this ILogger source)
        {
            return new LogTrackerLogger(source);
        }
    }
}
