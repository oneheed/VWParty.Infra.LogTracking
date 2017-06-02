using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace VWParty.Infra.LogTracking
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class LogTrackerAttribute : ActionFilterAttribute
    {
        public bool IsCreateRequestId { get { return true; } }

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            Logger _log = LogManager.GetLogger("LogTracker", actionContext.ControllerContext.Controller.GetType());

            //if (actionContext.Request.Headers.Contains("X-REQUEST-ID") == false)
            //{
            //    _log.Info("create req id");
            //    actionContext.Request.Headers.Add("X-REQUEST-ID", Guid.NewGuid().ToString());
            //    actionContext.Request.Headers.Add("X-REQUEST-UTCTIME", DateTime.UtcNow.ToString("u"));
            //}
            if (LogTrackerContext.Current == null)
            {
                _log.Info("create req id");
                LogTrackerContext.Init(LogTrackerContextStorageTypeEnum.ASPNET_HTTPCONTEXT);
            }

            _log.Info(
                "before call (req id: {0}, utctime: {1}, execute-ms: {2})",
                LogTrackerContext.Current.RequestId,
                LogTrackerContext.Current.RequestStartTimeUTC,
                LogTrackerContext.Current.RequestExecutingTime.TotalMilliseconds);

            _log.Info("-----------------------------------------------------------------------");
            foreach (var header in actionContext.Request.Headers)
            {
                _log.Info("- header [{0}]: {1}", header.Key, header.Value.FirstOrDefault());
            }
            _log.Info("-----------------------------------------------------------------------");

            base.OnActionExecuting(actionContext);
        }

        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            Logger _log = LogManager.GetLogger("LogTracker", actionExecutedContext.ActionContext.ControllerContext.Controller.GetType());
            //_log.Info(
            //    "after call (req id: {0}, utctime: {1}, utcnow: {2})",
            //    actionExecutedContext.Request.Headers.GetValues("X-REQUEST-ID").FirstOrDefault(),
            //    actionExecutedContext.Request.Headers.GetValues("X-REQUEST-UTCTIME").FirstOrDefault(),
            //    DateTime.UtcNow);

            _log.Info(
                "after call (req id: {0}, utctime: {1}, execute-ms: {2})",
                LogTrackerContext.Current.RequestId,
                LogTrackerContext.Current.RequestStartTimeUTC,
                LogTrackerContext.Current.RequestExecutingTime.TotalMilliseconds);

            //actionExecutedContext.Response.Headers.Add(
            //    "X-REQUEST-ID",
            //    actionExecutedContext.Request.Headers.GetValues("X-REQUEST-ID").FirstOrDefault());
            //actionExecutedContext.Response.Headers.Add(
            //    "X-REQUEST-UTCTIME",
            //    actionExecutedContext.Request.Headers.GetValues("X-REQUEST-UTCTIME").FirstOrDefault());

            actionExecutedContext.Response.Headers.Add(
                LogTrackerContext._KEY_REQUEST_ID,
                LogTrackerContext.Current.RequestId);

            actionExecutedContext.Response.Headers.Add(
                LogTrackerContext._KEY_REQUEST_START_UTCTIME,
                LogTrackerContext.Current.RequestStartTimeUTC.ToString("u"));

            base.OnActionExecuted(actionExecutedContext);
        }
    }
}
