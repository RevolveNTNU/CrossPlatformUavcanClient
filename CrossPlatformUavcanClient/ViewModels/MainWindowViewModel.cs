using RevolveUavcan.Uavcan.Interfaces;

namespace CrossPlatformUavcanClient.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public string Greeting => "Welcome to Avalonia! Lets get this party started!";

        public MainWindowViewModel(IUavcanSerializationGenerator uavcanSerializationGenerator)
        {
            MessageListViewModel = new MessageListViewModel(uavcanSerializationGenerator);
        }

        public MessageListViewModel MessageListViewModel { get; }
    }
}
