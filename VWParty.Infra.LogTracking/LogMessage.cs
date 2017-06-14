using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VWParty.Infra.LogTracking
{
    public class LogMessage
    {
        public LogMessage()
        {
            CustomFields = new Dictionary<string, string>();
            ExtraData = new Dictionary<string, object>();
        }
        public LogTrackerContext logContext { get; set;}
        public string RequestId { get; set; }
        public string RequestStartTimeUTC { get; set; }
        public string ShortMessage { get; set; }
        public string Message { get; set; }
        public Exception Exception { get; set; }
        public IDictionary<string, string> CustomFields { get; set; }
        [Obsolete]
        public string Category { get; set; }
        [Obsolete]
        public string Level { get; set; }
        [Obsolete]
        public string Project { get; set; }
        [Obsolete("Please use RequestID")]
        public string TID
        {
            get { return RequestId; }
            set { RequestId = value; }
        }
        [Obsolete()]
        public string FuncName { get; set; }
        [Obsolete]
        public string ProcessName { get; set; }
        [Obsolete]
        public IDictionary<string, object> ExtraData { get; set; }
    }
}
