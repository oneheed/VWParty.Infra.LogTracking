using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using NLog.Fluent;

namespace VWParty.Infra.LogTracking
{
    public class LogTrackerLogger : ILogTrackerLogger
    {
        private readonly ILogger _logger;

        public LogTrackerLogger(ILogger logger)
        {
            _logger = logger;
        }
        [Obsolete]
        public LogTrackerLogger(string configPath)
        {
            _logger = LogManager.GetLogger("");
        }
        public ILogger NLogger
        {
            get { return _logger; }
        }
        private static void SetValues(LogMessage item, LogBuilder builder)
        {
            if (!string.IsNullOrWhiteSpace(item.RequestId))
            {
                builder.Property("request_id", item.RequestId);
            }
            else if (item.logContext != null && !string.IsNullOrWhiteSpace(item.logContext.RequestId))
            {
                builder.Property("request_id", item.logContext.RequestId);
            }
            else if (LogTrackerContext.Current != null && !string.IsNullOrWhiteSpace(LogTrackerContext.Current.RequestId))
            {
                builder.Property("request_id", LogTrackerContext.Current.RequestId);
            }

            if (item.logContext != null)
            {
                builder.Property("request_start_time_utc", item.logContext.RequestStartTimeUTC_Text);
            }
            else if (LogTrackerContext.Current != null && !string.IsNullOrWhiteSpace(LogTrackerContext.Current.RequestId))
            {
                builder.Property("request_start_time_utc", LogTrackerContext.Current.RequestStartTimeUTC_Text);
            }

            if (item.logContext != null)
            {
                builder.Property("request_execute_time_ms", item.logContext.RequestExecutingTime_Text);
            }

            if (!string.IsNullOrWhiteSpace(item.ShortMessage))
            {
                builder.Property("short_message", item.ShortMessage);
            }

            foreach (var data in item.ExtraData)
            {
                var key = data.Key;
                builder.Property(key, data.Value);
            }

            foreach (var customField in item.CustomFields)
            {
                var key = customField.Key;
                builder.Property(key, customField.Value);
            }
        }
        public void Debug(string requestId, string message)
        {
            _logger.Debug()
                .Message(message)
                .Property("request_id", requestId)
                .Write();
        }
        public void Debug(LogMessage item)
        {
            var builder = _logger.Debug()
                .Exception(item.Exception)
                .Message(item.Message);

            SetValues(item, builder);
            builder.Write();
        }
        public void Info(string requestId, string message)
        {
            _logger.Info()
                .Message(message)
                .Property("request_id", requestId)
                .Write();
        }
        public void Info(LogMessage item)
        {
            var builder = _logger.Info()
                .Exception(item.Exception)
                .Message(item.Message);

            SetValues(item, builder);
            builder.Write();
        }
        public void Warn(string requestId, string message)
        {
            _logger.Warn()
                .Message(message)
                .Property("request_id", requestId)
                .Write();
        }
        public void Warn(LogMessage item)
        {
            var builder = _logger.Info()
                .Exception(item.Exception)
                .Message(item.Message);

            SetValues(item, builder);
            builder.Write();
        }
        public void Error(string requestId, string message, Exception exception = null)
        {
            _logger.Error()
                .Message(message)
                .Exception(exception)
                .Property("request_id", requestId)
                .Write();
        }
        public void Error(LogMessage item)
        {
            var builder = _logger.Error()
                .Message(item.Message)
                .Exception(item.Exception);

            SetValues(item, builder);
            builder.Write();
        }
        public void Fatal(string requestId, string message, Exception exception = null)
        {
            _logger.Fatal()
                .Message(message)
                .Exception(exception)
                .Property("request_id", requestId)
                .Write();
        }
        public void Fatal(LogMessage item)
        {
            var builder = _logger.Fatal()
                .Exception(item.Exception)
                .Message(item.Message);

            SetValues(item, builder);
            builder.Write();
        }
        public void Trace(string requestId, string message, Exception exception = null)
        {
            _logger.Fatal()
                .Message(message)
                .Exception(exception)
                .Property("request_id", requestId)
                .Write();
        }
        public void Trace(LogMessage item)
        {
            var builder = _logger.Fatal()
                .Exception(item.Exception)
                .Message(item.Message);

            SetValues(item, builder);
            builder.Write();
        }
    }
}
