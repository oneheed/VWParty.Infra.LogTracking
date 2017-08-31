using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VWParty.Infra.LogTracking.LogFormatter.LogModel.Business
{
    class CancelBetLog : IBusinessLog
    {
        public string Event { get; set; }

        public string ToJsonString()
        {
            string jsonString = String.Empty;

            // TODO serialize to json string

            return jsonString;
        }
    }
}
