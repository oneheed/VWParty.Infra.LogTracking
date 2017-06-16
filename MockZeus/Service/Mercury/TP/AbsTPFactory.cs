using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using VWParty.Infra.LogTracking;

namespace MockZeus.Service.Mercury.TP
{
    [LogTracker]
    public abstract class AbsTPFactory
    {
        internal static LogTrackerLogger logger;

        static AbsTPFactory()
        {
            if (logger == null)
            {
                logger = new LogTrackerLogger(LogManager.GetCurrentClassLogger());
            }
        }
    }
}