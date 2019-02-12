using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace VWParty.Infra.LogTracking
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class LogTrackerAttribute : ActionFilterAttribute
    {
        public string Prefix { get; set; }

        private static LogTrackerLogger _logger = new LogTrackerLogger(LogManager.GetCurrentClassLogger());

        private readonly JsonSerializerSettings _jsonSettings = new JsonSerializerSettings()
        {
            Error = (serializer, err) => err.ErrorContext.Handled = true,
        };

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            Logger _log = LogManager.GetLogger("LogTracker");
            try
            {
                //
                // TODO: 後續版本必須移除。LogTrackerContext 不應該 "全自動" 的產生，必須有明確進入點才能建立新的 LogTrackerContext.
                //
                if (LogTrackerContext.Current == null)
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

                var arguments = GetArguments(actionContext.ActionArguments);
                string requestJson = JsonConvert.SerializeObject(arguments, _jsonSettings);

                _logger.Info(new LogMessage()
                {
                    Message = $"Before call {Prefix} {actionContext.ControllerContext.Request.RequestUri.AbsolutePath}",                    
                    ExtraData = new Dictionary<string, object>()
                    {
                        { "RequestId", LogTrackerContext.Current.RequestId },
                        { "RequestUri", actionContext.Request.RequestUri.AbsoluteUri },
                        { "RequestBody", actionContext.Request.Content.ReadAsStringAsync().Result},
                        { "ModelBindingRequest", requestJson},
                    }
                });
            }
            catch (Exception ex)
            {
                _log.Warn(ex);
            }
            base.OnActionExecuting(actionContext);
        }

        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            Logger _log = LogManager.GetLogger("LogTracker");
            try
            {
                var arguments = GetArguments(actionExecutedContext.ActionContext.ActionArguments);
                string requestJson = JsonConvert.SerializeObject(arguments, _jsonSettings);

                _logger.Info(new LogMessage()
                {
                    Message = $"After call {Prefix} {actionExecutedContext.ActionContext.ControllerContext.Request.RequestUri.AbsolutePath}",
                    ExtraData = new Dictionary<string, object>()
                    {
                        { "RequestId", LogTrackerContext.Current.RequestId },
                        { "RequestUri", actionExecutedContext.Request.RequestUri.AbsoluteUri },
                        { "RequestBody", actionExecutedContext.Request.Content.ReadAsStringAsync().Result},
                        { "ModelBindingRequest", requestJson},
                        { "ResponseBody", actionExecutedContext.Response.Content.ReadAsStringAsync().Result}
                    }
                });
                if (LogTrackerContext.Current != null)
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

        private IDictionary<string, object> GetArguments(IDictionary<string, object> actionArguments)
        {
            var arguments = new Dictionary<string, object>();

            foreach (var actionArgument in actionArguments)
            {
                if (actionArgument.Value is HttpRequestMessage)
                    arguments.Add(actionArgument.Key, (actionArgument.Value as HttpRequestMessage).Content.ReadAsStringAsync());
                else
                    arguments.Add(actionArgument.Key, actionArgument.Value);
            }

            return arguments;
        }
    }
}