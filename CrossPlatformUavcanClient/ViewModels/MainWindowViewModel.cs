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
                Height = 200,
                Width = 400,
                Content = new StackPanel
                {
                    Spacing = 4,
                    Children =
                    {
                        new TextBlock { Text = "DsdlPath" },
                        new TextBlock { Text = "No path selected. Press browse!" },
                        (folderButton = new Button
                        {
                            HorizontalAlignment = HorizontalAlignment.Center,
                            Content = "Browse"
                        }),
                        new TextBox { Watermark = "IpAdress" },
                        new TextBox { Watermark = "Read port",  },
                        new TextBox { Watermark = "Write port" },
                        (button = new Button
                        {
                            HorizontalAlignment = HorizontalAlignment.Center,
                            Content = "Save settings"
                        }),
                    }
                },
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            button.Click += (_, __) =>
            {
                chosenIpAdress = ((window.Content as StackPanel).Children[3] as TextBox).Text;
                chosenReadPort = int.Parse(((window.Content as StackPanel).Children[4] as TextBox).Text);
                chosenWritePort = int.Parse(((window.Content as StackPanel).Children[5] as TextBox).Text);
                InitAllModules();
                window.Close();
            };

            folderButton.Click += async (_, __) =>
            {
                await GetDsdlFolderPath();
                ((window.Content as StackPanel).Children[1] as TextBlock).Text = dsdlPath;
            };
            return window;
        }
    }


}
