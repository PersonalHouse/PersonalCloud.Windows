using System;
using System.IO;
using System.Reflection;

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
            Console.WriteLine("Personal Cloud service started.");

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

                x.SetServiceName("PersonalCloud.Apps.WindowsService");
                x.SetDescription("Personal Cloud Service is responsible for managing Personal Cloud and related network drives.");
                x.SetDisplayName("Personal Cloud");

                x.EnableServiceRecovery(service => service.RestartService(1));
                x.StartAutomaticallyDelayed();
            });

            var exitCode = (int) Convert.ChangeType(rc, rc.GetTypeCode());
            Environment.ExitCode = exitCode;
        }
    }
}
