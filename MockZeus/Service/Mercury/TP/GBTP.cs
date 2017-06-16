using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using VWParty.Infra.LogTracking;

namespace MockZeus.Service.Mercury.TP
{
    public class GBTP : AbsTPFactory
    {
        public void GetBalance()
        {
            logger.Info(new LogMessage()
            {
                Message = "MockZeus Test Message from GBTP.GetBalance()"
            });
        }
    }
}