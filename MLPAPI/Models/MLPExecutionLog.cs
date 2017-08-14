using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MLPAPI.Models
{
    public class MLPExecutionLogger
    {
        public static void Error(string name, string message, Exception ex)
        {
            ILog log = LogManager.GetLogger(name);

            if (log.IsErrorEnabled)
            {
                log.Error(message, ex);
            }
        }

        public static void Error(string name, string message)
        {
            ILog log = LogManager.GetLogger(name);

            if (log.IsErrorEnabled)
            {
                log.Error(message);
            }
        }

        public static void Info(string name, string message)
        {
            ILog log = LogManager.GetLogger(name);

            if (log.IsInfoEnabled)
            {
                log.Info(message);
            }
        }

        public static void Warning(string name, string message)
        {

            ILog log = LogManager.GetLogger(name);

            if (log.IsWarnEnabled) {

                log.Warn(message);
            }

        }
    }
}