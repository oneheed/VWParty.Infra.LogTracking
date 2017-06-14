using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NLog;
using NLog.Targets;


namespace VWVWParty.Infra.LogTracking.Targets
{
    [Target("GelfTcp")]
    public sealed class GelfTcpTarget : TargetWithLayout
    {
        private TcpClient _tcpClient;
        private FixedSizedQueue<string> _queue;
        private readonly string _hostName;

        public GelfTcpTarget()
        {
            _hostName = Environment.MachineName;
            _tcpClient = new TcpClient();

            Facility = "gelf";
            RemoteAddress = "127.0.0.1";
            RemotePort = 12201;
            MaxBufferedMessage = 30000;
        }

        public string Facility { get; set; }

        public int MaxBufferedMessage { get; set; }

        public string RemoteAddress { get; set; }

        public int RemotePort { get; set; }

        protected override void InitializeTarget()
        {
            base.InitializeTarget();
            _queue = new FixedSizedQueue<string>(MaxBufferedMessage);
            SendMessage();
        }

        protected override void Write(LogEventInfo logEvent)
        {
            try
            {
                _queue.Enqueue(CreateGelfJsonFromLoggingEvent(logEvent));
            }
            catch
            {
                // ignored
            }
        }


        private void SendMessage()
        {
            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    if (_queue.IsEmpty)
                    {
                        Thread.Sleep(3000);
                        continue;
                    }

                    if (_tcpClient == null)
                    {
                        _tcpClient = new TcpClient();
                    }

                    if (_tcpClient != null && _tcpClient.Connected == false)
                    {
                        try
                        {
                            _tcpClient.Connect(RemoteAddress, RemotePort);
                        }
                        catch
                        {
                            if (_tcpClient != null)
                            {
                                _tcpClient.Close();
                                _tcpClient.Dispose();
                                _tcpClient = null;
                            }
                            Thread.Sleep(3000);
                            continue;
                        }
                    }

                    string message = string.Empty;
                    _queue.TryDequeue(out message);

                    if (string.IsNullOrWhiteSpace(message))
                    {
                        continue;
                    }

                    var netStream = _tcpClient.GetStream();
                    try
                    {
                        if (netStream.CanWrite)
                        {
                            var msgArray = Encoding.UTF8.GetBytes(message);
                            var payload = new byte[msgArray.Length + 1];
                            msgArray.CopyTo(payload, 0);
                            payload[msgArray.Length] = new byte();

                            netStream.Write(payload, 0, payload.Length);
                        }
                    }
                    catch
                    {
                        continue;
                    }
                }
            });

        }


        private string CreateGelfJsonFromLoggingEvent(LogEventInfo logEventInfo)
        {
            var message = new Dictionary<string, object>();
            message.Add("version", "1.1");
            message.Add("host", _hostName);
            message.Add("level", ToGelfLevel(logEventInfo.Level));
            var shortMessage = logEventInfo.Message.Length > 200 ? logEventInfo.Message.Substring(0, 200 - 1) : logEventInfo.Message;
            message.Add("short_message", shortMessage);
            message.Add("full_message", logEventInfo.Message);
            var duration = logEventInfo.TimeStamp.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            message.Add("timestamp", duration);
            message.Add("_facility", Facility);
            message.Add("_logger_name", logEventInfo.LoggerName);


            foreach (var property in logEventInfo.Properties)
            {
                if (property.Key != null && property.Value != null)
                {
                    string key = property.Key.ToString();
                    string val = property.Value.ToString();

                    if (key == "CallerFilePath" || key == "CallerLineNumber" || key == "CallerMemberName")
                    {
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(key) == false && string.IsNullOrWhiteSpace(val) == false)
                    {
                        message.Add("_" + property.Key.ToString(), property.Value.ToString());
                    }
                }
            }

            var exceptioToLog = logEventInfo.Exception;
            if (exceptioToLog != null)
            {
                while (exceptioToLog.InnerException != null)
                {
                    exceptioToLog = exceptioToLog.InnerException;
                }

                message.Add("exception_message", logEventInfo.Exception.Message);
                message.Add("exception_stacktrace", logEventInfo.Exception.StackTrace);
            }

            return JsonConvert.SerializeObject(message);
        }


        private static int ToGelfLevel(LogLevel level)
        {
            if (level == LogLevel.Debug)
                return 7;
            if (level == LogLevel.Fatal)
                return 2;
            if (level == LogLevel.Info)
                return 6;
            if (level == LogLevel.Trace)
                return 7;
            return level == LogLevel.Warn ? 4 : 3;
        }

    }
}