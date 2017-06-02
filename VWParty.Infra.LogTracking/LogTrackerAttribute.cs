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
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            try
            {
                Logger _log = LogManager.GetLogger("LogTracker", actionContext.ControllerContext.Controller.GetType());
                if (LogTrackerContext.Current == null)
                {
                    _log.Info("create req id");
                    LogTrackerContext.Init(LogTrackerContextStorageTypeEnum.ASPNET_HTTPCONTEXT);
                }
                _log.Info(string.Format(
                    "before call (req id: {0}, utctime: {1}, execute-ms: {2})",
                    LogTrackerContext.Current.RequestId,
                    LogTrackerContext.Current.RequestStartTimeUTC,
                    LogTrackerContext.Current.RequestExecutingTime.TotalMilliseconds));
            }
            catch(Exception ex)
            {
                _logger.Warn(ex);
            }
            base.OnActionExecuting(actionContext);
        }

        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            try { 
                Logger _log = LogManager.GetLogger("LogTracker", actionExecutedContext.ActionContext.ControllerContext.Controller.GetType());

                _log.Info(string.Format(
                    "after call (req id: {0}, utctime: {1}, execute-ms: {2})",
                    LogTrackerContext.Current.RequestId,
                    LogTrackerContext.Current.RequestStartTimeUTC,
                    LogTrackerContext.Current.RequestExecutingTime.TotalMilliseconds));

                actionExecutedContext.Response.Headers.Add(
                    LogTrackerContext._KEY_REQUEST_ID,
                    LogTrackerContext.Current.RequestId);

                actionExecutedContext.Response.Headers.Add(
                    LogTrackerContext._KEY_REQUEST_START_UTCTIME,
                    LogTrackerContext.Current.RequestStartTimeUTC.ToString("u"));
            }
            catch (Exception ex)
            {
                _logger.Warn(ex);
            }

            base.OnActionExecuted(actionExecutedContext);
        }
    }
}
