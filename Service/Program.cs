using System;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

using NSPersonalCloud.WindowsContract;

using Topshelf;

namespace NSPersonalCloud.WindowsService
{
    internal class Program
    {
        [System.Runtime.InteropServices.DllImport("kernel32.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode, SetLastError = true)]
        [return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
        internal static extern bool SetDllDirectory(string lpPathName);

        public static void Main(string[] args)
        {

            var logsDir = Path.Combine(Globals.ConfigurationPath, "Logs");
            Directory.CreateDirectory(logsDir);

            if (args == null || args.Length == 0)
            {
                Globals.Loggers = LoggerFactory.Create(builder => builder.//SetMinimumLevel(LogLevel.Trace).
                AddConsole(x => {
                    x.TimestampFormat = "G";
                }).AddFile(Path.Combine(logsDir, "Service.log"),/*LogLevel.Trace, */fileSizeLimitBytes: 6291456, retainedFileCountLimit: 3));
            }
            else
            {
                Globals.Loggers = LoggerFactory.Create(builder => builder.//SetMinimumLevel(LogLevel.Trace).
                 AddFile(Path.Combine(logsDir, "Service.log"), /*LogLevel.Trace,*/ fileSizeLimitBytes: 6291456, retainedFileCountLimit: 3));
            }

            var l = Globals.Loggers.CreateLogger<Program>();
            l.LogInformation("Personal Cloud service started.");

            #region Library Architecture

            // Load DLL according to process architecture.
            var appPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var is64Bit = Environment.Is64BitOperatingSystem
                          // Comment out ONLY the next line to use CPU architecture.
                          && Environment.Is64BitProcess
                          ;
            var libFolder = !is64Bit ? "x86" : "x64";
            SetDllDirectory(Path.Combine(appPath, libFolder));

            #endregion Library Architecture


            var rc = HostFactory.Run(x => {
                x.Service<PersonalCloudWindowsService>();
                x.RunAsLocalSystem();

                x.SetServiceName(Services.ServiceName);
                x.SetDescription("Personal Cloud Service is responsible for managing Personal Cloud and related network drives in your local network.");
                x.SetDisplayName("Personal Cloud");

                x.EnableServiceRecovery(service => service.RestartService(1));
                x.StartAutomaticallyDelayed();
            });

            var exitCode = (int) Convert.ChangeType(rc, rc.GetTypeCode());
            Environment.ExitCode = exitCode;

            l.LogInformation($"Personal Cloud service exit with {exitCode}.");
            Globals.Loggers?.Dispose();
        }
    }
}
