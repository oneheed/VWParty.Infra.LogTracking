using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VWParty.Infra.LogTracking
{
    public interface ILogTrackerLogger
    {
        void Debug(string requestId, string message);
        void Debug(LogMessage item);
        void Info(string requestId, string message);
        void Info(LogMessage item);
        void Warn(string requestId, string message);
        void Warn(LogMessage item);
        void Error(string requestId, string message, Exception exception = null);
        void Error(LogMessage item);
        void Fatal(string requestId, string message, Exception exception = null);
        void Fatal(LogMessage item);
        void Trace(string requestId, string message, Exception exception = null);
        void Trace(LogMessage item);
    }
}
