using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using NLog;
using System.Diagnostics;

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
        /// <summary>
        /// prevent user to create LogTrackerContext itself.
        /// </summary>
        private LogTrackerContext()
        {

        }

        public const string _KEY_REQUEST_ID = "X-REQUEST-ID";
        public const string _KEY_REQUEST_START_UTCTIME = "X-REQUEST-START-UTCTIME";


        [Obsolete("請提供明確的 request-id prefix")]
        public static LogTrackerContext Create()
        {
            return Init(
                LogTrackerContextStorageTypeEnum.NONE,
                Guid.NewGuid().ToString(),
                DateTime.UtcNow);
        }

        /// <summary>
        /// 建立 LogTrackerContext. 會產生一組新的 Request-ID 與 Request-Start-UTCTime, 用來識別與追蹤這一串任務的相關日誌。
        /// 不指定 Storage Type 的情況下，這個 LogTrackerContext 將不會儲存在任何環境下，呼叫端必須明確地將這個物件傳遞下去。
        /// </summary>
        /// <remarks>
        /// Prefix 列表:
        /// - HC:   HttpClient
        /// - GW:   API Gateway
        /// - TEMP: 暫時使用 (ASP.NET MVC Filter)
        /// </remarks>
        /// <param name="prefix"></param>
        /// <returns></returns>
        public static LogTrackerContext Create(string prefix)
        {
            return Init(
                LogTrackerContextStorageTypeEnum.NONE,
                string.Format("{0}-{1:N}", prefix, Guid.NewGuid()).ToUpper(),
                //string.Format("{0}-{1}", prefix, Guid.NewGuid()),
                DateTime.UtcNow);
        }

        [Obsolete("請提供明確的 request-id prefix")]
        public static LogTrackerContext Create(LogTrackerContextStorageTypeEnum type)
        {
            return Init(
                type,
                Guid.NewGuid().ToString(),
                DateTime.UtcNow);
        }

        /// <summary>
        /// 建立 LogTrackerContext. 會產生一組新的 Request-ID 與 Request-Start-UTCTime, 用來識別與追蹤這一串任務的相關日誌。
        /// 建立好的 LogTrackerContext 關鍵資訊會自動儲存在指定的 Storage, 不需要明確的傳遞下去。只要執行時還維持在同樣的
        /// 執行環境 (context)，呼叫 LogTrackerContext.Current 即可取回先前 Create 的 Context 內容。
        /// </summary>
        /// <param name="prefix"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static LogTrackerContext Create(string prefix, LogTrackerContextStorageTypeEnum type)
        {
            return Init(
                type,
                string.Format("{0}-{1:N}", prefix, Guid.NewGuid()).ToUpper(),
                DateTime.UtcNow);
        }


        [Obsolete("請改用 Create(), Init 只做串接 context 的內容，不再負責起始新的 context 內容 (request id / time)。", true)]
        public static LogTrackerContext Init(LogTrackerContextStorageTypeEnum type)
        {
            return Init(
                type,
                Guid.NewGuid().ToString(),
                DateTime.UtcNow);
        }


        /// <summary>
        /// 從既有的 context 物件來初始化 LogTrackerContext。
        /// 只在串接上一關傳遞過來的 context 時使用，不會產生新的 Request-ID 跟 Request-Start-UTCTime。
        /// 若有需要產生新的 context, 請呼叫 Create( )
        /// </summary>
        /// <param name="type"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public static LogTrackerContext Init(LogTrackerContextStorageTypeEnum type, LogTrackerContext context)
        {
            if (context == null)
            {
                Trace.WriteLine(String.Format("{0} | parameter: context can not be NULL.",
                    DateTime.UtcNow.ToString("yyyy-MM-ddThh:mm:ss.fffZ")));
#if DEBUG
                throw new ArgumentNullException("parameter: context can not be NULL.");
#endif
                return null;
            }

            return Init(
                type,
                context.RequestId,
                context.RequestStartTimeUTC);
        }

        /// <summary>
        /// 從既有的 context 關鍵資訊 (request-id, request-start-utctime) 來初始化 LogTrackerContext。
        /// 只在串接上一關傳遞過來的 context 時使用，不會產生新的 Request-ID 跟 Request-Start-UTCTime。
        /// 若有需要產生新的 context, 請呼叫 Create( )
        /// </summary>
        /// <param name="type"></param>
        /// <param name="requestId"></param>
        /// <param name="requestStartTimeUTC"></param>
        /// <returns></returns>
        public static LogTrackerContext Init(LogTrackerContextStorageTypeEnum type, string requestId, DateTime requestStartTimeUTC)
        {
            if (String.IsNullOrEmpty(requestId) || String.IsNullOrWhiteSpace(requestId))
            {
                Trace.WriteLine(String.Format("{0} | LogTrackerContext Init fail | RequestId MUST NOT be null or empty or white space only.",
                    DateTime.UtcNow.ToString("yyyy-MM-ddThh:mm:ss.fffZ")));
#if DEBUG
                throw new ArgumentOutOfRangeException("RequestId MUST NOT be null or empty or white space only.");
#endif
                return null;
            }
            if (requestStartTimeUTC.Kind != DateTimeKind.Utc)
            {
                Trace.WriteLine(String.Format("{0} | LogTrackerContext Init fail | requestStartTimeUTC MUST be UTC time.",
                    DateTime.UtcNow.ToString("yyyy-MM-ddThh:mm:ss.fffZ")));
#if DEBUG
                throw new ArgumentOutOfRangeException("requestStartTimeUTC MUST be UTC time.");
#endif
                return null;
            }

            switch (type)
            {
                case LogTrackerContextStorageTypeEnum.ASPNET_HTTPCONTEXT:

                    HttpContext.Current.Request.Headers[_KEY_REQUEST_ID] = requestId;
                    HttpContext.Current.Request.Headers[_KEY_REQUEST_START_UTCTIME] = requestStartTimeUTC.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'");

                    return Current;

                case LogTrackerContextStorageTypeEnum.THREAD_DATASLOT:
                    _thread_static_is_set = true;
                    _thread_static_request_id = requestId;
                    _thread_static_request_start_utctime = requestStartTimeUTC;

                    return Current;

                case LogTrackerContextStorageTypeEnum.NONE:

                    return new LogTrackerContext()
                    {
                        StorageType = LogTrackerContextStorageTypeEnum.NONE,
                        RequestId = requestId,
                        RequestStartTimeUTC = requestStartTimeUTC
                    };

                case LogTrackerContextStorageTypeEnum.OWIN_CONTEXT:
                    throw new NotSupportedException();

            }

            throw new NotSupportedException();
        }

        /// <summary>
        /// 清除目前的 context 內容，清除過後 .current 就無法再取得 context 物件
        /// </summary>
        public static void Clean()
        {
            //if (LogTrackerContext.Current != null)
            if (LogTrackerContext.Current != null)
            {
                Clean(LogTrackerContext.Current.StorageType);
            }
        }

        /// <summary>
        /// 清儲存在指定 storage type 的 log context 資訊
        /// </summary>
        /// <param name="type"></param>
        public static void Clean(LogTrackerContextStorageTypeEnum type)
        {
            switch (type)
            {
                case LogTrackerContextStorageTypeEnum.ASPNET_HTTPCONTEXT:

                    HttpContext.Current.Request.Headers.Remove(_KEY_REQUEST_ID);
                    HttpContext.Current.Request.Headers.Remove(_KEY_REQUEST_START_UTCTIME);
                    break;

                case LogTrackerContextStorageTypeEnum.THREAD_DATASLOT:
                    _thread_static_is_set = false;
                    _thread_static_request_id = null;
                    _thread_static_request_start_utctime = DateTime.MinValue;
                    break;

                case LogTrackerContextStorageTypeEnum.OWIN_CONTEXT:
                case LogTrackerContextStorageTypeEnum.NONE:
                    throw new NotSupportedException();

            }
        }

        /// <summary>
        /// 取得目前作用中的 log context, 會依序搜尋下列 storage:
        /// 1. asp.net web hosting environment (http context)
        /// 2. asp.net owin context (not implement)
        /// 3. .net thread data slot
        /// 若無符合的內容，則會直接 return null;
        /// Exception: ArgumentNullException, FormatException
        /// </summary>
        public static LogTrackerContext Current
        {
            get
            {
                HttpContext _context = null;
                HttpRequest _request = null;
                try
                {
                    _context = HttpContext.Current;
                    _request = HttpContext.Current.Request;
                }
                catch(Exception ex)
                {
                    // in app_start or app_end event handler
                    if(ex is System.Web.HttpException || ex is System.TypeInitializationException)
                    return null;
                }

                if (_context != null && string.IsNullOrEmpty(_context.Request.Headers.Get(_KEY_REQUEST_ID)) == false)
                {
                    // RequestStartTimeUTC Parse過程可能發生Exception: DateTime格式不合(ArgumentNullException, FormatException)
                    // Production環境回傳null，Develop環境則直接丟Exception;兩者均會先寫出Trace log
                    try
                    {
                        return new LogTrackerContext()
                        {
                            StorageType = LogTrackerContextStorageTypeEnum.ASPNET_HTTPCONTEXT,
                            RequestId = _context.Request.Headers.Get(_KEY_REQUEST_ID),
                            RequestStartTimeUTC = DateTime.Parse(_context.Request.Headers.Get(_KEY_REQUEST_START_UTCTIME)).ToUniversalTime()
                        };
                    }
                    catch(ArgumentNullException ex)
                    {
                        Trace.WriteLine(String.Format("LogTrackerContext.Current Exception: {0}", ex.Message));
#if DEBUG
                        throw ex;
#endif
                        return null;
                    }
                    catch(FormatException ex)
                    {
                        Trace.WriteLine(String.Format("LogTrackerContext.Current Exception: {0}", ex.Message));
#if DEBUG
                        throw ex;
#endif
                        return null;
                    }
                }
                else if (false) // TODO: check OWIN environment
                {
                    return new LogTrackerContext()
                    {
                        StorageType = LogTrackerContextStorageTypeEnum.OWIN_CONTEXT,
                        RequestId = null,
                        RequestStartTimeUTC = DateTime.MinValue
                        //RequestStartTimeUTC = DateTime.Parse("0001-01-01T00:00:00.000Z").ToUniversalTime()
                    };
                }
                else if (_thread_static_is_set == true) // check thread environment
                {
                    return new LogTrackerContext()
                    {
                        StorageType = LogTrackerContextStorageTypeEnum.THREAD_DATASLOT,
                        RequestId = _thread_static_request_id,
                        RequestStartTimeUTC = _thread_static_request_start_utctime
                    };
                }

                return null;
            }
        }

        /// <summary>
        /// 儲存 log context 關鍵資訊的方式
        /// </summary>
        public LogTrackerContextStorageTypeEnum StorageType
        {
            get;
            private set;
        }


        [ThreadStatic]
        private static bool _thread_static_is_set = false;

        [ThreadStatic]
        private static string _thread_static_request_id = null;

        [ThreadStatic]
        private static DateTime _thread_static_request_start_utctime = DateTime.MinValue;
        //private static DateTime _thread_static_request_start_utctime = DateTime.Parse("0001-01-01T00:00:00.000Z").ToUniversalTime();

        //private string _local_request_id = null;
        //private DateTime _local_request_start_utctime = DateTime.MinValue;


        /// <summary>
        /// Request-ID
        /// </summary>
        public string RequestId
        {
            get;
            private set;
        }

        /// <summary>
        /// Request-Start-UTCTime
        /// </summary>
        public DateTime RequestStartTimeUTC
        {
            get;
            private set;
        }

        /// <summary>
        /// 格式化的 RequestStartTimeUTC 內容
        /// </summary>
        //[Obsolete]
        public string RequestStartTimeUTC_Text
        {
            get
            {
                return this.RequestStartTimeUTC.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'");
            }
        }


        /// <summary>
        /// Request-Executing-Time, 及時計算
        /// </summary>
        public TimeSpan RequestExecutingTime
        {
            get
            {
                return DateTime.UtcNow - this.RequestStartTimeUTC;
            }
        }

        /// <summary>
        /// 格式化的 RequestExecutingTime 內容
        /// </summary>
        //[Obsolete]
        public string RequestExecutingTime_Text
        {
            get
            {
                return (DateTime.UtcNow - this.RequestStartTimeUTC).TotalMilliseconds.ToString("000000.000");
            }
        }
    }
}
