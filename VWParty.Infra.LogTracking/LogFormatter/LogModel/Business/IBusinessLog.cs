using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace VWParty.Infra.LogTracking.LogFormatter.LogModel.Business
{
    public interface IBusinessLog
    {
        string Event { get; set; }

        string ToJsonString();
    }
}
