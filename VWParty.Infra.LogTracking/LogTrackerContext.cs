using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace VWParty.Infra.LogTracking
{
    public enum LogTrackerContextStorageTypeEnum : int
    {
        ASPNET_HTTPCONTEXT,
        OWIN_CONTEXT,   // not supported
        THREAD_DATASLOT,
        NONE
    }

    public class LogTrackerContext
    {
        private LogTrackerContext()
        {

        }

        public const string _KEY_REQUEST_ID = "X-REQUEST-ID";
        public const string _KEY_REQUEST_START_UTCTIME = "X-REQUEST-START-UTCTIME";


        public static LogTrackerContext Create()
        {
            return LogTrackerContext.Init(LogTrackerContextStorageTypeEnum.NONE);
        }

        public static LogTrackerContext Init(LogTrackerContextStorageTypeEnum type)
        {
            return Init(
                type,
                Guid.NewGuid().ToString("N"),
                DateTime.UtcNow);
        }

        public static LogTrackerContext Init(LogTrackerContextStorageTypeEnum type, string requestId, DateTime requestStartTimeUTC)
        {
            if (requestStartTimeUTC.Kind != DateTimeKind.Utc) throw new ArgumentOutOfRangeException("requestStartTimeUTC MUST be UTC time.");

            switch (type)
            {
                case LogTrackerContextStorageTypeEnum.ASPNET_HTTPCONTEXT:

                    HttpContext.Current.Request.Headers.Add(
                        _KEY_REQUEST_ID,
                        requestId);

                    HttpContext.Current.Request.Headers.Add(
                        _KEY_REQUEST_START_UTCTIME,
                        requestStartTimeUTC.ToString("u"));

                    return new LogTrackerContext()
                    {
                        StorageType = LogTrackerContextStorageTypeEnum.ASPNET_HTTPCONTEXT
                    };

                case LogTrackerContextStorageTypeEnum.THREAD_DATASLOT:

                    _thread_static_request_id = requestId;
                    _thread_static_request_start_utctime = requestStartTimeUTC;

                    return new LogTrackerContext()
                    {
                        StorageType = LogTrackerContextStorageTypeEnum.THREAD_DATASLOT
                    };

                case LogTrackerContextStorageTypeEnum.NONE:

                    return new LogTrackerContext()
                    {
                        StorageType = LogTrackerContextStorageTypeEnum.NONE,
                        _local_request_id = requestId,
                        _local_request_start_utctime = requestStartTimeUTC
                    };

                case LogTrackerContextStorageTypeEnum.OWIN_CONTEXT:
                    throw new NotSupportedException();

            }

            throw new NotSupportedException();
        }

        public static void Clean()
        {
            if (LogTrackerContext.Current !=null)
            {
                Clean(LogTrackerContext.Current.StorageType);
            }
        }

        public static void Clean(LogTrackerContextStorageTypeEnum type)
        {
            switch (type)
            {
                case LogTrackerContextStorageTypeEnum.ASPNET_HTTPCONTEXT:

                    HttpContext.Current.Request.Headers.Remove(_KEY_REQUEST_ID);
                    HttpContext.Current.Request.Headers.Remove(_KEY_REQUEST_START_UTCTIME);
                    break;

                case LogTrackerContextStorageTypeEnum.THREAD_DATASLOT:
                    _thread_static_request_id = null;
                    _thread_static_request_start_utctime = DateTime.MinValue;
                    break;

                case LogTrackerContextStorageTypeEnum.OWIN_CONTEXT:
                case LogTrackerContextStorageTypeEnum.NONE:
                    throw new NotSupportedException();

            }
        }

        internal static LogTrackerContext Init()
        {
            throw new NotImplementedException();
        }

        public static LogTrackerContext Current
        {
            get
            {
                if (HttpContext.Current != null && string.IsNullOrEmpty(HttpContext.Current.Request.Headers.Get(_KEY_REQUEST_ID)) == false)
                {
                    // match in httpcontext
                    return new LogTrackerContext()
                    {
                        StorageType = LogTrackerContextStorageTypeEnum.ASPNET_HTTPCONTEXT
                    };
                }

                return null;
            }
        }

        public LogTrackerContextStorageTypeEnum StorageType
        {
            get;
            private set;
        }



        [ThreadStatic]
        private static string _thread_static_request_id = null;

        [ThreadStatic]
        private static DateTime _thread_static_request_start_utctime = DateTime.MinValue;

        private string _local_request_id = null;
        private DateTime _local_request_start_utctime = DateTime.MinValue;

        public string RequestId
        {
            get
            {
                switch (this.StorageType)
                {
                    case LogTrackerContextStorageTypeEnum.ASPNET_HTTPCONTEXT:
                        return HttpContext.Current.Request.Headers.Get(_KEY_REQUEST_ID);

                    case LogTrackerContextStorageTypeEnum.THREAD_DATASLOT:
                        return _thread_static_request_id;

                    case LogTrackerContextStorageTypeEnum.NONE:
                        return this._local_request_id;

                }
                throw new NotSupportedException();
            }
        }

        public DateTime RequestStartTimeUTC
        {
            get
            {
                switch (this.StorageType)
                {
                    case LogTrackerContextStorageTypeEnum.ASPNET_HTTPCONTEXT:
                        DateTimeOffset dto = DateTimeOffset.Parse(HttpContext.Current.Request.Headers.Get(_KEY_REQUEST_START_UTCTIME));
                        return dto.UtcDateTime;

                    case LogTrackerContextStorageTypeEnum.THREAD_DATASLOT:
                        return _thread_static_request_start_utctime;

                    case LogTrackerContextStorageTypeEnum.NONE:
                        return this._local_request_start_utctime;
                }
                throw new NotSupportedException();
            }
        }

        public TimeSpan RequestExecutingTime
        {
            get
            {
                return DateTime.UtcNow - this.RequestStartTimeUTC;
            }
        }
    }
}
