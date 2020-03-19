using System.Threading;
using System.Windows;

using Hardcodet.Wpf.TaskbarNotification;

using JKang.IpcServiceFramework;

using Microsoft.Extensions.DependencyInjection;

using Unishare.Apps.WindowsConfigurator.IPC;
using Unishare.Apps.WindowsContract;

namespace Unishare.Apps.WindowsConfigurator
{
#pragma warning disable CA1001 // Objects are disposed at app shutdown.

    public partial class App : Application
#pragma warning restore CA1001
    {
        public TaskbarIcon TrayIcon { get; private set; }

        private CancellationTokenSource runners;

        private void OnApplicationStarted(object sender, StartupEventArgs e)
        {
            TrayIcon = (TaskbarIcon) FindResource("TrayIcon");

            runners = new CancellationTokenSource();

            var ipcServiceCollection = new ServiceCollection()
                .AddSingleton<PopupPresenter>()
                .AddSingleton<NotificationCenter>()
                .AddIpc(builder => {
                    builder.AddNamedPipe()
                           .AddService<IPopupPresenter>(services => services.GetRequiredService<PopupPresenter>())
                           .AddService<ICloudEventHandler>(services => services.GetRequiredService<NotificationCenter>());
                });
            _ = new IpcServiceHostBuilder(ipcServiceCollection.BuildServiceProvider())
                .AddNamedPipeEndpoint<IPopupPresenter>("Popups", Pipes.Messenger, true)
                .AddNamedPipeEndpoint<ICloudEventHandler>("Notifications", Pipes.NotificationCenter, true)
                .Build().RunAsync(runners.Token);

            Globals.CloudManager = new IpcServiceClientBuilder<ICloudManager>().UseNamedPipe(Pipes.CloudAdmin).Build();
            Globals.Mounter = new IpcServiceClientBuilder<IFileSystemController>().UseNamedPipe(Pipes.DiskMounter).Build();
            Globals.Storage = new IpcServiceClientBuilder<IPersistentStorage>().UseNamedPipe(Pipes.UserSettings).Build();

            /*
            Task.Run(async () => {
                var cloud = await Globals.Storage.InvokeAsync(x => x.GetAllPersonalCloud()).ConfigureAwait(false);
                Globals.PersonalCloud = cloud.Length == 0 ? null : (Guid?) cloud[0];

                Dispatcher.Invoke(() => {
                    if (Globals.PersonalCloud != null) MainWindow = new MainWindow();
                    else MainWindow = new WelcomeWindow();
                });
            });
            */
        }

        private void OnApplicationExit(object sender, ExitEventArgs e)
        {
            runners.Cancel();
            runners.Dispose();

            TrayIcon.Dispose();
        }
    }
}
