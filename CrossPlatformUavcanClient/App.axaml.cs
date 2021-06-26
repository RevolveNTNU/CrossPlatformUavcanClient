using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using CrossPlatformUavcanClient.CommunicationModules;
using CrossPlatformUavcanClient.ViewModels;
using CrossPlatformUavcanClient.Views;
using RevolveUavcan.Dsdl;
using RevolveUavcan.Uavcan;
using System.Net;

namespace CrossPlatformUavcanClient
{
    public class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var dsdlParser = new DsdlParser(@"C:\Users\Larsv\Revolve_Analyze\dsdl\revolve_dsdl");
                var rulesGenerator = new UavcanSerializationRulesGenerator(dsdlParser);

                // Revolve Analyze sends data to 1235, and reads from 1234
                var udpComm = new UdpCommunication(IPAddress.Loopback, 1235);
                var frameStorage = new UavcanFrameStorage(null);
                frameStorage.RegisterOnDataEvent(udpComm);

                rulesGenerator.Init();

                var uavcanParser = new UavcanParser(null, rulesGenerator, frameStorage);


                desktop.MainWindow = new MainWindow
                {
                    DataContext = new MainWindowViewModel(rulesGenerator),
                };
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}
