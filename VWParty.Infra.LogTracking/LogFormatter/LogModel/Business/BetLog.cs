using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VWParty.Infra.LogTracking.LogFormatter.LogModel.Business
{
    public class BetLog: IBusinessLog
    {
        [Display(Name = "LogTime", Order = -100)]
        public DateTime LogTime { get; set; }
        [Display(Name = "BrandID", Order = -99)]
        public string BrandID { get; set; }
    }
}
