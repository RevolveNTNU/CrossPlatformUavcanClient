using ReactiveUI;
using RevolveUavcan.Dsdl;
using RevolveUavcan.Uavcan;
using CrossPlatformUavcanClient.CommunicationModules;
using System.Net;
using Avalonia.Controls;
using CrossPlatformUavcanClient.Views;
using System.Threading;
using Avalonia.Layout;
using System.Threading.Tasks;
using Avalonia;
using System.IO;
using System;
using Avalonia.Media.Imaging;

namespace CrossPlatformUavcanClient.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private SynchronizationContext _syncContext = SynchronizationContext.Current;

        private string dsdlPath;

        private string chosenIpAdress;
        private int chosenReadPort;
        private int chosenWritePort;

        public MainWindowViewModel()
        {
            var window = CreateSampleWindow();

            window.ShowInTaskbar = false;

            window.Show(MainWindow.Instance);
        }

        private async Task GetDsdlFolderPath()
        {
            OpenFolderDialog dialog = new OpenFolderDialog();

            dsdlPath = await dialog.ShowAsync(MainWindow.Instance);
        }

        private void InitAllModules()
        {
            var dsdlParser = new DsdlParser(dsdlPath);
            var rulesGenerator = new UavcanSerializationRulesGenerator(dsdlParser);



            // Revolve Analyze sends data to 1235, and reads from 1234
            var commModule = new UdpCommunication(IPAddress.Parse(chosenIpAdress), chosenReadPort, chosenWritePort);
            var frameStorage = new UavcanFrameStorage(null);
            frameStorage.RegisterOnDataEvent(commModule);
            rulesGenerator.Init();

            var uavcanParser = new UavcanParser(null, rulesGenerator, frameStorage);

            _syncContext.Send(x =>
            {
                MessageListViewModel = new MessageListViewModel(uavcanParser, commModule);
                ReceivedMessagesListViewModel = new ReceivedMessagesListViewModel(uavcanParser);
                LayerIndex = -1;
                this.RaisePropertyChanged(nameof(MessageListViewModel));
                this.RaisePropertyChanged(nameof(ReceivedMessagesListViewModel));
                this.RaisePropertyChanged(nameof(LayerIndex));
            }, null);
        }

        public MessageListViewModel MessageListViewModel { get; set; }
        public ReceivedMessagesListViewModel ReceivedMessagesListViewModel { get; set; }

        private int layerIndex = 1000;
        public int LayerIndex { get => layerIndex; set => layerIndex = value; }

        private Window CreateSampleWindow()
        {
            Button button;
            Button folderButton;

            var window = new Window
            {
                Height = 350,
                Width = 400,
                SystemDecorations = SystemDecorations.BorderOnly,
                ExtendClientAreaToDecorationsHint = true,
                ExtendClientAreaChromeHints = Avalonia.Platform.ExtendClientAreaChromeHints.NoChrome,
                ExtendClientAreaTitleBarHeightHint = -1,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                CanResize = false,
                Content = new StackPanel
                {
                    Spacing = 4,
                    Children =
                    {
                        new TextBlock { Text = "UAVCAN Client Setup", FontSize=25, HorizontalAlignment=HorizontalAlignment.Center, Margin=Thickness.Parse("5") },
                        new TextBlock { Text = "Dsdl Path:", Margin=Thickness.Parse("5") },
                        new TextBox { Text = "No path selected. Press browse!", Margin=Thickness.Parse("5") },
                        (folderButton = new Button
                        {
                            HorizontalAlignment = HorizontalAlignment.Center,
                            Content = "Browse"
                        }),

                        new TextBlock { Text = "Provide IP Address and read- and write ports:", Margin=Thickness.Parse("5") },
                        new TextBox { Watermark = "IP Address", Text = "127.0.0.1", Margin=Thickness.Parse("5") },
                        new TextBox { Watermark = "Read port", Text = 1235.ToString(), Margin=Thickness.Parse("5") },
                        new TextBox { Watermark = "Write port", Text = 1234.ToString(), Margin=Thickness.Parse("5") },
                        (button = new Button
                        {
                            HorizontalAlignment = HorizontalAlignment.Center,
                            Content = "Save settings"
                        }),
                    }
                },
            };

            button.Click += (_, __) =>
                {
                    if (System.IO.Directory.Exists(dsdlPath))
                    {
                        chosenIpAdress = ((window.Content as StackPanel).Children[5] as TextBox).Text;
                        chosenReadPort = int.Parse(((window.Content as StackPanel).Children[6] as TextBox).Text);
                        chosenWritePort = int.Parse(((window.Content as StackPanel).Children[7] as TextBox).Text);
                        InitAllModules();
                        window.Close();
                    }
                };

            folderButton.Click += async (_, __) =>
            {
                await GetDsdlFolderPath();
                ((window.Content as StackPanel).Children[2] as TextBox).Text = dsdlPath;
            };
            return window;
        }
    }
}
