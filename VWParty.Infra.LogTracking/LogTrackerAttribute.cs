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
        public string Prefix { get; set; }


        private static LogTrackerLogger _logger = new LogTrackerLogger(LogManager.GetCurrentClassLogger());

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            Logger _log = LogManager.GetLogger("LogTracker", actionContext.ControllerContext.Controller.GetType());
            try
            {
                //
                // TODO: 後續版本必須移除。LogTrackerContext 不應該 "全自動" 的產生，必須有明確進入點才能建立新的 LogTrackerContext.
                //
                if (LogTrackerContext.IsEmptyOrNull(LogTrackerContext.Current))
                {
                    _log.Info("creating request_id and request_start_time_utc.");
                    {
                        if (string.IsNullOrEmpty(this.Prefix))
                        {
                            LogTrackerContext.Create("TEMP", LogTrackerContextStorageTypeEnum.ASPNET_HTTPCONTEXT);
                        }
                        else
                        {
                            LogTrackerContext.Create(this.Prefix, LogTrackerContextStorageTypeEnum.ASPNET_HTTPCONTEXT);
                        }
                    }
                    _log.Info("request_id and request_start_time_utc created.");
                }


                //_log.Info(string.Format(
                //    "Before call (request_id: {0}, request_start_time_utc: {1}, request_execute_time_ms: {2})",
                //    LogTrackerContext.Current.RequestId,
                //    LogTrackerContext.Current.RequestStartTimeUTC,
                //    LogTrackerContext.Current.RequestExecutingTime.TotalMilliseconds));
                _logger.Info(new LogMessage()
                {
                    Message = "Before call " + actionContext.ActionDescriptor.ControllerDescriptor.ControllerName + "/" +
                              actionContext.ActionDescriptor.ActionName,
                    ExtraData = new Dictionary<string, object>()
                    {
                        { "url", actionContext.Request.RequestUri.AbsoluteUri }
                    }
                });
            }
            catch(Exception ex)
            {
                _log.Warn(ex);
            }
            base.OnActionExecuting(actionContext);
        }

        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            Logger _log = LogManager.GetLogger("LogTracker", actionExecutedContext.ActionContext.ControllerContext.Controller.GetType());
            try
            { 
                //_log.Info(string.Format(
                //    "After call (request_id: {0}, request_start_time_utc: {1}, request_execute_time_ms: {2})",
                //    LogTrackerContext.Current.RequestId,
                //    LogTrackerContext.Current.RequestStartTimeUTC,
                //    LogTrackerContext.Current.RequestExecutingTime.TotalMilliseconds));
                _logger.Info(new LogMessage()
                {
                    Message = "After call " + actionExecutedContext.ActionContext.ControllerContext.ControllerDescriptor.ControllerName + "/" +
                              actionExecutedContext.ActionContext.ActionDescriptor.ActionName,
                    ExtraData = new Dictionary<string, object>()
                    {
                        { "url", actionExecutedContext.Request.RequestUri.AbsoluteUri }
                    }
                });
                if (!LogTrackerContext.IsEmptyOrNull(LogTrackerContext.Current))
                {
                    actionExecutedContext.Response.Headers.Add(
                        LogTrackerContext._KEY_REQUEST_ID,
                        LogTrackerContext.Current.RequestId);

                    actionExecutedContext.Response.Headers.Add(
                        LogTrackerContext._KEY_REQUEST_START_UTCTIME,
                        LogTrackerContext.Current.RequestStartTimeUTC_Text);
                }
            }
            catch (Exception ex)
            {
                _log.Warn(ex);
            }

            base.OnActionExecuted(actionExecutedContext);
        }
    }
}
