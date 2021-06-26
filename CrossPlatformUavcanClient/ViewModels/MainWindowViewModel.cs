using RevolveUavcan.Uavcan.Interfaces;
using RevolveUavcan.Communication;

namespace CrossPlatformUavcanClient.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public MainWindowViewModel(IUavcanCommunicationModule commModule, IUavcanParser uavcanParser)
        {
            MessageListViewModel = new MessageListViewModel(uavcanParser, commModule);
            ReceivedMessagesListViewModel = new ReceivedMessagesListViewModel(uavcanParser);
        }

        public MessageListViewModel MessageListViewModel { get; }
        public ReceivedMessagesListViewModel ReceivedMessagesListViewModel { get; }
    }
}
