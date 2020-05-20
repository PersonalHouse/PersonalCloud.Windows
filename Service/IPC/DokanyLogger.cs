using Microsoft.Extensions.Logging;

namespace NSPersonalCloud.WindowsService.IPC
{
    internal class DokanyLogger : DokanNet.Logging.ILogger
    {
        private ILogger Logger { get; }

        public DokanyLogger()
        {
            Logger = Globals.Loggers.CreateLogger<DokanyLogger>();
        }

        public void Debug(string message, params object[] args)
        {
            Logger.LogDebug(message, args);
        }

        public void Error(string message, params object[] args)
        {
            Logger.LogError(message, args);
        }

        public void Fatal(string message, params object[] args)
        {
            Logger.LogCritical(message, args);
        }

        public void Info(string message, params object[] args)
        {
            Logger.LogInformation(message, args);
        }

        public void Warn(string message, params object[] args)
        {
            Logger.LogWarning(message, args);
        }
    }
}
