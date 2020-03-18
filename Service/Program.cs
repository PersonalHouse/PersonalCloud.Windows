using System;
using System.Drawing;

using Pastel;

using Topshelf;

namespace Unishare.Apps.WindowsService
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Playground started.".Pastel(Color.LawnGreen));

            var rc = HostFactory.Run(x => {
                x.Service<PersonalCloudWindowsService>();
                x.RunAsLocalSystem();

                x.SetServiceName("PersonalCloud.Playground");
                x.SetDescription("Personal Cloud Playground Service");
                x.SetDisplayName("Personal Cloud Playground");
            });

            var exitCode = (int) Convert.ChangeType(rc, rc.GetTypeCode());
            Environment.ExitCode = exitCode;
        }
    }
}
