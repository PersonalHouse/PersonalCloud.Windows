using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;

namespace NSPersonalCloud.WindowsService.IPC
{
    class DokanyLogger: DokanNet.Logging.ILogger
    {
        ILogger logger;
        public DokanyLogger(ILogger l)
        {
            logger = l;
        }

        public void Debug(string message, params object[] args)
        {
            logger.LogDebug(message, args);
        }

        public void Error(string message, params object[] args)
        {
            logger.LogError(message, args);
        }

        public void Fatal(string message, params object[] args)
        {
            logger.LogError(message, args);
        }

        public void Info(string message, params object[] args)
        {
            logger.LogInformation(message, args);
        }

        public void Warn(string message, params object[] args)
        {
            logger.LogWarning(message, args);
        }
    }
}
